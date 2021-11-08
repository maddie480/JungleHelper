using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.JungleHelper.Entities {
    [CustomEntity("JungleHelper/ClimbableOneWayPlatform")]
    [Tracked]
    public class ClimbableOneWayPlatform : Entity {
        private static ILHook hookOnUpdateSprite;

        private static FieldInfo actorMovementCounter = typeof(Actor).GetField("movementCounter", BindingFlags.Instance | BindingFlags.NonPublic);

        private static bool hooksActive = false;

        public static void Load() {
            On.Celeste.LevelLoader.ctor += onLevelLoad;
            On.Celeste.OverworldLoader.ctor += onOverworldLoad;
        }

        public static void Unload() {
            On.Celeste.LevelLoader.ctor -= onLevelLoad;
            On.Celeste.OverworldLoader.ctor -= onOverworldLoad;
            deactivateHooks();
        }

        private static void onLevelLoad(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startPosition) {
            orig(self, session, startPosition);

            if (session.MapData?.Levels?.Any(level => level.Entities?.Any(entity => entity.Name == "JungleHelper/ClimbableOneWayPlatform") ?? false) ?? false) {
                activateHooks();
            } else {
                deactivateHooks();
            }
        }

        private static void onOverworldLoad(On.Celeste.OverworldLoader.orig_ctor orig, OverworldLoader self, Overworld.StartMode startMode, HiresSnow snow) {
            orig(self, startMode, snow);

            if (startMode != (Overworld.StartMode) (-1)) { // -1 = in-game overworld from the collab utils
                deactivateHooks();
            }
        }

        public static void activateHooks() {
            if (hooksActive) {
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

            // customization hooks (stamina conservation and speed boosts)
            On.Celeste.Player.ClimbBegin += modPlayerClimbBegin;
            On.Celeste.Player.ClimbUpdate += modPlayerClimbUpdate;
        }

        public static void deactivateHooks() {
            if (!hooksActive) {
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

            On.Celeste.Player.ClimbBegin -= modPlayerClimbBegin;
            On.Celeste.Player.ClimbUpdate -= modPlayerClimbUpdate;
        }

        private static void addSidewaysJumpthrusInHorizontalMoveMethods(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall<Entity>("CollideFirst"))
                 && cursor.TryGotoNext(instr => instr.OpCode == OpCodes.Brfalse_S || instr.OpCode == OpCodes.Brtrue_S)) {

                Logger.Log("JungleHelper/ClimbableOneWayPlatform", $"Injecting sideways jumpthru check at {cursor.Index} in IL for {il.Method.Name}");
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.EmitDelegate<Func<Solid, Actor, int, Solid>>((orig, self, moveH) => {
                    if (orig != null)
                        return orig;

                    int moveDirection = Math.Sign(moveH);
                    bool movingLeftToRight = moveH > 0;
                    if (checkCollisionWithSidewaysMovingPlatformsWhileMoving(self, moveDirection, movingLeftToRight)) {
                        return new Solid(Vector2.Zero, 0, 0, false); // what matters is that it is non null.
                    }

                    return null;
                });
            }
        }

        private static bool checkCollisionWithSidewaysMovingPlatformsWhileMoving(Actor self, int moveDirection, bool movingLeftToRight) {
            ClimbableOneWayPlatform climbablePlatform = collideFirstOutside(self, self.Position + Vector2.UnitX * moveDirection, !movingLeftToRight);
            bool canGrabOnClimbableOneWayPlatform = Input.GrabCheck && self is Player && climbablePlatform != null && climbablePlatform.climbJumpGrabCooldown <= 0f;
            if (climbablePlatform != null && climbablePlatform.staminaBehavior == StaminaBehavior.None) {
                return canGrabOnClimbableOneWayPlatform && (self as Player).Stamina >= 20f;
            }

            // if a custom stamina behavior is present, set the stamina to slightly above red point
            if (canGrabOnClimbableOneWayPlatform) {
                (self as Player).Stamina = Math.Max((self as Player).Stamina, 21f);
            }
            return canGrabOnClimbableOneWayPlatform;
        }

        private static bool onPlayerClimbHopBlockedCheck(On.Celeste.Player.orig_ClimbHopBlockedCheck orig, Player self) {
            bool vanillaCheck = orig(self);
            if (vanillaCheck) {
                return vanillaCheck;
            }

            // block climb hops on jumpthrus because those look weird
            return self.CollideCheckOutside<ClimbableOneWayPlatform>(self.Position + Vector2.UnitX * (int) self.Facing);
        }

        private static void modCollideChecks(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // create a Vector2 temporary variable
            VariableDefinition checkAtPositionStore = new VariableDefinition(il.Import(typeof(Vector2)));
            il.Body.Variables.Add(checkAtPositionStore);

            bool isClimb = il.Method.Name.Contains("Climb");

            while (cursor.Next != null) {
                Instruction next = cursor.Next;

                // we want to replace all CollideChecks with solids here.
                if (next.OpCode == OpCodes.Call && (next.Operand as MethodReference)?.FullName == "System.Boolean Monocle.Entity::CollideCheck<Celeste.Solid>(Microsoft.Xna.Framework.Vector2)") {
                    Logger.Log("JungleHelper/ClimbableOneWayPlatform", $"Patching Entity.CollideCheck to include climbable one-ways at {cursor.Index} in IL for {il.Method.Name}");

                    callOrigMethodKeepingEverythingOnStack(cursor, checkAtPositionStore, isSceneCollideCheck: false);

                    cursor.EmitDelegate<Func<bool, Entity, Vector2, bool>>((orig, self, checkAtPosition) => {
                        // we still want to check for solids...
                        if (orig)
                            return true;

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

                if (next.OpCode == OpCodes.Callvirt && (next.Operand as MethodReference)?.FullName == "System.Boolean Monocle.Scene::CollideCheck<Celeste.Solid>(Microsoft.Xna.Framework.Vector2)") {
                    Logger.Log("JungleHelper/ClimbableOneWayPlatform", $"Patching Scene.CollideCheck to include climbable one-ways at {cursor.Index} in IL for {il.Method.Name}");

                    callOrigMethodKeepingEverythingOnStack(cursor, checkAtPositionStore, isSceneCollideCheck: true);

                    cursor.EmitDelegate<Func<bool, Scene, Vector2, bool>>((orig, self, vector) => orig || self.CollideCheck<ClimbableOneWayPlatform>(vector));
                }

                cursor.Index++;
            }
        }

        private static void callOrigMethodKeepingEverythingOnStack(ILCursor cursor, VariableDefinition checkAtPositionStore, bool isSceneCollideCheck) {
            // store the position in the local variable
            cursor.Emit(OpCodes.Stloc, checkAtPositionStore);
            cursor.Emit(OpCodes.Ldloc, checkAtPositionStore);

            // let vanilla call CollideCheck
            cursor.Index++;

            // reload the parameters
            cursor.Emit(OpCodes.Ldarg_0);
            if (isSceneCollideCheck) {
                cursor.Emit(OpCodes.Call, typeof(Entity).GetProperty("Scene").GetGetMethod());
            }

            cursor.Emit(OpCodes.Ldloc, checkAtPositionStore);
        }

        private static void modPlayerNormalUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // let's jump to if (this.Speed.Y < 0f) => "is the player going up? if so, they can't grab!"
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdsfld(typeof(Input), "Grab")) &&
                cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchLdarg(0), // this
                instr => instr.MatchLdflda<Player>("Speed"),
                instr => instr.MatchLdfld<Vector2>("Y"),
                instr => instr.MatchLdcR4(0f),
                instr => instr.OpCode == OpCodes.Blt_Un || instr.OpCode == OpCodes.Blt_Un_S)) {

                Instruction afterCheck = cursor.Next;

                // step back before the "Speed.Y < 0f" check (more specifically, inside it. it would be skipped otherwise)
                cursor.Index -= 4;

                Logger.Log("JungleHelper/ClimbableOneWayPlatform", $"Injecting code to be able to grab climbable one-ways when going up at {cursor.Index} in IL code for Player.NormalUpdate");

                // inject ourselves to jump over the "Speed.Y < 0f" check, and put this back
                cursor.EmitDelegate<Func<Player, bool>>(self => {
                    ClimbableOneWayPlatform platform = collideFirstOutside(self, self.Position + new Vector2((int) self.Facing * 2, 0), self.Facing == Facings.Left);
                    return platform != null && platform.climbJumpGrabCooldown <= 0f;
                });
                cursor.Emit(OpCodes.Brtrue, afterCheck);
                cursor.Emit(OpCodes.Ldarg_0);
            }
        }

        // grabs the speed and stamina if applicable at the earliest possible time
        private static void modPlayerClimbBegin(On.Celeste.Player.orig_ClimbBegin orig, Player self) {
            ClimbableOneWayPlatform platform = collideFirstOutside(self, self.Position + new Vector2((int) self.Facing * 2, 0), self.Facing == Facings.Left);
            if (platform != null) {
                // store initial speed and stamina if needed   
                if (platform.hasMomentumCarrying) {
                    platform.initialSpeed = self.Speed.X;
                    platform.startTimer();
                }
                if (platform.staminaBehavior == StaminaBehavior.Conserve) {
                    platform.stamStored = true;
                    platform.initialStamina = self.Stamina;
                }
            }
            orig.Invoke(self);
        }

        private static int modPlayerClimbUpdate(On.Celeste.Player.orig_ClimbUpdate orig, Player self) {
            ClimbableOneWayPlatform platform = collideFirstOutside(self, self.Position + new Vector2((int) self.Facing * 2, 0), self.Facing == Facings.Left);
            if (platform != null) {
                // checks for momentum jumps and same direction jumps. the latter cannot be activated if the former is active, as the purpose is nullified anyways
                if ((Input.Jump.Pressed || Input.Jump.BufferTime < 0.08f) && (int) Input.Aim.Value.X != 0) {
                    if (platform.hasMomentumCarrying && platform.grabTimer < platform.momentumJumpDecayTime) {
                        self.Speed.X += platform.bonusSpeed(platform.initialSpeed, platform.grabTimer);
                        platform.grabTimer = 0f;
                    } else if (platform.sameDirBoost) {
                        if (Math.Abs(self.Speed.X) < 240f && Input.Aim.Value.X != 0 && Input.Aim.Value.X / Math.Abs(Input.Aim.Value.X) == (int) self.Facing) {
                            self.Speed.X += (140f * (1 - Math.Abs(self.Speed.X) / 240f)) * (int) self.Facing;
                        }
                    }
                    platform.stamStored = false;
                }

                // stamina management
                if (platform.staminaBehavior == StaminaBehavior.Regain) {
                    self.RefillStamina();
                } else if (platform.stamStored) {
                    self.Stamina = Math.Max(platform.initialStamina, 21);
                }
            }
            return orig.Invoke(self);
        }

        private static void modPlayerClimbJump(On.Celeste.Player.orig_ClimbJump orig, Player self) {
            orig(self);

            ClimbableOneWayPlatform platform = collideFirstOutside(self, self.Position + new Vector2((int) self.Facing * 2, 0), self.Facing == Facings.Left);
            if (platform != null) {
                // trigger the cooldown
                platform.climbJumpGrabCooldown = 0.35f;

                // stamina management
                if (platform.staminaBehavior == StaminaBehavior.Regain) {
                    self.RefillStamina();
                } else if (platform.staminaBehavior == StaminaBehavior.Conserve) {
                    self.Stamina = Math.Max(platform.initialStamina, 21);
                }
            }
        }

        private static Platform modSurfaceIndexGetPlatformByPriority(On.Celeste.SurfaceIndex.orig_GetPlatformByPriority orig, List<Entity> platforms) {
            // if vanilla already has platforms to get the sound index from, use those.
            if (platforms.Count != 0) {
                return orig(platforms);
            }

            // check if we are climbing a climbable one-way platform.
            Player player = Engine.Scene.Tracker.GetEntity<Player>();
            if (player != null) {
                ClimbableOneWayPlatform platform = player.CollideFirst<ClimbableOneWayPlatform>(player.Center + Vector2.UnitX * (float) player.Facing);
                if (platform != null && platform.surfaceIndex != -1) {
                    // yes we are! pass it off as a Platform so that the game can get its surface index later.
                    return new WallSoundIndexHolder(platform.surfaceIndex);
                }
            }

            return orig(platforms);
        }

        // this is a dummy Platform that is just here to hold a wall surface sound index, that the game will read.
        // it isn't actually used as a platform!
        private class WallSoundIndexHolder : Platform {
            private int wallSoundIndex;

            public WallSoundIndexHolder(int wallSoundIndex) : base(Vector2.Zero, false) {
                this.wallSoundIndex = wallSoundIndex;
            }

            public override void MoveHExact(int move) {
                throw new NotImplementedException();
            }

            public override void MoveVExact(int move) {
                throw new NotImplementedException();
            }

            public override int GetWallSoundIndex(Player player, int side) {
                return wallSoundIndex;
            }
        }

        private static ClimbableOneWayPlatform collideFirstOutside(Entity e, Vector2 at, bool leftToRight) {
            foreach (ClimbableOneWayPlatform item in e.Scene.Tracker.GetEntities<ClimbableOneWayPlatform>()) {
                if (item.AllowLeftToRight == leftToRight && !Collide.Check(e, item) && Collide.Check(e, item, at)) {
                    return item;
                }
            }
            return null;
        }

        // ======== Begin of entity code ========
        public enum StaminaBehavior {
            None,
            Conserve,
            Regain
        };

        // options
        private int lines;
        private string overrideTexture;
        private float animationDelay;
        private int surfaceIndex;
        public bool AllowLeftToRight;

        private bool sameDirBoost;
        public StaminaBehavior staminaBehavior = StaminaBehavior.None;
        private bool hasMomentumCarrying = false;
        public float momentumJumpDecayTime;
        public float momentumJumpDecayCurvature;

        // state keeping
        private float climbJumpGrabCooldown = -1f;

        private float initialStamina = -1f;
        public bool stamStored = false;

        private float initialSpeed = 0f;
        private float grabTimer = 0f;

        // generates stored speed following the function https://www.desmos.com/calculator/wtsume11vs 
        // time parameter refers to elapsed time since initial speed was set
        private float bonusSpeed(float spdX, float time) {
            if (time > momentumJumpDecayTime) {
                return 0f;
            }
            if (time <= 4 * Engine.DeltaTime) {
                return (spdX * 1.2f);
            }
            return (float) ((spdX * 1.2) * Math.Pow(momentumJumpDecayTime, -2.0) * (momentumJumpDecayTime + time) * (momentumJumpDecayTime - time) * Math.Pow(momentumJumpDecayCurvature, time));
        }

        public ClimbableOneWayPlatform(Vector2 position, int height, bool allowLeftToRight, string overrideTexture, float animationDelay, int surfaceIndex, StaminaBehavior StaminaBehavior, bool sameDirBoost, float momentumJumpDecayTime, float momentumJumpDecayCurvature)
            : base(position) {

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

            // assigns and organizes customization variables
            this.momentumJumpDecayTime = Calc.Clamp(momentumJumpDecayTime, 0f, int.MaxValue);
            this.momentumJumpDecayCurvature = Calc.Clamp(momentumJumpDecayCurvature, 0f, 1f);
            hasMomentumCarrying = (this.momentumJumpDecayTime != 0 && this.momentumJumpDecayCurvature != 0f);
            staminaBehavior = StaminaBehavior;
            this.sameDirBoost = sameDirBoost;
            initialStamina = staminaBehavior == StaminaBehavior.Conserve ? 0f : -1f;
        }

        public ClimbableOneWayPlatform(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Height, !data.Bool("left"), data.Attr("texture", "default"), data.Float("animationDelay", 0f), data.Int("surfaceIndex", -1),
                  data.Enum("staminaBehavior", StaminaBehavior.None), data.Bool("sameDirectionJumpBoost", false), data.Float("momentumJumpDecayTime", 0f), data.Float("momentumJumpDecayCurvature", 1f)) { }

        public override void Awake(Scene scene) {
            if (animationDelay > 0f) {
                for (int i = 0; i < lines; i++) {
                    Sprite jumpthruSprite = new Sprite(GFX.Game, "objects/jumpthru/" + overrideTexture);
                    jumpthruSprite.AddLoop("idle", "", animationDelay);

                    jumpthruSprite.Y = i * 8;
                    jumpthruSprite.Rotation = (float) (Math.PI / 2);
                    if (AllowLeftToRight)
                        jumpthruSprite.X = 8;
                    else
                        jumpthruSprite.Scale.Y = -1;

                    jumpthruSprite.Play("idle");
                    Add(jumpthruSprite);
                }
            } else {
                AreaData areaData = AreaData.Get(scene);
                string jumpthru = areaData.Jumpthru;
                if (!string.IsNullOrEmpty(overrideTexture) && !overrideTexture.Equals("default")) {
                    jumpthru = overrideTexture;
                }

                MTexture mTexture = GFX.Game["objects/jumpthru/" + jumpthru];
                int num = mTexture.Width / 8;
                for (int i = 0; i < lines; i++) {
                    int xTilePosition;
                    int yTilePosition;
                    if (i == 0) {
                        xTilePosition = 0;
                        yTilePosition = ((!CollideCheck<Solid>(Position + new Vector2(0f, -1f))) ? 1 : 0);
                    } else if (i == lines - 1) {
                        xTilePosition = num - 1;
                        yTilePosition = ((!CollideCheck<Solid>(Position + new Vector2(0f, 1f))) ? 1 : 0);
                    } else {
                        xTilePosition = 1 + Calc.Random.Next(num - 2);
                        yTilePosition = Calc.Random.Choose(0, 1);
                    }
                    Image image = new Image(mTexture.GetSubtexture(xTilePosition * 8, yTilePosition * 8, 8, 8));
                    image.Y = i * 8;
                    image.Rotation = (float) (Math.PI / 2);

                    if (AllowLeftToRight)
                        image.X = 8;
                    else
                        image.Scale.Y = -1;

                    Add(image);
                }
            }

            ClimbableOneWayPlatform other = CollideFirst<ClimbableOneWayPlatform>();
            if (other != null) {
                climbJumpGrabCooldown = other.climbJumpGrabCooldown;
            }
        }

        public override void Update() {
            base.Update();

            // deplete the cooldown
            if (climbJumpGrabCooldown >= 0f) {
                climbJumpGrabCooldown -= Engine.DeltaTime;
            }

            // increase the grab timer
            if (grabTimer > 0f) {
                grabTimer += Engine.DeltaTime;
            }
        }

        // starts a new timer
        private void startTimer() {
            if (hasMomentumCarrying) {
                grabTimer = Engine.DeltaTime;
            }
        }
    }
}
