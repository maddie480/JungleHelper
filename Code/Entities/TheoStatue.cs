using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.JungleHelper.Entities {
    [CustomEntity("JungleHelper/TheoStatue")]
    [Tracked]
    public class TheoStatue : TheoCrystal {
        private DynData<Holdable> holdData;

        public TheoStatue(EntityData data, Vector2 offset) : base(data, offset) {
            // a Theo statue is a Theo crystal...
            DynData<TheoCrystal> self = new DynData<TheoCrystal>(this);

            // with a different sprite...
            Remove(self.Get<Sprite>("sprite"));
            Sprite sprite = JungleHelperModule.CreateReskinnableSprite(data, "theo_statue");
            self["sprite"] = sprite;
            Add(sprite);

            // a different hitbox...
            Collider = new Hitbox(9f, 43f, -5f, -35f);

            // ... that cannot be held
            holdData = new DynData<Holdable>(Hold);

            // also fix issues related to transitioning.
            Tag = 0;
        }

        public override void Update() {
            base.Update();

            // this is not grabbable.
            holdData["cannotHoldTimer"] = 1f;
        }
    }
}
