using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.IntoTheJungleCodeMod {
    // ... yeah this name is kind of a meme.
    [CustomEntity("IntoTheJungleCodeMod/SetColorGradeIfFlagOnSpawnController")]
    class SetColorGradeIfFlagOnSpawnController : Entity {
        private readonly string colorGrade;
        private readonly string flag;

        public SetColorGradeIfFlagOnSpawnController(EntityData data, Vector2 offset) {
            colorGrade = data.Attr("colorGrade");
            flag = data.Attr("flag");
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            // "if flag on spawn"
            if (SceneAs<Level>().Session.GetFlag(flag)) {
                // "set color grade"
                SceneAs<Level>().SnapColorGrade(colorGrade);
            }

            // then we're done
            RemoveSelf();
        }
    }
}
