using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.JungleHelper.Entities {
    /// <summary>
    /// A Crumble Platform, except the bits aren't randomly shuffled around when it respawns.
    /// </summary>
    [CustomEntity("JungleHelper/UnrandomizedCrumblePlatform")]
    class UnrandomizedCrumblePlatform : CrumblePlatform {
        public static void Load() {
            On.Celeste.CrumblePlatform.TileIn += onCrumblePlatformTileIn;
        }

        public static void Unload() {
            On.Celeste.CrumblePlatform.TileIn -= onCrumblePlatformTileIn;
        }

        private static IEnumerator onCrumblePlatformTileIn(On.Celeste.CrumblePlatform.orig_TileIn orig, CrumblePlatform self, int index, Image img, float delay) {
            if (self is UnrandomizedCrumblePlatform) {
                // we are respawning an image at position index: instead of respawning images[fallOrder[index]], respawn images[index].
                img = new DynData<CrumblePlatform>(self).Get<List<Image>>("images")[index];
            }

            return orig(self, index, img, delay);
        }

        public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            UnrandomizedCrumblePlatform platform = new UnrandomizedCrumblePlatform(entityData, offset);
            platform.OverrideTexture = entityData.Attr("texture");
            return platform;
        }

        public UnrandomizedCrumblePlatform(EntityData data, Vector2 offset) : base(data, offset) { }
    }
}
