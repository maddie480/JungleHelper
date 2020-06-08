using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JungleHelper {
    [CustomEntity("JungleHelper/ZipMovingPlatform")]
    public class MovingZipPlatform : JumpThru {
        public MovingZipPlatform(Vector2 position, int width, Vector2 node) : base(position, width, false) {
            start = Position;
            end = node;
            Add(sfx = new SoundSource());
            SurfaceSoundIndex = 5;
            Add(new LightOcclude(0.2f));
            Add(new Coroutine(ZipUp(), true));
        }

        public MovingZipPlatform(EntityData data, Vector2 offset) : this(data.Position + offset, data.Width, data.Nodes[0] + offset) {
            TextureName = data.Attr("texture", "default");
        }

        public override void Added(Scene scene) {
            AreaData areaData = AreaData.Get(scene);
            string woodPlatform = areaData.WoodPlatform;
            if (OverrideTexture != null) {
                areaData.WoodPlatform = OverrideTexture;
            }

            base.Added(scene);
            MTexture mtexture = GFX.Game["objects/woodPlatform/" + TextureName];
            textures = new MTexture[mtexture.Width / 8];
            for (int i = 0; i < textures.Length; i++) {
                textures[i] = mtexture.GetSubtexture(i * 8, 0, 8, 8, null);
            }
            Vector2 value = new Vector2(Width, Height + 4f) / 2f;
            scene.Add(new MovingPlatformLine(start + value, end + value));

            areaData.WoodPlatform = woodPlatform;
        }

        public override void Render() {
            textures[0].Draw(Position);
            int xPosition = 8;
            while (xPosition < Width - 8f) {
                textures[1].Draw(Position + new Vector2(xPosition, 0f));
                xPosition += 8;
            }
            textures[3].Draw(Position + new Vector2(base.Width - 8f, 0f));
            textures[2].Draw(Position + new Vector2(base.Width / 2f - 4f, 0f));
        }

        public override void OnStaticMoverTrigger(StaticMover sm) {
            sinkTimer = 0.4f;
        }
        private IEnumerator ZipUp() {
            while (true) {
                while (!HasPlayerRider()) {
                    yield return null;
                }
                sfx.Play("event:/junglehelper/sfx/Zip_platform", null, 0f);
                float at = 0f;
                while (at < 1f) {
                    yield return null;
                    at = Calc.Approach(at, 1f, 2f * Engine.DeltaTime);
                    percent = Ease.SineIn(at);
                    Vector2 to = Vector2.Lerp(start, end, percent);
                    MoveTo(to);
                }
                StartShaking(0.2f);
                at = 0f;
                while (at < 1f) {
                    yield return null;
                    at = Calc.Approach(at, 1f, 0.5f * Engine.DeltaTime);
                    percent = 1f - Ease.SineIn(at);
                    Vector2 to = Vector2.Lerp(end, start, Ease.SineIn(at));
                    MoveTo(to);
                }
                StartShaking(0.1f);
                yield return 0.5f;
            }
        }

        public override void Update() {
            base.Update();
            if (HasPlayerRider()) {
                sinkTimer = 0.2f;
                addY = Calc.Approach(addY, 3f, 50f * Engine.DeltaTime);
            } else {
                if (sinkTimer > 0f) {
                    sinkTimer -= Engine.DeltaTime;
                    addY = Calc.Approach(addY, 3f, 50f * Engine.DeltaTime);
                } else {
                    addY = Calc.Approach(addY, 0f, 20f * Engine.DeltaTime);
                }
            }
        }

        private Vector2 start;

        private string TextureName;

        private float percent;

        private Vector2 end;

        private float addY;

        private float sinkTimer;

        private MTexture[] textures;

        private SoundSource sfx;

        public string OverrideTexture;
    }
}
