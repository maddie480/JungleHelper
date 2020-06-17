
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
namespace Celeste.Mod.JungleHelper {

    [CustomEntity("JungleHelper/Hawk")]
    public class Hawk : Entity {

        private enum States {
            Wait,
            Fling,
            Move,
            WaitForLightningClear,
            Leaving
        }

        public static ParticleType P_Feather;

        public const float SkipDist = 100f;

        public static readonly Vector2 FlingSpeed = new Vector2(380f, -100f);

        private Vector2 spriteOffset = new Vector2(0f, 8f);

        private Sprite sprite;

        private States state;

        private Vector2 flingSpeed;

        private Vector2 flingTargetSpeed;

        private float flingAccel;

        private Color trailColor = Calc.HexToColor("639bff");

        private EntityData entityData;

        private SoundSource moveSfx;
        public bool LightningRemoved;

        public Hawk() {
            Add(sprite = GFX.SpriteBank.Create("bird"));
            sprite.Play("hover");
            sprite.Scale.X = -1f;
            sprite.Position = spriteOffset;
            sprite.OnFrameChange = delegate {
                BirdNPC.FlapSfxCheck(sprite);
            };
            base.Collider = new Circle(16f);
            Add(new PlayerCollider(OnPlayer));
            Add(moveSfx = new SoundSource());
            Add(new TransitionListener {
                OnOut = delegate (float t) {
                    sprite.Color = Color.White * (1f - Calc.Map(t, 0f, 0.4f));
                }
            });
        }

        public Hawk(EntityData data, Vector2 levelOffset):base(data.Position+levelOffset){
            entityData = data;
            Position = data.Position;
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            Player entity = scene.Tracker.GetEntity<Player>();
            if (entity != null && entity.X > base.X) {
                RemoveSelf();
            }
        }
        public override void Added(Scene scene) {
            base.Added(scene);
            Console.WriteLine("HEWP I'M AT "+Position);
        }
        private void Skip() {
            state = States.Move;
            Add(new Coroutine(MoveRoutine()));
        }

        private void OnPlayer(Player player) {
            if (state == States.Wait) {
                flingSpeed = player.Speed * 0.4f;
                flingSpeed.Y = 120f;
                flingTargetSpeed = Vector2.Zero;
                flingAccel = 1000f;
                player.Speed = Vector2.Zero;
                state = States.Fling;
                Add(new Coroutine(DoFlingRoutine(player)));
                Audio.Play("event:/new_content/game/10_farewell/bird_throw", base.Center);
            }
        }

        public override void Update() {
            base.Update();
            if (state != 0) {
                sprite.Position = Calc.Approach(sprite.Position, spriteOffset, 32f * Engine.DeltaTime);
            }
            switch (state) {
                case States.Move:
                    break;
                case States.Wait: {
                        Player entity = base.Scene.Tracker.GetEntity<Player>();
                        if (entity != null && entity.X - base.X >= 100f) {
                            Skip();
                        } else if (entity != null) {
                            float scaleFactor = Calc.ClampedMap((entity.Center - Position).Length(), 16f, 64f, 12f, 0f);
                            Vector2 value = (entity.Center - Position).SafeNormalize();
                        }
                        break;
                    }
                case States.Fling:
                    if (flingAccel > 0f) {
                        flingSpeed = Calc.Approach(flingSpeed, flingTargetSpeed, flingAccel * Engine.DeltaTime);
                    }
                    Position += flingSpeed * Engine.DeltaTime;
                    break;
                case States.WaitForLightningClear:
                    if (base.Scene.Entities.FindFirst<Lightning>() == null || base.X > (float) (base.Scene as Level).Bounds.Right) {
                        sprite.Scale.X = 1f;
                        state = States.Leaving;
                        Add(new Coroutine(LeaveRoutine()));
                    }
                    break;
            }
        }

        private IEnumerator DoFlingRoutine(Player player) {
            Level level = Scene as Level;
            Vector2 camera = level.Camera.Position;
            Vector2 zoom = player.Position - camera;
            zoom.X = Calc.Clamp(zoom.X, 145f, 215f);
            zoom.Y = Calc.Clamp(zoom.Y, 85f, 95f);
            Add(new Coroutine(level.ZoomTo(zoom, 1.1f, 0.2f)));
            Engine.TimeRate = 0.8f;
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            while (flingSpeed != Vector2.Zero) {
                yield return null;
            }
            sprite.Play("throw");
            sprite.Scale.X = 1f;
            flingSpeed = new Vector2(-140f, 140f);
            flingTargetSpeed = Vector2.Zero;
            flingAccel = 1400f;
            yield return 0.1f;
            Celeste.Freeze(0.05f);
            flingTargetSpeed = FlingSpeed;
            flingAccel = 6000f;
            yield return 0.1f;
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            Engine.TimeRate = 1f;
            level.Shake();
            Add(new Coroutine(level.ZoomBack(0.1f)));
            player.FinishFlingBird();
            flingTargetSpeed = Vector2.Zero;
            flingAccel = 4000f;
            yield return 0.3f;
            Add(new Coroutine(MoveRoutine()));
        }

        private IEnumerator MoveRoutine() {
            state = States.Move;
            sprite.Play("fly");
            sprite.Scale.X = 1f;
            moveSfx.Play("event:/new_content/game/10_farewell/bird_relocate");
            sprite.Rotation = 0f;
            sprite.Scale = Vector2.One;
            sprite.Play("hover");
            sprite.Scale.X = -1f;
            state = States.Wait;
            yield break;
        }

        private IEnumerator LeaveRoutine() {
            sprite.Scale.X = 1f;
            sprite.Play("fly");
            Vector2 to = new Vector2((Scene as Level).Bounds.Right + 32, Y);
            yield return MoveOnCurve(Position, (Position + to) * 0.5f - Vector2.UnitY * 12f, to);
            RemoveSelf();
        }

        private IEnumerator MoveOnCurve(Vector2 from, Vector2 anchor, Vector2 to) {
            SimpleCurve curve = new SimpleCurve(from, to, anchor);
            float duration = curve.GetLengthParametric(32) / 500f;
            Vector2 was = from;
            for (float t = 0.016f; t <= 1f; t += Engine.DeltaTime / duration) {
                Position = curve.GetPoint(t).Floor();
                sprite.Rotation = Calc.Angle(curve.GetPoint(Math.Max(0f, t - 0.05f)), curve.GetPoint(Math.Min(1f, t + 0.05f)));
                sprite.Scale.X = 1.25f;
                sprite.Scale.Y = 0.7f;
                yield return null;
            }
            Position = to;
        }

        public override void Render() {
            base.Render();
        }

        private void DrawLine(Vector2 a, Vector2 anchor, Vector2 b) {
            new SimpleCurve(a, b, anchor).Render(Color.Red, 32);
        }
    }
}