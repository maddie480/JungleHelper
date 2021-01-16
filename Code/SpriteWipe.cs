using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Celeste.Mod.JungleHelper {
    // Useful when you want to do a wipe, but you know how to sprite better than you know how to code.
    // To make the wipe obey colouring, make your sprites white.
    class SpriteWipe : ScreenWipe {
        public static void Load() {
            On.Celeste.Mod.Meta.MapMeta.ApplyTo += onParseScreenWipe;
            On.Celeste.LevelEnter.Go += onLevelEnter;

            using (new DetourContext { After = { "*" } }) { // if another mod hooks LevelLoader.StartLevel, we should be called first to intercept it.
                On.Celeste.LevelLoader.StartLevel += onLevelStart;
            }
        }

        public static void Unload() {
            On.Celeste.Mod.Meta.MapMeta.ApplyTo -= onParseScreenWipe;
            On.Celeste.LevelEnter.Go -= onLevelEnter;
            On.Celeste.LevelLoader.StartLevel -= onLevelStart;
        }

        private static Atlas wipeAtlas;
        private static string atlasPath;
        private static Task spriteLoadingTask;

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

        private static void onLevelEnter(On.Celeste.LevelEnter.orig_Go orig, Session session, bool fromSaveData) {
            runIfMapUsesSpriteWipe(session.Area, spritePath => {
                // start loading sprites ahead of time.
                Logger.Log("JungleHelper/SpriteWipe", $"Starting loading sprites for {spritePath} (from LevelEnter)...");
                startLoadingSpritesFor(spritePath);
            });

            orig(session, fromSaveData);
        }

        private static void onLevelStart(On.Celeste.LevelLoader.orig_StartLevel orig, LevelLoader self) {
            bool goingToSpriteWipeLoadingScreen = false;

            if (spriteLoadingTask != null && (!spriteLoadingTask.IsCompleted && !spriteLoadingTask.IsFaulted && !spriteLoadingTask.IsCanceled)) {
                // sprite loading was started on LevelEnter, but is not done yet. show the loading screen
                Logger.Log("JungleHelper/SpriteWipe", "Loading sprites is ongoing, transitioning to the loading screen...");
                Engine.Scene = new SpriteWipeLoadingScreen(self, spriteLoadingTask);
                goingToSpriteWipeLoadingScreen = true;
            } else if (spriteLoadingTask == null) {
                // no sprite wipe has been done, check if there should have been one...
                runIfMapUsesSpriteWipe(self.Level.Session.Area, spritePath => {
                    // we didn't start the sprite loading on level enter (console load possibly?), so do that right now.
                    Logger.Log("JungleHelper/SpriteWipe", $"Starting loading sprites for {spritePath} (from LevelLoader)...");
                    startLoadingSpritesFor(spritePath);
                    Engine.Scene = new SpriteWipeLoadingScreen(self, spriteLoadingTask);
                    goingToSpriteWipeLoadingScreen = true;
                });
            }

            // forget the sprite loading task, it's either done or we passed over to the loading screen.
            spriteLoadingTask = null;

            // only run vanilla LevelLoader.StartLevel if we are not going to the loading screen.
            if (!goingToSpriteWipeLoadingScreen) {
                orig(self);
            }
        }

        /// <summary>
        /// Runs the given action if the given area uses the sprite wipe.
        /// The parameter given to the action is the path of the sprite wipe.
        /// </summary>
        private static void runIfMapUsesSpriteWipe(AreaKey area, Action<string> toRun) {
            string wipe = AreaData.Get(area)?.GetMeta()?.Wipe;
            if (wipe != null && wipe.StartsWith("JungleHelper/SpriteWipe:")) {
                string spriteName = wipe.Substring("JungleHelper/SpriteWipe:".Length);
                if (atlasPath != spriteName) {
                    toRun(spriteName);
                }
            }
        }

        private static void startLoadingSpritesFor(string spritePath) {
            spriteLoadingTask = new Task(() => {
                try {
                    // allow Everest to identify this is a thread that can load textures silently, so that AssetReloadHelper doesn't kick in...
                    // because triggering it on level enter messes up scene transitions and makes you stuck on a black screen.
                    // we're providing our own loading screen anyway.
                    Thread.CurrentThread.Name = "Jungle Helper Sprite Wipe Loading Thread";

                    Stopwatch timer = Stopwatch.StartNew();

                    // unload the previous wipe
                    if (wipeAtlas != null) {
                        Logger.Log(LogLevel.Info, "JungleHelper/SpriteWipe", $"Unloading sprites for wipe {atlasPath}");
                        wipeAtlas.Dispose();
                    }

                    // load the new wipe
                    wipeAtlas = Atlas.FromAtlas("Graphics/Atlases/Wipes/" + spritePath, Atlas.AtlasDataFormat.PackerNoAtlas);
                    atlasPath = spritePath;

                    // log time
                    timer.Stop();
                    Logger.Log(LogLevel.Info, "JungleHelper/SpriteWipe", $"Loading sprites for {spritePath} ({wipeAtlas.Textures.Count} texture(s)) took {timer.ElapsedMilliseconds} ms");

                } catch (Exception e) {
                    // log and rethrow the exception, so that it leaves a trace in log.txt.
                    Logger.LogDetailed(e, "JungleHelper/SpriteWipe");
                    throw;
                }
            });
            spriteLoadingTask.Start();
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
