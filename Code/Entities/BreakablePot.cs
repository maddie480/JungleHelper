using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.JungleHelper.Entities {
    [CustomEntity("JungleHelper/BreakablePot")]
    class BreakablePot : Actor {
        private Image rupee;
        private Sprite sprite;
        private Holdable hold;

        private Vector2 speed;

        private readonly EntityID potID;
        private readonly bool containsKey;

        public BreakablePot(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset) {
            containsKey = data.Bool("containsKey", defaultValue: false);
            potID = id;

            if (!containsKey) {
                // set up the rupee first, so that it is behind the pot.
                Add(rupee = new Image(GFX.Game[data.Attr("rupeeImage", "JungleHelper/Breakable Pot/rupee")]));
                rupee.CenterOrigin();
                rupee.Y = -8;
                rupee.X = 1;
            }

            // set up the pot sprite.
            Add(sprite = JungleHelperModule.CreateReskinnableSprite(data, "breakable_pot"));

            // make the pot holdable.
            Add(hold = new Holdable());
            hold.PickupCollider = new Hitbox(17f, 20f, -8f, -20f);
            hold.SlowRun = false;
            hold.OnPickup = onPickup;
            hold.OnRelease = onRelease;
            hold.SpeedGetter = () => speed;
            Collider = hold.PickupCollider;
        }

        private void onPickup() {
            speed = Vector2.Zero;
            AddTag(Tags.Persistent);

            // give the pot the same hitbox width as Madeline, odd stuff is going to happen otherwise.
            Collider.Width = 8f;
            Collider.Position.X = -4f;
        }

        private void onRelease(Vector2 force) {
            RemoveTag(Tags.Persistent);
            if (force.X != 0f && force.Y == 0f) {
                force.Y = -0.4f;
            }
            speed = force * 200f;

            // restore the normal pot hitbox.
            Collider.Width = 17f;
            Collider.Position.X = -8f;

            // vanilla code ensured we weren't in a wall before, by dragging the pot down.
            // if **now** we're colliding with a wall, we should move the pot out of it horizontally!
            if (CollideCheck<Solid>()) {
                // try to move the pot by increasing distances, and stop when we got out of the wall (by 100px max, just in case).
                int displacement = 0;
                for (int safe = 0; safe < 100; safe++) {
                    displacement++;

                    // move it left?
                    if (!CollideCheck<Solid>(Position - Vector2.UnitX * displacement)) {
                        Position -= Vector2.UnitX * displacement;
                        break;
                    }

                    // move it right?
                    if (!CollideCheck<Solid>(Position + Vector2.UnitX * displacement)) {
                        Position += Vector2.UnitX * displacement;
                        break;
                    }
                }
            }
        }

        public override void Update() {
            base.Update();

            // stop right here if the pot isn't holdable or if the player is holding it!
            if (hold == null || hold.Holder != null) {
                return;
            }

            // enforce gravity.
            if (!OnGround()) {
                float acceleration = 800f;
                if (Math.Abs(speed.Y) <= 30f) {
                    acceleration *= 0.5f;
                }
                speed.Y = Calc.Approach(speed.Y, 200f, acceleration * Engine.DeltaTime);
            }

            // move the pot around, breaking it if it hits anything.
            MoveV(speed.Y * Engine.DeltaTime, _ => breakPot());
            MoveH(speed.X * Engine.DeltaTime, _ => breakPot());
        }

        private void breakPot() {
            if (speed.LengthSquared() > 200 * 200) {
                // pot isn't holdable anymore.
                Remove(hold);
                hold = null;

                // play breaking animation and sound, and animate the rupee (or spawn the key).
                sprite.Play("break");
                if (containsKey) {
                    SoundSource sound = new SoundSource("event:/junglehelper/sfx/pot_ding") { RemoveOnOneshotEnd = true };
                    Add(sound);
                    Add(new Coroutine(animateKeyRoutine(sound)));
                } else {
                    Add(new SoundSource("event:/junglehelper/sfx/ch2_secret_ding"));
                    Add(new Coroutine(animateRupeeRoutine()));
                }
            }
        }

        private IEnumerator animateRupeeRoutine() {
            // rupee goes up
            float p = 0f;
            while (p < 1f) {
                rupee.Y = -5 - Ease.CubeOut(p) * 20f;
                yield return null;
                p += 3f * Engine.DeltaTime;
            }
            rupee.Y = -25f;

            // wait for a bit
            yield return 0.1f;

            // rupee fades out
            float a = 1f;
            while (a > 0f) {
                rupee.Color = Color.White * a;
                yield return null;
                a -= 4f * Engine.DeltaTime;
            }

            // entity is now invisible so it can go away.
            RemoveSelf();
        }

        private IEnumerator animateKeyRoutine(SoundSource sound) {
            // spawn the key! it shouldn't be collectable by the player, though.
            Key key = new Key(Center, potID, new Vector2[0]);
            key.Depth = 1;
            key.Collidable = false;
            Scene.Add(key);
            yield return null; // make sure the key is actually added to the scene by waiting for a frame

            // animate it coming out of the pot.
            float p = 0f;
            while (p < 1f && !key.CollideCheck<Solid>()) {
                key.Y = Center.Y - 5 - Ease.CubeOut(p) * 20f;
                yield return null;
                p += 3f * Engine.DeltaTime;
            }

            // set final position.
            if (!key.CollideCheck<Solid>()) {
                key.Y = Center.Y - 25f;
            }

            // auto collect the key if player is still alive.
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null) {
                SceneAs<Level>().Particles.Emit(Key.P_Collect, 10, key.Position, Vector2.One * 3f);
                player.Leader.GainFollower(key.Get<Follower>());
                Collidable = false;
                Session session = SceneAs<Level>().Session;
                session.DoNotLoad.Add(key.ID);
                session.Keys.Add(key.ID);
                session.UpdateLevelStartDashes();
                key.Get<Wiggler>().Start();
                key.Depth = -1000000;
            }

            // wait until the pot animation is finished
            while (sprite.Animating) {
                yield return null;
            }

            // wait for sound to end
            while (sound.Scene != null) {
                yield return null;
            }

            // entity is now invisible and doesn't emit any sound so it can go away.
            RemoveSelf();
        }
    }
}
