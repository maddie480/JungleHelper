using System;
using Monocle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using Celeste.Mod.Entities;

namespace Celeste.Mod.JungleHelper.Triggers {
    public class UIText : Entity {
        private VirtualRenderTarget textTarget;
        private bool disposed = false;
        private string text;
        private bool newlineApplied = false;
        private Vector2 textOffset;

        public float Alpha = 0f;
        public bool Displayed;

        public UIText(Vector2 textOffset, string text) {
            int width = Math.Min(1920, Engine.ViewWidth);
            int height = Math.Min(1080, Engine.ViewHeight);
            textTarget = VirtualContent.CreateRenderTarget("text", width, height);
            this.text = text;
            this.textOffset = textOffset;
            Tag = Tags.HUD | Tags.FrozenUpdate;
            Add(new BeforeRenderHook(BeforeRender));
        }

        private void drawText(Vector2 offset, Color color) {
            if (!SceneAs<Level>().FrozenOrPaused) {
                if (text != null && !newlineApplied) {
                    text = ActiveFont.FontSize.AutoNewline(text, 1024);
                    newlineApplied = true;
                }
                Vector2 position = new Vector2(960f, 540f) + offset + textOffset;
                ActiveFont.Draw(text, position, new Vector2(0.5f, 0.5f), Vector2.One * 1.5f, color);
            }
        }

        public void BeforeRender() {
            if (!disposed) {
                Engine.Graphics.GraphicsDevice.SetRenderTarget(textTarget);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                Matrix transformationMatrix = Matrix.CreateScale(textTarget.Width / 1920f);
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, transformationMatrix);
                if (!string.IsNullOrEmpty(text)) {
                    drawText(new Vector2(-2f, 0f), Color.Black);
                    drawText(new Vector2(2f, 0f), Color.Black);
                    drawText(new Vector2(0f, -2f), Color.Black);
                    drawText(new Vector2(0f, 2f), Color.Black);
                    drawText(Vector2.Zero, Color.White);
                }
                Draw.SpriteBatch.End();

            }
        }

        private void dispose() {
            if (!disposed) {
                textTarget.Dispose();
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
            Draw.SpriteBatch.Draw(textTarget, Vector2.Zero, textTarget.Bounds, Color.White * Alpha, 0f, Vector2.Zero, 1920f / textTarget.Width, SpriteEffects.None, 0f);
        }
    }

    [CustomEntity("JungleHelper/UITextTrigger")]
    public class UITextTrigger : Trigger {
        private UIText uiText;
        private float fadeIn = 1f;
        private float fadeOut = 1f;
        private string flag = "";
        private Coroutine fader = new Coroutine();

        public UITextTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            Vector2 textOffset = new Vector2(data.Float("TextX"), data.Float("TextY"));
            fadeIn = data.Float("FadeIn", 1);
            fadeOut = data.Float("FadeOut", 1);
            flag = data.Attr("Flag", "");
            uiText = new UIText(textOffset, Dialog.Get(data.Attr("Dialog")));
        }

        public override void Awake(Scene scene) {
            scene.Add(uiText);
            base.Awake(scene);
        }

        public override void OnEnter(Player player) {
            if (SceneAs<Level>().Session.GetFlag(flag) || flag == "") {
                fader.RemoveSelf();
                Add(fader = new Coroutine(makeTextAppear()));
            }
            base.OnEnter(player);
        }

        public override void OnLeave(Player player) {
            if (SceneAs<Level>().Session.GetFlag(flag) || flag == "") {
                fader.RemoveSelf();
                Add(fader = new Coroutine(makeTextDisappear()));
            }
            base.OnLeave(player);
        }

        private IEnumerator makeTextAppear() {
            uiText.Displayed = true;
            for (float t = 0f; t < 1; t += Engine.RawDeltaTime / fadeIn) {
                uiText.Alpha = Ease.CubeOut(t);
                yield return null;
            }
            uiText.Alpha = 1;
        }

        private IEnumerator makeTextDisappear() {
            uiText.Displayed = false;
            for (float t = 0f; t < 1; t += Engine.RawDeltaTime / fadeOut) {
                uiText.Alpha = Ease.CubeIn(1 - t);
                yield return null;
            }
            uiText.Alpha = 0;
        }

        private bool flagWasActive;
        public override void Update() {
            if (flag != "" && flagWasActive != SceneAs<Level>().Session.GetFlag(flag)) {
                if (PlayerIsInside && uiText.Alpha < 0.1) {
                    fader.RemoveSelf();
                    Add(fader = new Coroutine(makeTextAppear()));
                } else if (PlayerIsInside && uiText.Alpha > 0.9) {
                    fader.RemoveSelf();
                    Add(fader = new Coroutine(makeTextDisappear()));
                }
                flagWasActive = SceneAs<Level>().Session.GetFlag(flag);
            }

            base.Update();
        }
    }
}
