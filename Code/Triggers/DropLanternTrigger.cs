using Celeste.Mod.Entities;
using Celeste.Mod.JungleHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JungleHelper.Triggers {
    [CustomEntity("JungleHelper/DropLanternTrigger")]
    [Tracked]
    class DropLanternTrigger : Trigger {
        private readonly bool oneUse;
        private readonly bool destroyLantern;

        public DropLanternTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            oneUse = data.Bool("oneUse");
            destroyLantern = data.Bool("destroyLantern");
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            if (player.Sprite.Mode == Lantern.SpriteModeMadelineLantern) {
                Lantern.DropLantern(player, destroyLantern);

                if (oneUse) {
                    RemoveSelf();
                }
            }
        }
    }
}
