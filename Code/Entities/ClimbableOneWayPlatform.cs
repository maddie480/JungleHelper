using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.JungleHelper.Entities
{
    [CustomEntity("JungleHelper/ClimbableOneWayPlatform")]
    [Tracked]
    public class ClimbableOneWayPlatform : Entity
    {
        private static ILHook hookOnUpdateSprite;

        private static FieldInfo actorMovementCounter = typeof(Actor).GetField("movementCounter", BindingFlags.Instance | BindingFlags.NonPublic);

        private static bool hooksActive = false;

        public static void Load()
        {
            On.Celeste.LevelLoader.ctor += onLevelLoad;
            On.Celeste.OverworldLoader.ctor += onOverworldLoad;
        }

        public static void Unload()
        {
            On.Celeste.LevelLoader.ctor -= onLevelLoad;
            On.Celeste.OverworldLoader.ctor -= onOverworldLoad;
            deactivateHooks();
        }

        private static void onLevelLoad(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startPosition)
        {
            orig(self, session, startPosition);

            if (session.MapData?.Levels?.Any(level => level.Entities?.Any(entity => entity.Name == "JungleHelper/ClimbableOneWayPlatform") ?? false) ?? false)
            {
                activateHooks();
            }
            else
            {
                deactivateHooks();
            }
        }

        private static void onOverworldLoad(On.Celeste.OverworldLoader.orig_ctor orig, OverworldLoader self, Overworld.StartMode startMode, HiresSnow snow)
        {
            orig(self, startMode, snow);

            if (startMode != (Overworld.StartMode)(-1))
            { // -1 = in-game overworld from the collab utils
                deactivateHooks();
            }
        }

        public static void activateHooks()
        {
            if (hooksActive)
            {
                return;
            }
            hooksActive = true;

            Logger.Log(LogLevel.Info, "JungleHelper/ClimbableOneWayPlatform", "=== Activating climbable one-way platform hooks");

            // implement the basic collision between actors/platforms and sideways jumpthrus.
            IL.Celeste.Actor.MoveHExact += addSidewaysJumpthrusInHorizontalMoveMethods;

            // block "climb hopping" on top of climbable one-way platforms, because this just looks weird.
            On.Celeste.Player.ClimbHopBlockedCheck += onPlayerClimbHopBlockedCheck;

            // mod collide checks to include climbable one-way platforms, so that the player behaves with them like with walls.
            IL.Celeste.Player.ClimbCheck += modCollideChecks; // allow player to climb on them
            IL.Celeste.Player.ClimbBegin += modCollideChecks; // if not applied, the player will clip through jumpthrus if trying to climb on them
            IL.Celeste.Player.ClimbUpdate += modCollideChecks; // when climbing, jumpthrus are handled like walls
            IL.Celeste.Player.SlipCheck += modCollideChecks; // make climbing on jumpthrus not slippery
            IL.Celeste.Player.OnCollideH += modCollideChecks; // handle dashes against jumpthrus properly, without "shifting" down

            // have the push animation when Madeline runs against a jumpthru for example
            hookOnUpdateSprite = new ILHook(typeof(Player).GetMethod("orig_UpdateSprite", BindingFlags.NonPublic | BindingFlags.Instance), modCollideChecks);

            // make the player able to grab the climbable one-way even when going up.
            IL.Celeste.Player.NormalUpdate += modPlayerNormalUpdate;
            On.Celeste.Player.ClimbJump += modPlayerClimbJump;

            On.Celeste.SurfaceIndex.GetPlatformByPriority += modSurfaceIndexGetPlatformByPriority;

            //customization hooks
            On.Celeste.Player.ClimbBegin += JackalMomentum_modPlayerClimbBegin;
            On.Celeste.Player.ClimbUpdate += JackalMomentum_modPlayerClimbUpdate;
        }

        public static void deactivateHooks()
        {
            if (!hooksActive)
            {
                return;
            }
            hooksActive = false;

            Logger.Log(LogLevel.Info, "JungleHelper/ClimbableOneWayPlatform", "=== Deactivating one-way platform hooks");

            IL.Celeste.Actor.MoveHExact -= addSidewaysJumpthrusInHorizontalMoveMethods;

            On.Celeste.Player.ClimbHopBlockedCheck -= onPlayerClimbHopBlockedCheck;

            IL.Celeste.Player.ClimbCheck -= modCollideChecks;
            IL.Celeste.Player.ClimbBegin -= modCollideChecks;
            IL.Celeste.Player.ClimbUpdate -= modCollideChecks;
            IL.Celeste.Player.SlipCheck -= modCollideChecks;
            IL.Celeste.Player.OnCollideH -= modCollideChecks;

            hookOnUpdateSprite?.Dispose();

            IL.Celeste.Player.NormalUpdate -= modPlayerNormalUpdate;
            On.Celeste.Player.ClimbJump -= modPlayerClimbJump;

            On.Celeste.SurfaceIndex.GetPlatformByPriority -= modSurfaceIndexGetPlatformByPriority;

            //customization hooks
            On.Celeste.Player.ClimbBegin -= JackalMomentum_modPlayerClimbBegin;
            On.Celeste.Player.ClimbUpdate -= JackalMomentum_modPlayerClimbUpdate;
        }

        private static void addSidewaysJumpthrusInHorizontalMoveMethods(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall<Entity>("CollideFirst"))
                 && cursor.TryGotoNext(instr => instr.OpCode == OpCodes.Brfalse_S || instr.OpCode == OpCodes.Brtrue_S))
            {

                Logger.Log("JungleHelper/ClimbableOneWayPlatform", $"Injecting sideways jumpthru check at {cursor.Index} in IL for {il.Method.Name}");
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.EmitDelegate<Func<Solid, Actor, int, Solid>>((orig, self, moveH) => {
                    if (orig != null)
                        return orig;

                    int moveDirection = Math.Sign(moveH);
                    bool movingLeftToRight = moveH > 0;
                    if (checkCollisionWithSidewaysMovingPlatformsWhileMoving(self, moveDirection, movingLeftToRight))
                    {
                        return new Solid(Vector2.Zero, 0, 0, false); // what matters is that it is non null.
                    }

                    return null;
                });
            }
        }

        private static bool checkCollisionWithSidewaysMovingPlatformsWhileMoving(Actor self, int moveDirection, bool movingLeftToRight)
        {
            ClimbableOneWayPlatform climbablePlatform = collideFirstOutside(self, self.Position + Vector2.UnitX * moveDirection, !movingLeftToRight);
            if (climbablePlatform != null && climbablePlatform.stamBehavior == "none")
            {
                return Input.Grab.Check && self is Player player && player.Stamina >= 20f && climbablePlatform != null && climbablePlatform.climbJumpGrabCooldown <= 0f;
            }
            return Input.Grab.Check && self is Player && climbablePlatform != null && climbablePlatform.climbJumpGrabCooldown <= 0f;
        }

        private static bool onPlayerClimbHopBlockedCheck(On.Celeste.Player.orig_ClimbHopBlockedCheck orig, Player self)
        {
            bool vanillaCheck = orig(self);
            if (vanillaCheck)
                return vanillaCheck;

            // block climb hops on jumpthrus because those look weird
            return self.CollideCheckOutside<ClimbableOneWayPlatform>(self.Position + Vector2.UnitX * (int)self.Facing);
        }

        private static void modCollideChecks(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            // create a Vector2 temporary variable
            VariableDefinition checkAtPositionStore = new VariableDefinition(il.Import(typeof(Vector2)));
            il.Body.Variables.Add(checkAtPositionStore);

            bool isClimb = il.Method.Name.Contains("Climb");

            while (cursor.Next != null)
            {
                Instruction next = cursor.Next;

                // we want to replace all CollideChecks with solids here.
                if (next.OpCode == OpCodes.Call && (next.Operand as MethodReference)?.FullName == "System.Boolean Monocle.Entity::CollideCheck<Celeste.Solid>(Microsoft.Xna.Framework.Vector2)")
                {
                    Logger.Log("JungleHelper/ClimbableOneWayPlatform", $"Patching Entity.CollideCheck to include climbable one-ways at {cursor.Index} in IL for {il.Method.Name}");

                    callOrigMethodKeepingEverythingOnStack(cursor, checkAtPositionStore, isSceneCollideCheck: false);

                    cursor.EmitDelegate<Func<bool, Entity, Vector2, bool>>((orig, self, checkAtPosition) => {
                        // we still want to check for solids...
                        if (orig) return true;

                        // if we are not checking a side, this certainly has nothing to do with jumpthrus.
                        if (self.Position.X == checkAtPosition.X)
                            return false;

                        // our entity also collides if this is with a jumpthru and we are colliding with the solid side of it.
                        // we are in this case if the jumpthru is left to right (the "solid" side of it is the right one) 
                        // and we are checking the collision on the left side of the player for example.
                        bool collideOnLeftSideOfPlayer = (self.Position.X > checkAtPosition.X);
                        ClimbableOneWayPlatform oneway = collideFirstOutside(self, checkAtPosition, collideOnLeftSideOfPlayer);
                        return oneway != null && self is Player player
                            && oneway.Bottom >= self.Top + checkAtPosition.Y - self.Position.Y + 3;
                    });
                }

                if (next.OpCode == OpCodes.Callvirt && (next.Operand as MethodReference)?.FullName == "System.Boolean Monocle.Scene::CollideCheck<Celeste.Solid>(Microsoft.Xna.Framework.Vector2)")
                {
                    Logger.Log("JungleHelper/ClimbableOneWayPlatform", $"Patching Scene.CollideCheck to include climbable one-ways at {cursor.Index} in IL for {il.Method.Name}");

                    callOrigMethodKeepingEverythingOnStack(cursor, checkAtPositionStore, isSceneCollideCheck: true);

                    cursor.EmitDelegate<Func<bool, Scene, Vector2, bool>>((orig, self, vector) => orig || self.CollideCheck<ClimbableOneWayPlatform>(vector));
                }

                cursor.Index++;
            }
        }

        private static void callOrigMethodKeepingEverythingOnStack(ILCursor cursor, VariableDefinition checkAtPositionStore, bool isSceneCollideCheck)
        {
            // store the position in the local variable
            cursor.Emit(OpCodes.Stloc, checkAtPositionStore);
            cursor.Emit(OpCodes.Ldloc, checkAtPositionStore);

            // let vanilla call CollideCheck
            cursor.Index++;

            // reload the parameters
            cursor.Emit(OpCodes.Ldarg_0);
            if (isSceneCollideCheck)
            {
                cursor.Emit(OpCodes.Call, typeof(Entity).GetProperty("Scene").GetGetMethod());
            }

            cursor.Emit(OpCodes.Ldloc, checkAtPositionStore);
        }

        private static void modPlayerNormalUpdate(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            // let's jump to if (this.Speed.Y < 0f) => "is the player going up? if so, they can't grab!"
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdsfld(typeof(Input), "Grab")) &&
                cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchLdarg(0), // this
                instr => instr.MatchLdflda<Player>("Speed"),
                instr => instr.MatchLdfld<Vector2>("Y"),
                instr => instr.MatchLdcR4(0f),
                instr => instr.OpCode == OpCodes.Blt_Un || instr.OpCode == OpCodes.Blt_Un_S))
            {

                Instruction afterCheck = cursor.Next;

                // step back before the "Speed.Y < 0f" check (more specifically, inside it. it would be skipped otherwise)
                cursor.Index -= 4;

                Logger.Log("JungleHelper/ClimbableOneWayPlatform", $"Injecting code to be able to grab climbable one-ways when going up at {cursor.Index} in IL code for Player.NormalUpdate");

                // inject ourselves to jump over the "Speed.Y < 0f" check, and put this back
                cursor.EmitDelegate<Func<Player, bool>>(self => {
                    ClimbableOneWayPlatform platform = collideFirstOutside(self, self.Position + new Vector2((int)self.Facing * 2, 0), self.Facing == Facings.Left);
                    return platform != null && platform.climbJumpGrabCooldown <= 0f;
                });
                cursor.Emit(OpCodes.Brtrue, afterCheck);
                cursor.Emit(OpCodes.Ldarg_0);
            }
        }

        //grabs the speed and stamina if applicable at the earliest poossible time
        private static void JackalMomentum_modPlayerClimbBegin(On.Celeste.Player.orig_ClimbBegin orig, Player self)
        {
            ClimbableOneWayPlatform platform = collideFirstOutside(self, self.Position + new Vector2((int)self.Facing * 2, 0), self.Facing == Facings.Left);
            if (platform != null)
            {

                if (platform.hasMomentumCarrying)
                {
                    platform.initialSpeed = self.Speed.X;
                    platform.startTimer();
                    if(Math.Sign(Input.Aim.Value.X) == (self.Facing == Facings.Right ? 1 : -1)) {
                        self.Speed.X += platform.bonusSpeed(platform.initialSpeed, platform.grabTimer);
                    }
                }
                if (platform.initialStamina >= 0f)
                {
                    platform.initialStamina = self.Stamina;
                }

                if (platform.sameDirBoost && (Input.Jump.Pressed || Input.Jump)) {
                    if (Math.Abs(self.Speed.X) < 240f && Math.Sign(Input.Aim.Value.X) == (self.Facing == Facings.Right ? 1 : -1)) { 
                        self.Speed.X += (85f * (1 - Math.Abs(self.Speed.X) / 240f)) * (self.Facing == Facings.Right ? 1 : -1);
                        self.Speed.Y -= 15f;
                    }
                }
            }

            orig.Invoke(self);
        }

        private static int JackalMomentum_modPlayerClimbUpdate(On.Celeste.Player.orig_ClimbUpdate orig, Player self)
        {

            ClimbableOneWayPlatform platform = collideFirstOutside(self, self.Position + new Vector2((int)self.Facing * 2, 0), self.Facing == Facings.Left);
            if (platform != null)
            {
                //stamina management
                if (platform.stamBehavior == "regain")
                    self.RefillStamina();
                else if (platform.initialStamina > 0f && platform.stamStored)
                    self.Stamina = platform.initialStamina;
            }
            return orig.Invoke(self);
        }

        private static void modPlayerClimbJump(On.Celeste.Player.orig_ClimbJump orig, Player self)
        {
            orig(self);

            ClimbableOneWayPlatform platform = collideFirstOutside(self, self.Position + new Vector2((int)self.Facing * 2, 0), self.Facing == Facings.Left);
            if (platform != null)
            {
                // trigger the cooldown
                platform.climbJumpGrabCooldown = 0.35f;

                //stamina management
                if (platform.stamBehavior == "regain")
                    self.RefillStamina();
                else if (platform.initialStamina > 0f && platform.stamStored)
                    self.Stamina = platform.initialStamina;

                //momentum jumping, similar to a corner boost
                if (platform.hasMomentumCarrying && Math.Sign(Input.Aim.Value.X) == (self.Facing == Facings.Right ? 1 : -1))
                {
                    self.Speed.X += platform.bonusSpeed(platform.initialSpeed, platform.grabTimer);
                }

                //provides bonus momentum to make jumping from each side the same strength, after momentum jump so they do not stack
                if (platform.sameDirBoost)
                {
                    if (Math.Abs(self.Speed.X) < 240f && Math.Sign(Input.Aim.Value.X) == (self.Facing == Facings.Right ? 1 : -1))
                    {
                        self.Speed.X += (85f * (1 - Math.Abs(self.Speed.X) / 240f)) * (self.Facing == Facings.Right ? 1 : -1);
                        self.Speed.Y -= 15f;
                    }
                }
            }
        }

        private static Platform modSurfaceIndexGetPlatformByPriority(On.Celeste.SurfaceIndex.orig_GetPlatformByPriority orig, List<Entity> platforms)
        {
            // if vanilla already has platforms to get the sound index from, use those.
            if (platforms.Count != 0)
            {
                return orig(platforms);
            }

            // check if we are climbing a climbable one-way platform.
            Player player = Engine.Scene.Tracker.GetEntity<Player>();
            if (player != null)
            {
                ClimbableOneWayPlatform platform = player.CollideFirst<ClimbableOneWayPlatform>(player.Center + Vector2.UnitX * (float)player.Facing);
                if (platform != null && platform.surfaceIndex != -1)
                {
                    // yes we are! pass it off as a Platform so that the game can get its surface index later.
                    return new WallSoundIndexHolder(platform.surfaceIndex);
                }
            }

            return orig(platforms);
        }

        // this is a dummy Platform that is just here to hold a wall surface sound index, that the game will read.
        // it isn't actually used as a platform!
        private class WallSoundIndexHolder : Platform
        {
            private int wallSoundIndex;

            public WallSoundIndexHolder(int wallSoundIndex) : base(Vector2.Zero, false)
            {
                this.wallSoundIndex = wallSoundIndex;
            }

            public override void MoveHExact(int move)
            {
                throw new NotImplementedException();
            }

            public override void MoveVExact(int move)
            {
                throw new NotImplementedException();
            }

            public override int GetWallSoundIndex(Player player, int side)
            {
                return wallSoundIndex;
            }
        }

        private static ClimbableOneWayPlatform collideFirstOutside(Entity e, Vector2 at, bool leftToRight)
        {
            foreach (ClimbableOneWayPlatform item in e.Scene.Tracker.GetEntities<ClimbableOneWayPlatform>())
            {
                if (item.AllowLeftToRight == leftToRight && !Collide.Check(e, item) && Collide.Check(e, item, at))
                {
                    return item;
                }
            }
            return null;
        }

        // ======== Begin of entity code ========

        private int lines;
        private string overrideTexture;
        private float animationDelay;
        private int surfaceIndex;

        public bool AllowLeftToRight;

        private float climbJumpGrabCooldown = -1f;

        #region CustomizationOptions_Jackal

        private bool sameDirBoost;
        public string stamBehavior;
        private float initialStamina = -1f;
        public bool stamStored = false;


        #region CornerBoostVars_Jackal

        private bool hasMomentumCarrying = false;
        private float initialSpeed = 0f;
        private float grabTimer = -1f;

        /** must be <=1 and > 0, rate at which stored speed decays*/
        public float curvature;

        /** time for all speed to decay */
        public float decayTime;

        /** generates stored speed following the function https://www.desmos.com/calculator/wtsume11vs 
         time parameter refers to elapsed time since initial speed was set*/
        private float bonusSpeed(float spdX, float time)
        {
            return (float)(spdX * Math.Pow(decayTime, -2.0) * (decayTime + time) * (decayTime - time) * Math.Pow(curvature, time));
        }

        #endregion CornerBoostVars_Jackal

        #endregion CustomizationOptions_Jackal

        public ClimbableOneWayPlatform(Vector2 position, int height, bool allowLeftToRight, string overrideTexture, float animationDelay, int surfaceIndex, string stamBehavior, bool sameDirBoost, float decayTime, float curvature)
            : base(position)
        {

            lines = height / 8;
            AllowLeftToRight = allowLeftToRight;
            Depth = -60;
            this.overrideTexture = overrideTexture;
            this.animationDelay = animationDelay;
            this.surfaceIndex = surfaceIndex;

            float hitboxOffset = 0f;
            if (AllowLeftToRight)
                hitboxOffset = 3f;

            Collider = new Hitbox(5f, height, hitboxOffset, 0);

            this.decayTime = Calc.Clamp(decayTime, 0f, int.MaxValue);
            curvature = Calc.Clamp(curvature, 0f, 1f);
            hasMomentumCarrying = (decayTime != 0 && curvature != 0f);
            this.stamBehavior = stamBehavior.ToLower();
            this.sameDirBoost = sameDirBoost;
            initialStamina = stamBehavior == "conserve" ? 0f : -1f;
        }

        public ClimbableOneWayPlatform(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Height, !data.Bool("left"), data.Attr("texture", "default"), data.Float("animationDelay", 0f), data.Int("surfaceIndex", -1), data.Attr("staminaBehavior", "None"), data.Bool("sameDirectionJumpBoost", false), data.Float("momentumJumpDecayTime", 0f), data.Float("momentumJumpDecayCurvature", 1f)) { }

        public override void Awake(Scene scene)
        {
            if (animationDelay > 0f)
            {
                for (int i = 0; i < lines; i++)
                {
                    Sprite jumpthruSprite = new Sprite(GFX.Game, "objects/jumpthru/" + overrideTexture);
                    jumpthruSprite.AddLoop("idle", "", animationDelay);

                    jumpthruSprite.Y = i * 8;
                    jumpthruSprite.Rotation = (float)(Math.PI / 2);
                    if (AllowLeftToRight)
                        jumpthruSprite.X = 8;
                    else
                        jumpthruSprite.Scale.Y = -1;

                    jumpthruSprite.Play("idle");
                    Add(jumpthruSprite);
                }
            }
            else
            {
                AreaData areaData = AreaData.Get(scene);
                string jumpthru = areaData.Jumpthru;
                if (!string.IsNullOrEmpty(overrideTexture) && !overrideTexture.Equals("default"))
                {
                    jumpthru = overrideTexture;
                }

                MTexture mTexture = GFX.Game["objects/jumpthru/" + jumpthru];
                int num = mTexture.Width / 8;
                for (int i = 0; i < lines; i++)
                {
                    int xTilePosition;
                    int yTilePosition;
                    if (i == 0)
                    {
                        xTilePosition = 0;
                        yTilePosition = ((!CollideCheck<Solid>(Position + new Vector2(0f, -1f))) ? 1 : 0);
                    }
                    else if (i == lines - 1)
                    {
                        xTilePosition = num - 1;
                        yTilePosition = ((!CollideCheck<Solid>(Position + new Vector2(0f, 1f))) ? 1 : 0);
                    }
                    else
                    {
                        xTilePosition = 1 + Calc.Random.Next(num - 2);
                        yTilePosition = Calc.Random.Choose(0, 1);
                    }
                    Image image = new Image(mTexture.GetSubtexture(xTilePosition * 8, yTilePosition * 8, 8, 8));
                    image.Y = i * 8;
                    image.Rotation = (float)(Math.PI / 2);

                    if (AllowLeftToRight)
                        image.X = 8;
                    else
                        image.Scale.Y = -1;

                    Add(image);
                }
            }

            ClimbableOneWayPlatform other = CollideFirst<ClimbableOneWayPlatform>();
            if (other != null)
            {
                climbJumpGrabCooldown = other.climbJumpGrabCooldown;
            }
        }

        public override void Update()
        {
            base.Update();

            // deplete the cooldown
            if (climbJumpGrabCooldown >= 0f)
                climbJumpGrabCooldown -= Engine.DeltaTime;

            // keeps the stamina unstored only if player is not pressing grab, latched to a wall and colliding with a COWP
            if (stamStored && GetPlayer() != null)
                stamStored = Input.GrabCheck && GetPlayer().StateMachine.State == 1 && GetPlayer().CollideCheck(this, GetPlayer().Center + (GetPlayer().Facing == Facings.Right ? 2 : -2) * Vector2.UnitX);

            manageTimer(out bool v);

            Console.WriteLine(Input.Jump.BufferTime);



        }

        // starts a new timer
        public void startTimer()
        {
            if (hasMomentumCarrying && grabTimer < 0f)
            {
                grabTimer = Engine.DeltaTime;
            }
        }

        public void manageTimer(out bool valid)
        {
            if (grabTimer > 0f)
            {
                grabTimer += Engine.DeltaTime;
            }
            valid = grabTimer < decayTime;
            if (!valid)
            {
                grabTimer = 0f;
            }
        }

        public static Player GetPlayer()
        {
            return (Engine.Scene as Level)?.Tracker?.GetEntity<Player>();
        }
    }
}
