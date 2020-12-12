using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.JungleHelper.Entities {
    [CustomEntity("JungleHelper/Cockatiel")]
    class Cockatiel : Entity {
        private Sprite sprite;

        public Cockatiel(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Depth = -9999;

            StateMachine stateMachine = new StateMachine();
            stateMachine.SetCallbacks(0, idleUpdate, null);
            stateMachine.SetCallbacks(1, aggressiveUpdate, null, aggressiveBegin);
            stateMachine.SetCallbacks(2, null, flyAwayRoutine);
            Add(stateMachine);

            sprite = JungleHelperModule.SpriteBank.Create("cockatiel");
            sprite.Scale.X = data.Bool("facingLeft") ? 1 : -1;
            Add(sprite);
        }

        private int idleUpdate() {
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null && (Position - player.Center).LengthSquared() < 48 * 48) {
                // if the player is at less than 48px, the cockatiel gets aggressive.
                return 1;
            }
            return 0;
        }

        private void aggressiveBegin() {
            sprite.Play("aggressive");
        }

        private int aggressiveUpdate() {
            Player player = Scene.Tracker.GetEntity<Player>();

            if (player != null) {
                // stare at the player aggressively... but in the right direction please
                int facing = Math.Sign(X - player.X);
                if (facing != 0) {
                    sprite.Scale.X = facing;
                }
            }

            if (player == null || (Position - player.Center).LengthSquared() >= 48 * 48) {
                // player backed out or died: cockatiel calms down.
                sprite.Play("backout");
                return 0;
            }
            if (player != null && (Position - player.Center).LengthSquared() < 32 * 32) {
                // player got even closer: fly away.
                return 2;
            }
            return 1;
        }

        // mostly ripped from FlutterBird
        private IEnumerator flyAwayRoutine() {
            int direction = Math.Sign(X - (Scene.Tracker.GetEntity<Player>()?.X ?? 0));
            float delay = Calc.Random.NextFloat(0.2f);
            Level level = Scene as Level;

            Add(new SoundSource("event:/game/general/bird_startle") { RemoveOnOneshotEnd = true });

            yield return delay;

            // start flying away
            sprite.Play("flyaway");
            sprite.Scale.X = direction;
            level.ParticlesFG.Emit(Calc.Random.Choose(ParticleTypes.Dust), Position, -(float) Math.PI / 2f);

            // speed up
            Vector2 from = Position;
            Vector2 to = Position + new Vector2(direction * 4, -8f);
            for (float p = 0f; p < 1f; p += Engine.DeltaTime * 3f) {
                Position = from + (to - from) * Ease.CubeOut(p);
                yield return null;
            }

            Depth = -10001;

            // continue flying up
            sprite.Scale.X = 0f - sprite.Scale.X;
            Vector2 speed = new Vector2(direction, -4f) * 8f;
            while (Y + 8f > level.Bounds.Top) {
                speed += new Vector2(direction * 64, -128f) * Engine.DeltaTime;
                Position += speed * Engine.DeltaTime;
                yield return null;
            }

            // wait for sounds to be done.
            while (Get<SoundSource>() != null) {
                yield return null;
            }

            // done! we're off-screen.
            RemoveSelf();
        }
    }
}
