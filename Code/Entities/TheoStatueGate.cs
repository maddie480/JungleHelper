using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using System;

namespace Celeste.Mod.JungleHelper.Entities {
    [CustomEntity("JungleHelper/TheoStatueGate")]
    public class TheoStatueGate : TempleGate {
        public static void Load() {
            On.Celeste.TempleGate.TheoIsNearby += modTheoIsNearby;
        }

        public static void Unload() {
            On.Celeste.TempleGate.TheoIsNearby -= modTheoIsNearby;
        }

        private static bool modTheoIsNearby(On.Celeste.TempleGate.orig_TheoIsNearby orig, TempleGate self) {
            if (self is TheoStatueGate) {
                DynData<TempleGate> selfData = new DynData<TempleGate>(self);
                TheoCrystal entity = self.Scene.Tracker.GetEntity<TheoStatue>();
                return entity == null || entity.X > self.X + 10f ||
                    Vector2.DistanceSquared(selfData.Get<Vector2>("holdingCheckFrom"), entity.Center) < (selfData.Get<bool>("open") ? 6400f : 4096f);
            }

            return orig(self);
        }

        public static TheoStatueGate Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            entityData.Values["type"] = "HoldingTheo";
            return new TheoStatueGate(entityData, offset, levelData.Name);
        }

        public TheoStatueGate(EntityData data, Vector2 offset, string levelID) : base(data, offset, levelID) { }
    }
}
