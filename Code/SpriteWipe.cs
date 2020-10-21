using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Celeste.Mod.JungleHelper {
    // Useful when you want to do a wipe, but you know how to sprite better than you know how to code.
    // To make the wipe obey colouring, make your sprites white.
    class SpriteWipe : ScreenWipe {
        public static void Load() {
            On.Celeste.Mod.Meta.MapMeta.ApplyTo += onParseScreenWipe;
            On.Celeste.LevelLoader.LoadingThread += onLevelLoad;
        }

        public static void Unload() {
            On.Celeste.Mod.Meta.MapMeta.ApplyTo -= onParseScreenWipe;
            On.Celeste.LevelLoader.LoadingThread -= onLevelLoad;
        }

        private static Atlas wipeAtlas;
        private static string atlasPath;

        private static void onParseScreenWipe(On.Celeste.Mod.Meta.MapMeta.orig_ApplyTo orig, Meta.MapMeta self, AreaData area) {
            orig(self, area);

            if (!string.IsNullOrEmpty(self.Wipe) && self.Wipe.StartsWith("JungleHelper/SpriteWipe:")) {
                string spriteName = self.Wipe.Substring("JungleHelper/SpriteWipe:".Length);
                area.Wipe = (scene, wipeIn, onComplete) => {
                    if (spriteName == atlasPath) {
                        // the sprites are loaded in, proceed
                        new SpriteWipe(scene, wipeIn, onComplete);
                    } else {
                        // I'm sure no-one will notice, right?
                        new AngledWipe(scene, wipeIn, onComplete);
                    }
                };

                // let's make sure the wipe is in map metadata because this can get weird.
                if (area.GetMeta() != null) {
                    area.GetMeta().Wipe = self.Wipe;
                }
            }
        }

        private static void onLevelLoad(On.Celeste.LevelLoader.orig_LoadingThread orig, LevelLoader self) {
            AreaKey area = new DynData<LevelLoader>(self).Get<Session>("session").Area;
            string wipe = AreaData.Get(area)?.GetMeta()?.Wipe;
            if (wipe != null && wipe.StartsWith("JungleHelper/SpriteWipe:")) {
                string spriteName = wipe.Substring("JungleHelper/SpriteWipe:".Length);
                loadSpritesFor(spriteName);
            }

            orig(self);
        }

        private static void loadSpritesFor(string spritePath) {
            if (atlasPath != spritePath) {
                Stopwatch timer = Stopwatch.StartNew();

                if (wipeAtlas != null) {
                    Logger.Log(LogLevel.Info, "JungleHelper/SpriteWipe", $"Unloading sprites for wipe {atlasPath}");
                    wipeAtlas.Dispose();
                }

                wipeAtlas = Atlas.FromAtlas("Graphics/Atlases/Wipes/" + spritePath, Atlas.AtlasDataFormat.PackerNoAtlas);
                atlasPath = spritePath;

                timer.Stop();
                Logger.Log(LogLevel.Info, "JungleHelper/SpriteWipe", $"Loading sprites for {spritePath} took {timer.ElapsedMilliseconds} ms");
            }
        }

        private readonly List<MTexture> spritesIn;
        private readonly List<MTexture> spritesOut;

        public SpriteWipe(Scene scene, bool wipeIn, Action onComplete = null) : base(scene, wipeIn, onComplete) {
            spritesIn = wipeAtlas.GetAtlasSubtextures("wipein");
            spritesOut = wipeAtlas.GetAtlasSubtextures("wipeout");
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
