using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.JungleHelper.Entities {
    [CustomEntity("JungleHelper/TheoStatue")]
    [Tracked]
    public class TheoStatue : TheoCrystal {
        public TheoStatue(EntityData data, Vector2 offset) : base(data, offset) {
            // a Theo statue is a Theo crystal with a different sprite...
            Remove(sprite);
            sprite = JungleHelperModule.CreateReskinnableSprite(data, "theo_statue");
            Add(sprite);

            // with a different hitbox...
            Collider = new Hitbox(9f, 43f, -5f, -35f);

            // also fix issues related to transitioning.
            Tag = 0;
        }

        public override void Update() {
            base.Update();

            // this is not grabbable.
            Hold.cannotHoldTimer = 1f;
        }
    }
}
