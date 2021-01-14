using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.JungleHelper.Entities {
    [CustomEntity("JungleHelper/BreakablePot")]
    class BreakablePot : Actor {
        private Image rupee;
        private Sprite sprite;
        private Holdable hold;

        private Vector2 speed;

        public BreakablePot(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Add(rupee = new Image(GFX.Game[data.Attr("rupeeImage", "JungleHelper/Breakable Pot/rupee")]));
            rupee.CenterOrigin();
            rupee.Y = -8;
            rupee.X = 1;
            Add(sprite = JungleHelperModule.CreateReskinnableSprite(data, "breakable_pot"));

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
        }

        private void onRelease(Vector2 force) {
            RemoveTag(Tags.Persistent);
            if (force.X != 0f && force.Y == 0f) {
                force.Y = -0.4f;
            }
            speed = force * 200f;
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
            MoveV(speed.Y * Engine.DeltaTime, _ => breakPot());
            MoveH(speed.X * Engine.DeltaTime, _ => breakPot());
        }

        private void breakPot() {
            if (speed.LengthSquared() > 200 * 200) {
                // pot isn't holdable anymore.
                Remove(hold);
                hold = null;

                // play breaking animation and sound, and animate the rupee.
                sprite.Play("break");
                Add(new SoundSource("event:/junglehelper/sfx/ch2_secret_ding"));
                Add(new Coroutine(animateRupeeRoutine()));
            }
        }

        private IEnumerator animateRupeeRoutine() {
            float p = 0f;
            while (p < 1f) {
                rupee.Y = -5 - Ease.CubeOut(p) * 20f;
                yield return null;
                p += 3f * Engine.DeltaTime;
            }
            rupee.Y = -25f;

            yield return 0.1f;

            float a = 1f;
            while (a > 0f) {
                rupee.Color = Color.White * a;
                yield return null;
                a -= 4f * Engine.DeltaTime;
            }

            RemoveSelf();
        }
    }
}
