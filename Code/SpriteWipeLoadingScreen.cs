using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Reflection;
using System.Threading.Tasks;

namespace Celeste.Mod.JungleHelper {
    // ~~An AssetReloadHelper ripoff~~ A loading screen that is displayed while sprite wipes are loaded.
    class SpriteWipeLoadingScreen : Scene {
        private static MethodInfo levelLoaderStartLevel = typeof(LevelLoader).GetMethod("StartLevel", BindingFlags.NonPublic | BindingFlags.Instance);

        // parameters
        private LevelLoader levelLoader;
        private Task taskToFollow;

        private bool done => taskToFollow.IsCompleted || taskToFollow.IsFaulted || taskToFollow.IsCanceled;

        // rendering-related variables
        private const float timeIn = 0.3f;
        private const float timeOut = 0.15f;
        private float time;
        private bool transitionOutBegan;
        private MTexture cogwheel;

        public SpriteWipeLoadingScreen(LevelLoader levelLoader, Task taskToFollow) {
            this.levelLoader = levelLoader;
            this.taskToFollow = taskToFollow;

            cogwheel = GFX.Gui["reloader/cogwheel"];
        }

        public override void Update() {
            base.Update();

            if (!transitionOutBegan && done) {
                // task is just done! start transitioning out.
                time = 0f;
                transitionOutBegan = true;
            }

            if (transitionOutBegan && time > timeOut) {
                // fade out is over, go on with the level.
                Logger.Log("JungleHelper/SpriteWipeLoadingScreen", $"Transitioning to level");
                levelLoaderStartLevel.Invoke(levelLoader, new object[0]);
                if (!(Engine.NextScene is Level)) {
                    Engine.Scene = levelLoader.Level;
                }
            }
        }

        public override void Render() {
            // time passes.
            time += Engine.RawDeltaTime;

            Draw.SpriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Engine.ScreenMatrix
            );

            // compute the fade state.
            float t = transitionOutBegan ? 1f - time / timeOut : (time / timeIn);
            float a = Ease.SineInOut(Calc.Clamp(t, 0, 1));

            // draw the cogwheel.
            Vector2 anchor = new Vector2(96f, 96f);
            Vector2 pos = anchor + new Vector2(0f, 0f);
            float cogScale = MathHelper.Lerp(0.2f, 0.25f, Ease.CubeOut(a));
            if (!(cogwheel?.Texture?.Texture?.IsDisposed ?? true)) {
                float cogRot = RawTimeActive * 4f;
                for (int x = -2; x <= 2; x++)
                    for (int y = -2; y <= 2; y++)
                        if (x != 0 || y != 0)
                            cogwheel.DrawCentered(pos + new Vector2(x, y), Color.Black * a * a * a * a, cogScale, cogRot);
                cogwheel.DrawCentered(pos, Color.White * a, cogScale, cogRot);
            }

            // draw the text.
            pos = anchor + new Vector2(48f, 0f);
            Vector2 size = ActiveFont.Measure(Dialog.Clean("junglehelper_spritewipe_loading"));
            ActiveFont.DrawOutline(Dialog.Clean("junglehelper_spritewipe_loading"), pos + new Vector2(size.X * 0.5f, 0f), new Vector2(0.5f, 0.5f), Vector2.One * MathHelper.Lerp(0.8f, 1f, Ease.CubeOut(a)), Color.White * a, 2f, Color.Black * a * a * a * a);

            Draw.SpriteBatch.End();
        }

    }
}
