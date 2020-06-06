using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JungleHelper {
    [CustomEntity("JungleHelper/ZipMovingPlatform")]
    public class MovingZipPlatform : JumpThru {
        public MovingZipPlatform(Vector2 position, int width, Vector2 node) : base(position, width, false) {
            start = Position;
            end = node;
            base.Add(sfx = new SoundSource());
            SurfaceSoundIndex = 5;
            base.Add(new LightOcclude(0.2f));
            base.Add(new Coroutine(ZipUp(), true));
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
            orig_Added(scene);
            areaData.WoodPlatform = woodPlatform;
        }

        public override void Render() {
            textures[0].Draw(Position);
            int num = 8;
            while ((float) num < base.Width - 8f) {
                textures[1].Draw(Position + new Vector2((float) num, 0f));
                num += 8;
            }
            textures[3].Draw(Position + new Vector2(base.Width - 8f, 0f));
            textures[2].Draw(Position + new Vector2(base.Width / 2f - 4f, 0f));
        }

        public override void OnStaticMoverTrigger(StaticMover sm) {
            sinkTimer = 0.4f;
        }
        private IEnumerator ZipUp() {
            for (; ; )
            {
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
                    to = default;
                }
                StartShaking(0.2f);
                float at2 = 0f;
                while (at2 < 1f) {
                    yield return null;
                    at2 = Calc.Approach(at2, 1f, 0.5f * Engine.DeltaTime);
                    percent = 1f - Ease.SineIn(at2);
                    Vector2 to2 = Vector2.Lerp(end, start, Ease.SineIn(at2));
                    MoveTo(to2);
                    to2 = default;
                }
                StartShaking(0.1f);
                yield return 0.5f;
            }
        }
        public override void Update() {
            base.Update();
            bool flag = base.HasPlayerRider();
            bool flag2 = flag;
            if (flag2) {
                sinkTimer = 0.2f;
                addY = Calc.Approach(addY, 3f, 50f * Engine.DeltaTime);
            } else {
                bool flag3 = sinkTimer > 0f;
                if (flag3) {
                    sinkTimer -= Engine.DeltaTime;
                    addY = Calc.Approach(addY, 3f, 50f * Engine.DeltaTime);
                } else {
                    addY = Calc.Approach(addY, 0f, 20f * Engine.DeltaTime);
                }
            }
        }

        public void orig_Added(Scene scene) {
            base.Added(scene);
            MTexture mtexture = GFX.Game["objects/woodPlatform/" + TextureName];
            textures = new MTexture[mtexture.Width / 8];
            for (int i = 0; i < textures.Length; i++) {
                textures[i] = mtexture.GetSubtexture(i * 8, 0, 8, 8, null);
            }
            Vector2 value = new Vector2(base.Width, base.Height + 4f) / 2f;
            scene.Add(new MovingPlatformLine(start + value, end + value));
        }

        private Vector2 start;

        private string TextureName;

        private float percent;

        private Vector2 end;

        private float addY;

        private float sinkTimer;

        private MTexture[] textures;

        private string lastSfx;

        private SoundSource sfx;

        public string OverrideTexture;
    }
}
