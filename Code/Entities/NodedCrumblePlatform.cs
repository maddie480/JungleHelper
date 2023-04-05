using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Utils;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.JungleHelper.Entities {
    [CustomEntity("JungleHelper/NodedCrumblePlatform")]
    public class NodedCrumblePlatform : CrumblePlatform {
        private List<Image> outline;
        private Coroutine outlineFader;

        private VirtualRenderTarget ghostVersion;

        private readonly Vector2[] allPositions;

        public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            NodedCrumblePlatform platform = new NodedCrumblePlatform(entityData, offset);
            platform.OverrideTexture = entityData.Attr("texture");
            return platform;
        }

        public NodedCrumblePlatform(EntityData data, Vector2 offset) : base(data, offset) {
            allPositions = new Vector2[data.Nodes.Length + 1];
            allPositions[0] = data.Position + offset;
            for (int i = 0; i < data.Nodes.Length; i++) {
                allPositions[i + 1] = data.Nodes[i] + offset;
            }

            Add(new Coroutine(moveBetweenNodesRoutine()));
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            DynData<CrumblePlatform> self = new DynData<CrumblePlatform>(this);
            outline = self.Get<List<Image>>("outline");
            outlineFader = self.Get<Coroutine>("outlineFader");

            // place a "ghost" platform at every position the platform can take.
            foreach (Vector2 position in allPositions) {
                scene.Add(new GhostPlatform(this, position));
            }
        }

        private IEnumerator moveBetweenNodesRoutine() {
            int currentNode = 0;

            while (true) {
                // wait for the crumble block to crumble.
                while (Collidable) {
                    yield return null;
                }

                outlineFader.Replace(blinkRoutine());

                // the move will last 2 seconds, start to move after 1 second.
                yield return 1f;

                // move the crumble block now: get the start position and the next position.
                float progress = 0f;
                Vector2 startPosition = Position;
                currentNode++;
                currentNode %= allPositions.Length;
                Vector2 nextPosition = allPositions[currentNode];

                // ease the crumble block to its next position.
                while (progress < 1f) {
                    Position = Vector2.Lerp(startPosition, nextPosition, Ease.CubeOut(progress));

                    progress += Engine.DeltaTime;
                    yield return null;
                }
                Position = nextPosition;

                // make sure we wait for the crumble block to respawn.
                while (!Collidable) {
                    yield return null;
                }
            }
        }

        private IEnumerator blinkRoutine() {
            float time = 0f;
            while (true) {
                time += Engine.DeltaTime;

                float opacity = (0.2f + Ease.SineInOut(Calc.YoYo(time % 1f)) * 0.8f);
                Color color = Color.Lerp(Color.Black, Color.White, opacity);
                foreach (Image img in outline) {
                    img.Color = color;
                }
                yield return null;
            }
        }

        public override void Render() {
            base.Render();

            if (ghostVersion == null) {
                // we are going to render the platform in greyscale on a separate render target, so that we can render it at the different positions.
                ghostVersion = VirtualContent.CreateRenderTarget("noded-crumble-platform-ghost", (int) Width, (int) Height);

                // switch rendering to the noded platform ghost.
                GameplayRenderer.End();
                Engine.Instance.GraphicsDevice.SetRenderTarget(ghostVersion);
                Engine.Instance.GraphicsDevice.Clear(Color.Transparent);

                // start drawing with a greyscale color grade.
                ColorGrade.Set(GFX.ColorGrades["JungleHelper/greyscale"]);
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, ColorGrade.Effect);

                // draw the crumble platform at (0, 0).
                Vector2 oldPosition = Position;
                Position = Vector2.Zero;
                base.Render();
                Position = oldPosition;

                // end drawing.
                Draw.SpriteBatch.End();

                // switch back to rendering gameplay.
                Engine.Instance.GraphicsDevice.SetRenderTarget(GameplayBuffers.Gameplay);
                GameplayRenderer.Begin();
            }
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);

            ghostVersion?.Dispose();
            ghostVersion = null;
        }

        public override void SceneEnd(Scene scene) {
            base.SceneEnd(scene);

            ghostVersion?.Dispose();
            ghostVersion = null;
        }

        private class GhostPlatform : Entity {
            private readonly NodedCrumblePlatform parent;

            private float time = 0f;

            public GhostPlatform(NodedCrumblePlatform parent, Vector2 position) : base(position) {
                this.parent = parent;

                Depth = 1;
            }

            public override void Update() {
                base.Update();

                time += Engine.DeltaTime;
            }

            public override void Render() {
                base.Render();

                if (parent.ghostVersion != null) {
                    Draw.SpriteBatch.Draw(parent.ghostVersion, Position, Color.White * (0.20f + Ease.SineInOut(Calc.YoYo((time / 2f) % 1f)) * 0.15f));
                }
            }
        }
    }
}
