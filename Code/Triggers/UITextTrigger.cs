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
    public class UITextSeparatelyBcTriggersDontRender : Entity {
        private VirtualRenderTarget textTarget;
        public bool disposed = false;
        public float Alpha = 0f;
        public bool displayed;
        public string text;
        public Vector2 textOffset;

        public UITextSeparatelyBcTriggersDontRender(Vector2 textOffset, string text) {
            int num = Math.Min(1920, Engine.ViewWidth);
            int num2 = Math.Min(1080, Engine.ViewHeight);
            textTarget = VirtualContent.CreateRenderTarget("text", num, num2);
            this.text = text;
            this.textOffset = textOffset;
            base.Tag = ((int) Tags.HUD | (int) Tags.FrozenUpdate);
            Add(new BeforeRenderHook(BeforeRender));
        }

        private void DrawText(Vector2 offset, Color color) {
            if (text != null) {
                text = ActiveFont.FontSize.AutoNewline(text, 1024);
            }
            float num = ActiveFont.Measure(text).X * 1.5f;
            Vector2 vector = new Vector2(960f, 540f) + offset + textOffset;
            ActiveFont.Draw(text, vector, new Vector2(0.5f, 0.5f), Vector2.One * 1.5f, color);
        }

        public void BeforeRender() {
            if (!disposed) {
                Engine.Graphics.GraphicsDevice.SetRenderTarget(textTarget);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                Matrix transformationMatrix = Matrix.CreateScale((float) textTarget.Width / 1920f);
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, transformationMatrix);
                if (!string.IsNullOrEmpty(text)) {
                    DrawText(new Vector2(-2f, 0f), Color.Black);
                    DrawText(new Vector2(2f, 0f), Color.Black);
                    DrawText(new Vector2(0f, -2f), Color.Black);
                    DrawText(new Vector2(0f, 2f), Color.Black);
                    DrawText(Vector2.Zero, Color.White);
                }
                Draw.SpriteBatch.End();
               
            }
        }
        public void Dispose() {
            if (!disposed) {
                textTarget.Dispose();
                RemoveSelf();
                disposed = true;
            }
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);
            Dispose();
        }
        public override void Render() {
            Draw.SpriteBatch.Draw((RenderTarget2D) textTarget, Vector2.Zero, textTarget.Bounds, Color.White * Alpha, 0f, Vector2.Zero, 1920f / (float) textTarget.Width, SpriteEffects.None, 0f);
        }
    }
    [CustomEntity("JungleHelper/UITextTrigger")]
    public class UITextTrigger : Trigger {
        

        public UITextSeparatelyBcTriggersDontRender UItext;
        public float fadeIn = 1f;
        public float fadeOut = 1f;
        public string flag = "";
        public Coroutine fader = new Coroutine();
        public UITextTrigger(EntityData data, Vector2 offset) : base(data, offset) {

            Vector2 textOffset = new Vector2(data.Float("TextX"),data.Float("TextY"));
            fadeIn = data.Float("FadeIn",1);
            fadeOut = data.Float("FadeOut", 1);
            flag = data.Attr("Flag","");
            UItext = new UITextSeparatelyBcTriggersDontRender(textOffset, Dialog.Get(data.Attr("Dialog")));
        }
        public override void Awake(Scene scene) {
            scene.Add(UItext);
            base.Awake(scene);
        }
        public override void OnEnter(Player player) {
            if (SceneAs<Level>().Session.GetFlag(flag) && flag != "") {
                fader.RemoveSelf();
                Add(fader = new Coroutine(MakeTextAppear()));
            }
            base.OnEnter(player);
        }
        public override void OnLeave(Player player) {
            if (SceneAs<Level>().Session.GetFlag(flag) && flag != "") {
                fader.RemoveSelf();
                Add(fader = new Coroutine(MakeTextDisappear()));
            }
            base.OnLeave(player);
        }
        private IEnumerator MakeTextAppear() {
            UItext.displayed = true;
            for (float t2 = 0f; t2 < 1; t2 += Engine.RawDeltaTime/fadeIn) {
                UItext.Alpha = Ease.CubeOut(t2);
                yield return null;
            }
            UItext.Alpha = 1;
        }
        private IEnumerator MakeTextDisappear() {
            UItext.displayed = false;
            for (float t = 0f; t < 1; t += Engine.RawDeltaTime/fadeOut * 2f) {
                UItext.Alpha = Ease.CubeIn(1 - t);
                yield return null;
            }
            UItext.Alpha = 0;
        }
        private bool updateFlag;
        public override void Update() {
            if (updateFlag != SceneAs<Level>().Session.GetFlag(flag) && flag != "") {
                if (PlayerIsInside && UItext.Alpha < 0.1) {
                    fader.RemoveSelf();
                    Add(fader = new Coroutine(MakeTextAppear()));
                } else if (PlayerIsInside && UItext.Alpha > 0.9) {
                    fader.RemoveSelf();
                    Add(fader = new Coroutine(MakeTextDisappear()));
                }
                updateFlag = SceneAs<Level>().Session.GetFlag(flag);
            }
            base.Update();
        }
    }
}
