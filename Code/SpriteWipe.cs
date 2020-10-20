using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.JungleHelper {
    // Useful when you want to do a wipe, but you know how to sprite better than you know how to code.
    // To make the wipe obey colouring, make your sprites white.
    class SpriteWipe : ScreenWipe {
        public static void Load() {
            On.Celeste.Mod.Meta.MapMeta.ApplyTo += onParseScreenWipe;
        }

        public static void Unload() {
            On.Celeste.Mod.Meta.MapMeta.ApplyTo -= onParseScreenWipe;
        }

        private static void onParseScreenWipe(On.Celeste.Mod.Meta.MapMeta.orig_ApplyTo orig, Meta.MapMeta self, AreaData area) {
            orig(self, area);

            if (!string.IsNullOrEmpty(self.Wipe) && self.Wipe.StartsWith("JungleHelper/SpriteWipe:")) {
                string spritePath = self.Wipe.Substring("JungleHelper/SpriteWipe:".Length);
                area.Wipe = (scene, wipeIn, onComplete) => new SpriteWipe(scene, wipeIn, spritePath, onComplete);
            }
        }

        private readonly List<MTexture> spritesIn;
        private readonly List<MTexture> spritesOut;

        public SpriteWipe(Scene scene, bool wipeIn, string spritePath, Action onComplete = null) : base(scene, wipeIn, onComplete) {
            spritesIn = GFX.Gui.GetAtlasSubtextures(spritePath + "/wipein");
            spritesOut = GFX.Gui.GetAtlasSubtextures(spritePath + "/wipeout");
        }

        public override void Render(Scene scene) {
            MTexture frame;
            if (WipeIn) {
                frame = spritesIn[Percent >= 1 ? spritesIn.Count - 1 : (int) (spritesIn.Count * Percent)];
            } else {
                frame = spritesOut[Percent >= 1 ? spritesOut.Count - 1 : (int) (spritesOut.Count * Percent)];
            }

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, Engine.ScreenMatrix);
            frame.Draw(Vector2.Zero, Vector2.Zero, WipeColor);
            Draw.SpriteBatch.End();
        }
    }
}
