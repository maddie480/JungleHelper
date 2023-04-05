using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JungleHelper.Entities {
    [CustomEntity("JungleHelper/CheatCodeController")]
    public static class CheatCodeController {
        private class Nothing : Entity {
            public override void Added(Scene scene) {
                base.Added(scene);
                RemoveSelf();
            }
        }

        public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            if (!SaveData.Instance.CheatMode) {
                // yes this is definitely a very original entity I made
                return new UnlockEverythingThingy();
            }

            // we don't want the controller to spawn, but we don't want a "failed loading entity" warning either.
            return new Nothing();
        }
    }
}
