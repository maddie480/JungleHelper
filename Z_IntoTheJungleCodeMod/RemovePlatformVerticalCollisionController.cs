using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.IntoTheJungleCodeMod {
    // this cursed controller allows to boost the performance of that room with a bunch of falling blocks that won't ever hit anything anyway,
    // by just removing the collide check between a moving solid and other solids.
    [CustomEntity("IntoTheJungleCodeMod/RemovePlatformVerticalCollisionController")]
    class RemovePlatformVerticalCollisionController : Entity {
        public RemovePlatformVerticalCollisionController(EntityData data, Vector2 offset) : base(data.Position + offset) { }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            Logger.Log("IntoTheJungleCodeMod/RemovePlatformVerticalCollisionController", "Removing platform vertical collision to boost performance");
            On.Celeste.Platform.MoveVCollideSolids += moveVButDontCollideSolids;
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);

            Logger.Log("IntoTheJungleCodeMod/RemovePlatformVerticalCollisionController", "Restoring platform vertical collision");
            On.Celeste.Platform.MoveVCollideSolids -= moveVButDontCollideSolids;
        }

        public override void SceneEnd(Scene scene) {
            base.SceneEnd(scene);

            Logger.Log("IntoTheJungleCodeMod/RemovePlatformVerticalCollisionController", "Restoring platform vertical collision");
            On.Celeste.Platform.MoveVCollideSolids -= moveVButDontCollideSolids;
        }

        private bool moveVButDontCollideSolids(On.Celeste.Platform.orig_MoveVCollideSolids orig, Platform self, float moveV, bool thruDashBlocks, Action<Vector2, Vector2, Platform> onCollide) {
            self.MoveV(moveV);
            return false; // no collision
        }
    }
}
