using Celeste.Mod.Entities;
using Celeste.Mod.JungleHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JungleHelper.Triggers {
    [CustomEntity("JungleHelper/GeckoTutorial", "JungleHelper/GeckoTutorialTrigger")]
    [Tracked]
    public class GeckoTutorialTrigger : Trigger {
        public readonly string GeckoId;
        public readonly bool ShowTutorial;

        public GeckoTutorialTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            GeckoId = data.Attr("geckoId");
            ShowTutorial = data.Bool("showTutorial");
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            Gecko gecko = Gecko.FindById(Scene as Level, GeckoId);
            if (gecko != null) {
                if (ShowTutorial) {
                    gecko.TriggerShowTutorial();
                } else {
                    gecko.TriggerHideTutorial();
                }
            }
        }
    }
}
