﻿
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using FMOD;

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
        private float origY;
        private Vector2 flingSpeed;

        private Vector2 flingTargetSpeed;

        private float flingAccel;

        private Color trailColor = Calc.HexToColor("639bff");

        private EntityData entityData;

        private SoundSource moveSfx;
        public bool LightningRemoved;
        private SineWave sine;
        private float hawkSpeed;
        float playerSpeed;
        float playerlessSpeed;
        
        public Hawk(EntityData data, Vector2 levelOffset):base(data.Position+levelOffset) {
            entityData = data;
            Position = data.Position;
            playerSpeed = data.Float("mainSpeed");
            playerlessSpeed = data.Float("slowerSpeed");
            Add(sprite = JungleHelperModule.SpriteBank.Create("hawk"));
            sprite.Play("hover");
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

        public override void Awake(Scene scene) {
            base.Awake(scene);
            Player entity = scene.Tracker.GetEntity<Player>();
            if (entity != null && entity.X > base.X) {
                RemoveSelf();
            }
        }
        public override void Added(Scene scene) {
            base.Added(scene);
        }
        private void Skip() {
            state = States.Move;
            Add(new Coroutine(MoveRoutine()));
        }

        private void OnPlayer(Player player) {
            if ((CollideFirst<Solid>(Position) == null) && (state == States.Wait || state == States.Move)) {
                origY = Y;
                flingSpeed = player.Speed * 0.4f;
                flingSpeed.Y = 120f;
                flingTargetSpeed = Vector2.Zero;
                flingAccel = 1000f;
                player.Speed = Vector2.Zero;
                state = States.Fling;
                if (hawkSpeed == 0f) {
                    sprite.Play("throw");
                }
                Add(new Coroutine(DoFlingRoutine(player)));
            }
        }

        public override void Update() {
            base.Update();
            if (state != 0) {
                sprite.Position = Calc.Approach(sprite.Position, spriteOffset, 32f * Engine.DeltaTime);
            }
            switch (state) {
                case States.Move:
                    X += playerlessSpeed * Engine.DeltaTime;
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
            if (X >= (SceneAs<Level>().Bounds.Right + 5)) {
                RemoveSelf();

                Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
                if (player != null){
                    if (player.StateMachine.State == 11) {
                        player.StateMachine.State = 0;
                        player.ForceCameraUpdate = false;
                        player.DummyAutoAnimate = true;
                    }
                }
            }
        }

        private IEnumerator DoFlingRoutine(Player player) {
            sprite.Play("fly");
            Level level = SceneAs<Level>();
            hawkSpeed = 0;
            sprite.Scale.X = 1f;
            while (state == States.Fling) {
                yield return null;
                Y = Calc.Approach(Y, origY, 20f * Engine.DeltaTime);
                if (hawkSpeed <= playerSpeed) {
                    hawkSpeed = Calc.Approach(hawkSpeed, playerSpeed, playerSpeed/10);
                }
                X += hawkSpeed * Engine.DeltaTime;
                player.StateMachine.State = 11;
                player.ForceCameraUpdate = true;
                player.DummyAutoAnimate = false;
                player.Sprite.Play("fallSlow_carry");
                player.X = X;
                player.Y = Y + 16;
                if (Input.Jump.Pressed) {
                    player.StateMachine.State = 0;
                    break;
                }
                if (player.CollideFirst<Solid>(player.Position + new Vector2(hawkSpeed * Engine.DeltaTime, 0)) != null) 
                {
                    player.StateMachine.State = 0;
                    break;
                }
                if (Input.Dash.Pressed && player.CanDash) {
                    player.StateMachine.State = 2;
                    break;
                }
            }
            player.ForceCameraUpdate = false;
            player.DummyAutoAnimate = true;
            Add(new Coroutine(MoveRoutine()));
        }

        private IEnumerator MoveRoutine() {
            sprite.Play("fly");
            //sprite.Scale.X = 1f;
            sprite.Rotation = 0f;
            sprite.Scale = Vector2.One;
            //sprite.Scale.X = -1f;
            X += 80f * Engine.DeltaTime;
            yield return 0.1f;
            state = States.Move;
            if (state == States.Fling) {
                yield break;
            }
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