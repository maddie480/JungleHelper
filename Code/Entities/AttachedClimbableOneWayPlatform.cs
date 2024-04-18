using Celeste.Mod.Entities;
using Celeste.Mod.JungleHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.JungleHelper.Entities {
    /// <summary>
    /// This is Climbable One-Way Platform, except attached to a solid, using similar magic to Sideways Moving Platforms to handle player collision.
    /// Self-stolen from Maddie's Helping Hand: https://github.com/maddie480/MaddieHelpingHand/blob/master/Entities/AttachedSidewaysJumpThru.cs
    /// </summary>
    [CustomEntity("JungleHelper/AttachedClimbableOneWayPlatform")]
    [Tracked]
    [TrackedAs(typeof(ClimbableOneWayPlatform))]
    public class AttachedClimbableOneWayPlatform : ClimbableOneWayPlatform {
        // this variable is private, static, and never modified: so we only need reflection once to get it!
        private static readonly HashSet<Actor> solidRiders = (HashSet<Actor>) typeof(Solid).GetField("riders", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);

        public static new void Load() {
            On.Celeste.Player.IsRiding_Solid += modIsRidingSolid;
        }

        public static new void Unload() {
            On.Celeste.Player.IsRiding_Solid -= modIsRidingSolid;
        }

        // solid used internally to push/squash/carry the player around
        private Solid playerInteractingSolid;

        public readonly new bool Left;
        private readonly bool triggerBlocks;

        public DashCollision OnDashCollide;
        private Vector2 shakeOffset = Vector2.Zero;
        private StaticMover staticMover;

        public AttachedClimbableOneWayPlatform(EntityData data, Vector2 offset) : base(data, offset) {
            Left = data.Bool("left");
            triggerBlocks = data.Bool("triggerBlocks");

            // this solid will be made solid only when moving the player with the platform, so that the player gets squished and can climb the platform properly.
            playerInteractingSolid = new Solid(Position, Width, Height, safe: false);
            playerInteractingSolid.Collidable = false;
            playerInteractingSolid.Visible = false;
            if (!Left) {
                playerInteractingSolid.Position.X += 3f;
            }

            // create the StaticMover that will make this jumpthru attached.
            staticMover = new StaticMoverWithLiftSpeed() {
                SolidChecker = solid => solid.CollideRect(new Rectangle((int) X, (int) Y - 1, (int) Width, (int) Height + 2)),
                OnMove = move => SidewaysJumpthruOnMove(this, playerInteractingSolid, Left, move),
                OnShake = onShake,
                OnSetLiftSpeed = liftSpeed => playerInteractingSolid.LiftSpeed = liftSpeed
            };
            Add(staticMover);
        }

        private void onShake(Vector2 move) {
            shakeOffset += move;
            playerInteractingSolid.ShakeStaticMovers(move);
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            // add the hidden solid to the scene as well.
            scene.Add(playerInteractingSolid);
        }

        private static bool modIsRidingSolid(On.Celeste.Player.orig_IsRiding_Solid orig, Player self, Solid solid) {
            // if vanilla says we're riding, then... we're riding.
            if (orig(self, solid)) return true;

            // player isn't riding solid, but check whether the player is climbing on any of the jumpthrus attached to this solid.
            // we need to check that they're actually climbing first!
            if (self.StateMachine.State != Player.StClimb) return false;

            foreach (AttachedClimbableOneWayPlatform platform in solid.Scene.Tracker.GetEntities<AttachedClimbableOneWayPlatform>()) {
                // check whether that platform should trigger the solid we're interested in
                if (!platform.triggerBlocks || platform.staticMover.Platform != solid) continue;

                platform.playerInteractingSolid.Collidable = true;
                bool result = self.CollideCheck(platform.playerInteractingSolid, self.Position + Vector2.UnitX * (float) self.Facing);
                platform.playerInteractingSolid.Collidable = false;

                if (result) return true;
            }

            // the player isn't riding any platform
            return false;
        }

        public override void Render() {
            Position += shakeOffset;
            base.Render();
            Position -= shakeOffset;
        }

        // called when the platform moves, with the move amount
        // Self-stolen from Maddie's Helping Hand: https://github.com/maddie480/MaddieHelpingHand/blob/master/Entities/SidewaysMovingPlatform.cs
        private static void SidewaysJumpthruOnMove(Entity platform, Solid playerInteractingSolid, bool left, Vector2 move) {
            if (platform.Scene == null) {
                // the platform isn't in the scene yet (initial offset is applied by the moving platform), so don't do collide checks and just move.
                platform.Position += move;
                playerInteractingSolid.MoveHNaive(move.X);
                playerInteractingSolid.MoveVNaive(move.Y);
                return;
            }

            bool playerHasToMove = false;

            if (platform.CollideCheckOutside<Player>(platform.Position + move) && (Math.Sign(move.X) == (left ? -1 : 1))) {
                // the platform is pushing the player horizontally, so we should have the solid push the player.
                playerHasToMove = true;
            }
            if (GetPlayerClimbing(platform, left) != null) {
                // player is climbing the platform, so the solid should carry the player with the platform
                playerHasToMove = true;
            }

            // move the platform..
            platform.Position += move;

            // back up the riders, because we don't want to mess up the static variable by moving a solid while moving another solid.
            HashSet<Actor> ridersBackup = new HashSet<Actor>(solidRiders);
            solidRiders.Clear();

            // make the hidden solid collidable if it needs to push the player.
            playerInteractingSolid.Collidable = playerHasToMove;

            // determine who is riding the platform, we will need that later.
            List<Actor> platformRiders = new List<Actor>();
            if (playerInteractingSolid.Collidable) {
                foreach (Actor entity in platform.Scene.Tracker.GetEntities<Actor>()) {
                    if (entity.IsRiding(playerInteractingSolid)) {
                        platformRiders.Add(entity);
                    }
                }
            }

            // move the hidden solid, keeping its lift speed. If it is solid, it will push the player and carry them if they climb the platform.
            Vector2 liftSpeed = playerInteractingSolid.LiftSpeed;
            playerInteractingSolid.MoveH(move.X, liftSpeed.X);
            playerInteractingSolid.MoveV(move.Y, liftSpeed.Y);
            playerInteractingSolid.Collidable = false;

            // restore the riders; skip those that were also riding the platform, to avoid a double move.
            solidRiders.Clear();
            foreach (Actor rider in ridersBackup) {
                if (!platformRiders.Contains(rider)) {
                    solidRiders.Add(rider);
                }
            }
        }

        // variant on Solid.GetPlayerClimbing() that also checks for the jumpthru orientation.
        // Self-stolen from Maddie's Helping Hand: https://github.com/maddie480/MaddieHelpingHand/blob/master/Entities/SidewaysMovingPlatform.cs
        private static Player GetPlayerClimbing(Entity platform, bool left) {
            foreach (Player player in platform.Scene.Tracker.GetEntities<Player>()) {
                if (player.StateMachine.State == 1) {
                    if (!left && player.Facing == Facings.Left && platform.CollideCheckOutside(player, platform.Position + Vector2.UnitX)) {
                        return player;
                    }
                    if (left && player.Facing == Facings.Right && platform.CollideCheckOutside(player, platform.Position - Vector2.UnitX)) {
                        return player;
                    }
                }
            }
            return null;
        }
    }
}
