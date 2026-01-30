using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JungleHelper.Entities {
    [CustomEntity("JungleHelper/ZipMovingPlatform")]
    public class ZipMovingPlatform : JumpThru {
        public ZipMovingPlatform(Vector2 position, int width, Vector2 node) : base(position, width, false) {
            start = Position;
            end = node;
            Add(sfx = new SoundSource());
            SurfaceSoundIndex = 5;
            Add(new LightOcclude(0.2f));
            Add(new Coroutine(ZipUp(), true));
        }

        public ZipMovingPlatform(EntityData data, Vector2 offset) : this(data.Position + offset, data.Width, data.Nodes[0] + offset) {
            TextureName = data.Attr("texture", "default");
            waitTimer = data.Float("waitTimer", 0f);
            cooldownTimer = data.Float("cooldownTimer", 0.5f);
            movementMode = data.Enum<MovementModes>("movementMode");
            // old version
            if (data.Has("noReturn"))
                movementMode = data.Bool("noReturn") ? MovementModes.DisabledOnReachEnd : MovementModes.Normal;
            lineEdgeColor = data.HexColor("lineEdgeColor", Calc.HexToColor("2a1923"));
            lineInnerColor = data.HexColor("lineInnerColor", Calc.HexToColor("160b12"));
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
            scene.Add(new MovingPlatformLine(start + value, end + value, lineInnerColor, lineEdgeColor));

            areaData.WoodPlatform = woodPlatform;
        }

        public override void Render() {
            textures[0].Draw(Position + base.Shake);
            int xPosition = 8;
            while (xPosition < Width - 8f) {
                textures[1].Draw(Position + new Vector2(xPosition, 0f) + base.Shake);
                xPosition += 8;
            }

            textures[3].Draw(Position + new Vector2(base.Width - 8f, 0f) + base.Shake);
            textures[2].Draw(Position + new Vector2(base.Width / 2f - 4f, 0f) + base.Shake);
        }

        public override void OnStaticMoverTrigger(StaticMover sm) {
            sinkTimer = 0.4f;
        }

        private IEnumerator MoveTo(Vector2 from, Vector2 to, float moveSpeed, bool playSfx, float shakeDuration) {
            if (playSfx)
                sfx.Play("event:/junglehelper/sfx/Zip_platform", null, 0f);
            float at = 0f;
            while (at < 1f) {
                yield return null;
                at = Calc.Approach(at, 1f, moveSpeed * Engine.DeltaTime);
                percent = Ease.SineIn(at);
                Vector2 lerpedPosition = Vector2.Lerp(from, to, percent);
                MoveTo(lerpedPosition);
            }

            StartShaking(shakeDuration);
        }

        private IEnumerator ZipUp() {
            while (true) {
                yield return new SwapImmediately(WaitForStartMoving());
                yield return new SwapImmediately(MoveTo(start, end, 2f, true, 0.2f));
                switch (movementMode) {
                    case MovementModes.DisabledOnReachEnd:
                        yield break;
                    case MovementModes.StopOnReachEnd:
                        yield return cooldownTimer;
                        yield return new SwapImmediately(WaitForStartMoving());
                        yield return new SwapImmediately(MoveTo(end, start, 2f, true, 0.2f));
                        break;
                    case MovementModes.Normal:
                        yield return new SwapImmediately(MoveTo(end, start, 0.5f, false, 0.1f));
                        break;
                }
                yield return cooldownTimer;
            }
        }

        private IEnumerator WaitForStartMoving() {
            while (!HasPlayerRider()) {
                yield return null;
            }

            // If the platform is going to wait any time before it starts moving, shake the platform to indicate it's about to move
            if (waitTimer > 0) {
                StartShaking(waitTimer);
                yield return waitTimer;
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

        public override void MoveHExact(int move) {
            base.MoveHExact(move);

            // be sure to apply momentum to entities riding this platform.
            if (Collidable) {
                foreach (Actor entity in Scene.Tracker.GetEntities<Actor>()) {
                    if (entity.IsRiding(this)) {
                        entity.LiftSpeed = LiftSpeed;
                    }
                }
            }
        }

        public override void MoveVExact(int move) {
            base.MoveVExact(move);

            // be sure to apply momentum to entities riding this platform.
            if (Collidable) {
                foreach (Actor entity in Scene.Tracker.GetEntities<Actor>()) {
                    if (entity.IsRiding(this)) {
                        entity.LiftSpeed = LiftSpeed;
                    }
                }
            }
        }

        public enum MovementModes {
            Normal,
            DisabledOnReachEnd,
            StopOnReachEnd,
        }

        private MovementModes movementMode;

        private Vector2 start;

        private string TextureName;

        private float percent;

        private Vector2 end;

        private float addY;

        private float sinkTimer;

        private float waitTimer;

        private float cooldownTimer;

        private Color lineEdgeColor;

        private Color lineInnerColor;

        private MTexture[] textures;

        private SoundSource sfx;

        public string OverrideTexture;

        public class MovingPlatformLine : Entity {
            private Color lineEdgeColor;

            private Color lineInnerColor;

            private Vector2 end;

            public MovingPlatformLine(Vector2 position, Vector2 end, Color innerColor, Color edgeColor) {
                Position = position;
                Depth = 9001;
                this.end = end;
                lineInnerColor = innerColor;
                lineEdgeColor = edgeColor;
            }

            public override void Render() {
                Vector2 vector = (end - Position).SafeNormalize();
                Vector2 vector2 = new Vector2(-vector.Y, vector.X);
                Draw.Line(Position - vector - vector2, end + vector - vector2, lineEdgeColor);
                Draw.Line(Position - vector, end + vector, lineEdgeColor);
                Draw.Line(Position - vector + vector2, end + vector + vector2, lineEdgeColor);
                Draw.Line(Position, end, lineInnerColor);
            }
        }
    }
}