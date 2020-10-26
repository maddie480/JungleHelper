using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Entities;
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

namespace Celeste.Mod.JungleHelper.Entities {
    [CustomEntity("JungleHelper/ClimbableOneWayPlatform")]
    [Tracked]
    class ClimbableOneWayPlatform : Entity {
        private static ILHook hookOnUpdateSprite;

        private static FieldInfo actorMovementCounter = typeof(Actor).GetField("movementCounter", BindingFlags.Instance | BindingFlags.NonPublic);

        private static bool hooksActive = false;
        private static bool maxHelpingHandIsHere = false;

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
                maxHelpingHandIsHere = Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "MaxHelpingHand", Version = new Version(1, 4, 2) });
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
            using (new DetourContext { Before = { "*" } }) { // these don't always call the orig methods, better apply them first.
                On.Celeste.Actor.MoveHExact += onActorMoveHExact;
            }

            // block "climb hopping" on top of climbable one-way platforms, because this just looks weird.
            On.Celeste.Player.ClimbHopBlockedCheck += onPlayerClimbHopBlockedCheck;

            using (new DetourContext { ID = "JungleHelper", Before = { "*" } }) { // let's take over Spring Collab 2020, we can break it, this is not a collab map!
                // mod collide checks to include climbable one-way platforms, so that the player behaves with them like with walls.
                IL.Celeste.Player.ClimbCheck += modCollideChecks; // allow player to climb on them
                IL.Celeste.Player.ClimbBegin += modCollideChecks; // if not applied, the player will clip through jumpthrus if trying to climb on them
                IL.Celeste.Player.ClimbUpdate += modCollideChecks; // when climbing, jumpthrus are handled like walls
                IL.Celeste.Player.SlipCheck += modCollideChecks; // make climbing on jumpthrus not slippery
                IL.Celeste.Player.OnCollideH += modCollideChecks; // handle dashes against jumpthrus properly, without "shifting" down

                // have the push animation when Madeline runs against a jumpthru for example
                hookOnUpdateSprite = new ILHook(typeof(Player).GetMethod("orig_UpdateSprite", BindingFlags.NonPublic | BindingFlags.Instance), modCollideChecks);
            }

            // make the player able to grab the climbable one-way even when going up.
            IL.Celeste.Player.NormalUpdate += modPlayerNormalUpdate;
            On.Celeste.Player.ClimbJump += modPlayerClimbJump;

            On.Celeste.SurfaceIndex.GetPlatformByPriority += modSurfaceIndexGetPlatformByPriority;
        }

        public static void deactivateHooks() {
            if (!hooksActive) {
                return;
            }
            hooksActive = false;

            Logger.Log(LogLevel.Info, "JungleHelper/ClimbableOneWayPlatform", "=== Deactivating one-way platform hooks");

            On.Celeste.Actor.MoveHExact -= onActorMoveHExact;

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
        }

        private static bool onActorMoveHExact(On.Celeste.Actor.orig_MoveHExact orig, Actor self, int moveH, Collision onCollide, Solid pusher) {
            // fall back to vanilla if no sideways jumpthru or climbable one-way is in the room.
            if (self.Scene == null || (self.Scene.Tracker.CountEntities<ClimbableOneWayPlatform>() == 0 && !(maxHelpingHandIsHere && RoomContainsSidewaysJumpThrus(self))))
                return orig(self, moveH, onCollide, pusher);

            Vector2 targetPosition = self.Position + Vector2.UnitX * moveH;
            int moveDirection = Math.Sign(moveH);
            int moveAmount = 0;
            bool movingLeftToRight = moveH > 0;
            while (moveH != 0) {
                bool didCollide = false;

                // check if colliding with a solid
                Solid solid = self.CollideFirst<Solid>(self.Position + Vector2.UnitX * moveDirection);
                if (solid != null) {
                    didCollide = true;
                } else {
                    // check if colliding with a climbable one-way platform while pressing Grab
                    ClimbableOneWayPlatform climbablePlatform = collideFirstOutside(self, self.Position + Vector2.UnitX * moveDirection, !movingLeftToRight);
                    if (Input.Grab.Check && self is Player player && player.Stamina >= 20f && climbablePlatform != null && climbablePlatform.climbJumpGrabCooldown <= 0f) {

                        // there is a sideways jump-thru and we are moving in the opposite direction => collision
                        didCollide = true;
                    } else if (maxHelpingHandIsHere) {
                        // check if colliding with a sideways jumpthru
                        didCollide = CheckCollisionWithSidewaysJumpthruWhileMoving(self, moveDirection, movingLeftToRight);
                    }
                }

                if (didCollide) {
                    Vector2 movementCounter = (Vector2) actorMovementCounter.GetValue(self);
                    movementCounter.X = 0f;
                    actorMovementCounter.SetValue(self, movementCounter);
                    onCollide?.Invoke(new CollisionData {
                        Direction = Vector2.UnitX * moveDirection,
                        Moved = Vector2.UnitX * moveAmount,
                        TargetPosition = targetPosition,
                        Hit = solid,
                        Pusher = pusher
                    });
                    return true;
                }

                // continue moving
                moveAmount += moveDirection;
                moveH -= moveDirection;
                self.X += moveDirection;
            }
            return false;
        }

        private static bool onPlayerClimbHopBlockedCheck(On.Celeste.Player.orig_ClimbHopBlockedCheck orig, Player self) {
            bool vanillaCheck = orig(self);
            if (vanillaCheck)
                return vanillaCheck;

            // block climb hops on jumpthrus because those look weird
            return self.CollideCheckOutside<ClimbableOneWayPlatform>(self.Position + Vector2.UnitX * (int) self.Facing);
        }

        private static void modCollideChecks(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            bool isClimb = il.Method.Name.Contains("Climb");

            while (cursor.Next != null) {
                Instruction next = cursor.Next;

                // we want to replace all CollideChecks with solids here.
                if (next.OpCode == OpCodes.Call && (next.Operand as MethodReference)?.FullName == "System.Boolean Monocle.Entity::CollideCheck<Celeste.Solid>(Microsoft.Xna.Framework.Vector2)") {
                    Logger.Log("JungleHelper/ClimbableOneWayPlatform", $"Patching Entity.CollideCheck to include climbable one-ways at {cursor.Index} in IL for {il.Method.Name}");

                    cursor.Remove();
                    cursor.EmitDelegate<Func<Entity, Vector2, bool>>((self, checkAtPosition) => {
                        // we still want to check for solids...
                        if (self.CollideCheck<Solid>(checkAtPosition))
                            return true;

                        // if we are not checking a side, this certainly has nothing to do with jumpthrus.
                        if (self.Position.X == checkAtPosition.X)
                            return false;

                        // our entity also collides if this is with a jumpthru and we are colliding with the solid side of it.
                        // we are in this case if the jumpthru is left to right (the "solid" side of it is the right one) 
                        // and we are checking the collision on the left side of the player for example.
                        bool collideOnLeftSideOfPlayer = (self.Position.X > checkAtPosition.X);
                        ClimbableOneWayPlatform oneway = collideFirstOutside(self, checkAtPosition, collideOnLeftSideOfPlayer);
                        return (oneway != null && self is Player player
                            && oneway.Bottom >= self.Top + checkAtPosition.Y - self.Position.Y + 3)
                            || (maxHelpingHandIsHere && EntityIsCollidingWithSidewaysJumpthrus(self, checkAtPosition, isClimb));
                    });
                }

                if (next.OpCode == OpCodes.Callvirt && (next.Operand as MethodReference)?.FullName == "System.Boolean Monocle.Scene::CollideCheck<Celeste.Solid>(Microsoft.Xna.Framework.Vector2)") {
                    Logger.Log("JungleHelper/ClimbableOneWayPlatform", $"Patching Scene.CollideCheck to include climbable one-ways at {cursor.Index} in IL for {il.Method.Name}");

                    cursor.Remove();
                    cursor.EmitDelegate<Func<Scene, Vector2, bool>>((self, vector) => self.CollideCheck<Solid>(vector) || self.CollideCheck<ClimbableOneWayPlatform>(vector)
                        || (maxHelpingHandIsHere && SceneIsCollidingWithSidewaysJumpthrus(self, vector, isClimb)));
                }

                cursor.Index++;
            }
        }

        private static bool CheckCollisionWithSidewaysJumpthruWhileMoving(Actor self, int moveDirection, bool movingLeftToRight) {
            return SidewaysJumpThru.CheckCollisionWithSidewaysJumpthruWhileMoving(self, moveDirection, movingLeftToRight);
        }

        private static bool RoomContainsSidewaysJumpThrus(Actor self) {
            return SidewaysJumpThru.RoomContainsSidewaysJumpThrus(self);
        }

        private static bool EntityIsCollidingWithSidewaysJumpthrus(Entity self, Vector2 checkAtPosition, bool isClimb) {
            return SidewaysJumpThru.EntityCollideCheckWithSidewaysJumpthrus(self, checkAtPosition, isClimb, isWallJump: false);
        }

        private static bool SceneIsCollidingWithSidewaysJumpthrus(Scene self, Vector2 vector, bool isClimb) {
            return SidewaysJumpThru.SceneCollideCheckWithSidewaysJumpthrus(self, vector, isClimb, isWallJump: false);
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

        private static void modPlayerClimbJump(On.Celeste.Player.orig_ClimbJump orig, Player self) {
            orig(self);

            ClimbableOneWayPlatform platform = collideFirstOutside(self, self.Position + new Vector2((int) self.Facing * 2, 0), self.Facing == Facings.Left);
            if (platform != null) {
                // trigger the cooldown
                platform.climbJumpGrabCooldown = 0.35f;
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

        private int lines;
        private string overrideTexture;
        private float animationDelay;
        private int surfaceIndex;

        public bool AllowLeftToRight;

        private float climbJumpGrabCooldown = -1f;

        public ClimbableOneWayPlatform(Vector2 position, int height, bool allowLeftToRight, string overrideTexture, float animationDelay, int surfaceIndex)
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
        }

        public ClimbableOneWayPlatform(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Height, !data.Bool("left"), data.Attr("texture", "default"), data.Float("animationDelay", 0f), data.Int("surfaceIndex", -1)) { }

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
            if (climbJumpGrabCooldown >= 0f)
                climbJumpGrabCooldown -= Engine.DeltaTime;
        }
    }
}
