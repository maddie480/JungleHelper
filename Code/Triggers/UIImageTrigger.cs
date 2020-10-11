using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monocle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using Celeste.Mod.Entities;

namespace Celeste.Mod.JungleHelper.Triggers {
    public class UIImage : Entity {
        private VirtualRenderTarget imageTarget;
        public bool disposed = false;
        public float Alpha = 0f;
        public bool displayed;
        public MTexture image;
        public Vector2 imageOffset;

        public UIImage(Vector2 imageOffset, MTexture image) {
            int num = Math.Min(1920, Engine.ViewWidth);
            int num2 = Math.Min(1080, Engine.ViewHeight);
            imageTarget = VirtualContent.CreateRenderTarget("text", num, num2);
            this.image = image;
            this.imageOffset = imageOffset;
            base.Tag = ((int) Tags.HUD | (int) Tags.FrozenUpdate);
            Add(new BeforeRenderHook(BeforeRender));
        }

        private void DrawImage(Vector2 offset, Color color) {
            Vector2 vector = /*new Vector2(960f, 540f) +*/ offset + imageOffset;
            Console.WriteLine(Alpha.ToString());
            image.Draw(vector, Vector2.Zero, color);
        }

        public void BeforeRender() {
            if (!disposed) {
                Engine.Graphics.GraphicsDevice.SetRenderTarget(imageTarget);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                Matrix transformationMatrix = Matrix.CreateScale((float) imageTarget.Width / 1920f);
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, transformationMatrix);
                DrawImage(Vector2.Zero, Color.White);
                Draw.SpriteBatch.End();

            }
        }
        public void Dispose() {
            if (!disposed) {
                imageTarget.Dispose();
                RemoveSelf();
                disposed = true;
            }
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);
            Dispose();
        }
        public override void Render() {
            Draw.SpriteBatch.Draw((RenderTarget2D) imageTarget, Vector2.Zero, imageTarget.Bounds, Color.White * Alpha, 0f, Vector2.Zero, 1920f / (float) imageTarget.Width, SpriteEffects.None, 0f);
        }
    }
    [CustomEntity("JungleHelper/UIImageTrigger")]
    public class UIImageTrigger : Trigger {


        public UIImage img;
        public float fadeIn = 1f;
        public float fadeOut = 1f;
        public string flag = "";
        public Coroutine fader = new Coroutine();
        public UIImageTrigger(EntityData data, Vector2 offset) : base(data, offset) {

            Vector2 imageOffset = new Vector2(data.Float("ImageX"), data.Float("ImageY"));
            fadeIn = data.Float("FadeIn", 1);
            fadeOut = data.Float("FadeOut", 1);
            flag = data.Attr("Flag", "");
            img = new UIImage(imageOffset,GFX.Gui[data.Attr("ImagePath")]);
        }
        public override void Awake(Scene scene) {
            scene.Add(img);
            base.Awake(scene);
        }
        public override void OnEnter(Player player) {
            if (SceneAs<Level>().Session.GetFlag(flag) || flag == "") {
                fader.RemoveSelf();
                Add(fader = new Coroutine(MakeImageAppear()));
            }
            base.OnEnter(player);
        }
        public override void OnLeave(Player player) {
            if (SceneAs<Level>().Session.GetFlag(flag) || flag == "") {
                fader.RemoveSelf();
                Add(fader = new Coroutine(MakeImageDisappear()));
            }
            base.OnLeave(player);
        }
        private IEnumerator MakeImageAppear() {
            img.displayed = true;
            for (float t2 = 0f; t2 < 1; t2 += Engine.RawDeltaTime / fadeIn) {
                img.Alpha = Ease.CubeOut(t2);
                yield return null;
            }
            img.Alpha = 1;
        }
        private IEnumerator MakeImageDisappear() {
            img.displayed = false;
            for (float t = 0f; t < 1; t += Engine.RawDeltaTime / fadeOut * 2f) {
                img.Alpha = Ease.CubeIn(1 - t);
                yield return null;
            }
            img.Alpha = 0;
        }
        private bool updateFlag;
        public override void Update() {
            if (updateFlag != SceneAs<Level>().Session.GetFlag(flag)) {
                if (PlayerIsInside && img.Alpha < 0.1) {
                    fader.RemoveSelf();
                    Add(fader = new Coroutine(MakeImageAppear()));
                } else if (PlayerIsInside && img.Alpha > 0.9) {
                    fader.RemoveSelf();
                    Add(fader = new Coroutine(MakeImageDisappear()));
                }
                updateFlag = SceneAs<Level>().Session.GetFlag(flag);
            }
            base.Update();
        }
    }
}
