using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monocle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;

namespace Celeste.Mod.JungleHelper.Triggers {
    class UITextTrigger:Trigger {
        private VirtualRenderTarget textTarget;
        public bool disposed;
        public float Alpha = 1f;
        public bool displayed;
        public float TextAlpha = 1f;
        public string text;
        private IEnumerator MakeTextAppear() {
            displayed = true;
            for (float t2 = 0f; t2< 1f; t2 += Engine.RawDeltaTime)
	        {
		        Alpha = Ease.CubeOut(t2);
		        yield return null;
	        }
        }
        private IEnumerator MakeTextDisappear() {
            displayed = true;
            for (float t = 0f; t < 1f; t += Engine.RawDeltaTime * 2f) {
                Alpha = Ease.CubeIn(1f - t);
                yield return null;
            }
        }
        public override void Update() {
            if (PlayerIsInside && !displayed) {
                Add(new Coroutine(MakeTextAppear()));
            } else if (!PlayerIsInside && displayed) {
                Add(new Coroutine(MakeTextDisappear()));
            }
            base.Update();
        }

        private void DrawText(Vector2 offset, Color color) {
            if (text != null) {
                text = ActiveFont.FontSize.AutoNewline(text, 1024);
            }
            MTexture mTexture = GFX.Gui["textside"];
            float num = ActiveFont.Measure(text).X * 1.5f;
            Vector2 vector = new Vector2(960f, 540f) + offset;
            mTexture.DrawCentered(vector - Vector2.UnitX * (num / 2f + 64f), color);
            ActiveFont.Draw(text, vector, new Vector2(0.5f, 0.5f), Vector2.One * 1.5f, color);
            mTexture.DrawCentered(vector + Vector2.UnitX * (num / 2f + 64f), color);
        }
        public UITextTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            text = Dialog.Get(data.Attr("Dialog", "CH5_BSIDE_THEO_B"));
            base.Tag = ((int) Tags.HUD | (int) Tags.FrozenUpdate);
            Add(new BeforeRenderHook(BeforeRender));
        }

        public void BeforeRender() {
            if (!disposed) {
                Engine.Graphics.GraphicsDevice.SetRenderTarget(textTarget);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                Matrix transformationMatrix = Matrix.CreateScale((float) textTarget.Width / 1920f);
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, transformationMatrix);
                if (!string.IsNullOrEmpty(text)) {
                    DrawText(new Vector2(-2f, 0f), Color.Black * TextAlpha);
                    DrawText(new Vector2(2f, 0f), Color.Black * TextAlpha);
                    DrawText(new Vector2(0f, -2f), Color.Black * TextAlpha);
                    DrawText(new Vector2(0f, 2f), Color.Black * TextAlpha);
                    DrawText(Vector2.Zero, Color.White * TextAlpha);
                }
                Draw.SpriteBatch.End();
                //MagicGlow.Render((RenderTarget2D) text, timer, -1f, Matrix.CreateScale(0.5f));
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
            Draw.SpriteBatch.Draw((RenderTarget2D) textTarget, Vector2.Zero, textTarget.Bounds, Color.White*Alpha, 0f, Vector2.Zero, 1920f / (float) textTarget.Width, SpriteEffects.None, 0f);
            base.Render();
        }
    }
}
