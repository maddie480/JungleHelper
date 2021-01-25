using System;
using Monocle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using Celeste.Mod.Entities;

namespace Celeste.Mod.JungleHelper.Triggers {
    public class UIImage : Entity {
        private VirtualRenderTarget imageTarget;
        private bool disposed = false;
        private MTexture image;
        private Vector2 imageOffset;

        public float Alpha = 0f;
        public bool Displayed;

        public UIImage(Vector2 imageOffset, MTexture image) {
            int width = Math.Min(1920, Engine.ViewWidth);
            int height = Math.Min(1080, Engine.ViewHeight);
            imageTarget = VirtualContent.CreateRenderTarget("text", width, height);
            this.image = image;
            this.imageOffset = imageOffset;
            Tag = Tags.HUD | Tags.FrozenUpdate;
            Add(new BeforeRenderHook(BeforeRender));
        }

        private void drawImage(Color color) {
            if (!SceneAs<Level>().FrozenOrPaused) {
                Vector2 position = imageOffset;
                image.Draw(position, Vector2.Zero, color);
            }
        }

        public void BeforeRender() {
            if (!disposed) {
                Engine.Graphics.GraphicsDevice.SetRenderTarget(imageTarget);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                Matrix transformationMatrix = Matrix.CreateScale(imageTarget.Width / 1920f);
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, transformationMatrix);
                drawImage(Color.White);
                Draw.SpriteBatch.End();
            }
        }

        private void dispose() {
            if (!disposed) {
                imageTarget.Dispose();
                RemoveSelf();
                disposed = true;
            }
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);
            dispose();
        }

        public override void SceneEnd(Scene scene) {
            base.SceneEnd(scene);
            dispose();
        }

        public override void Render() {
            Draw.SpriteBatch.Draw(imageTarget, Vector2.Zero, imageTarget.Bounds, Color.White * Alpha, 0f, Vector2.Zero, 1920f / imageTarget.Width, SpriteEffects.None, 0f);
        }
    }
    [CustomEntity("JungleHelper/UIImageTrigger")]
    public class UIImageTrigger : Trigger {
        private UIImage img;
        private float fadeIn = 1f;
        private float fadeOut = 1f;
        private string flag = "";
        private Coroutine fader = new Coroutine();

        public UIImageTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            Vector2 imageOffset = new Vector2(data.Float("ImageX"), data.Float("ImageY"));
            fadeIn = data.Float("FadeIn", 1);
            fadeOut = data.Float("FadeOut", 1);
            flag = data.Attr("Flag", "");
            img = new UIImage(imageOffset, GFX.Gui[data.Attr("ImagePath")]);
        }

        public override void Awake(Scene scene) {
            scene.Add(img);
            base.Awake(scene);
        }

        public override void OnEnter(Player player) {
            if (SceneAs<Level>().Session.GetFlag(flag) || flag == "") {
                fader.RemoveSelf();
                Add(fader = new Coroutine(makeImageAppear()));
            }
            base.OnEnter(player);
        }

        public override void OnLeave(Player player) {
            if (SceneAs<Level>().Session.GetFlag(flag) || flag == "") {
                fader.RemoveSelf();
                Add(fader = new Coroutine(makeImageDisappear()));
            }
            base.OnLeave(player);
        }

        private IEnumerator makeImageAppear() {
            img.Displayed = true;
            for (float t = 0f; t < 1; t += Engine.RawDeltaTime / fadeIn) {
                img.Alpha = Ease.CubeOut(t);
                yield return null;
            }
            img.Alpha = 1;
        }

        private IEnumerator makeImageDisappear() {
            img.Displayed = false;
            for (float t = 0f; t < 1; t += Engine.RawDeltaTime / fadeOut) {
                img.Alpha = Ease.CubeIn(1 - t);
                yield return null;
            }
            img.Alpha = 0;
        }

        private bool flagWasActive;
        public override void Update() {
            if (flag != "" && flagWasActive != SceneAs<Level>().Session.GetFlag(flag)) {
                if (PlayerIsInside && img.Alpha < 0.1) {
                    fader.RemoveSelf();
                    Add(fader = new Coroutine(makeImageAppear()));
                } else if (PlayerIsInside && img.Alpha > 0.9) {
                    fader.RemoveSelf();
                    Add(fader = new Coroutine(makeImageDisappear()));
                }
                flagWasActive = SceneAs<Level>().Session.GetFlag(flag);
            }
            base.Update();
        }
    }
}
