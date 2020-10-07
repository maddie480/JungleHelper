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

        public UITextSeparatelyBcTriggersDontRender(Vector2 position, string text) {
            int num = Math.Min(1920, Engine.ViewWidth);
            int num2 = Math.Min(1080, Engine.ViewHeight);
            textTarget = VirtualContent.CreateRenderTarget("text", num, num2);
            base.Tag = ((int) Tags.HUD | (int) Tags.FrozenUpdate);
            Add(new BeforeRenderHook(BeforeRender));
        }

        private void DrawText(Vector2 offset, Color color) {
            if (text != null) {
                text = ActiveFont.FontSize.AutoNewline(text, 1024);
            }
            MTexture mTexture = GFX.Gui["poemside"];
            float num = ActiveFont.Measure(text).X * 1.5f;
            Vector2 vector = new Vector2(960f, 540f) + offset;
            mTexture.DrawCentered(vector - Vector2.UnitX * (num / 2f + 64f), color);
            ActiveFont.Draw(text, vector, new Vector2(0.5f, 0.5f), Vector2.One * 1.5f, color);
            mTexture.DrawCentered(vector + Vector2.UnitX * (num / 2f + 64f), color);
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
        public UITextTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            UItext = new UITextSeparatelyBcTriggersDontRender(Vector2.Zero, Dialog.Get(data.Attr("text")));
        }
        public override void Awake(Scene scene) {
            scene.Add(UItext);
            base.Awake(scene);
        }
        public override void OnEnter(Player player) {
            Add(new Coroutine(MakeTextAppear()));
            base.OnEnter(player);
        }
        public override void OnLeave(Player player) {
            UItext.Add(new Coroutine(MakeTextDisappear()));
            base.OnLeave(player);
        }
        private IEnumerator MakeTextAppear() {
            UItext.displayed = true;
            for (float t2 = 0f; t2 < 1f; t2 += Engine.RawDeltaTime) {
                UItext.Alpha = Ease.CubeOut(t2);
                yield return null;
            }
        }
        private IEnumerator MakeTextDisappear() {
            UItext.displayed = false;
            for (float t = 0f; t < 1f; t += Engine.RawDeltaTime * 2f) {
                UItext.Alpha = Ease.CubeIn(1f - t);
                yield return null;
            }
        }
    }
}
