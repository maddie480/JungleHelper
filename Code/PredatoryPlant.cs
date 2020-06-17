using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JungleHelper {
    [CustomEntity("JungleHelper/PredatorPlant")]
    class PredatoryPlant : Entity {
        private const int TRIGGER_RADIUS_SQUARED = 40 * 40;

        private enum Color {
            Pink, Blue, Yellow
        };

        private Sprite sprite;
        private PlayerCollider bounceCollider;

        public PredatoryPlant(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Add(sprite = JungleHelperModule.SpriteBank.Create($"predator_plant_{data.Enum("color", Color.Pink).ToString().ToLowerInvariant()}"));
            sprite.Y = -4;
            sprite.OnFinish = _ => checkRange();
            sprite.OnFrameChange = _ => updateHitbox();

            Add(new PlayerCollider(onCollideWithPlayer));
            Add(bounceCollider = new PlayerCollider(onJumpOnPlant));

            Collider = new Hitbox(15, 11, 1, -15);
            updateBounceCollider();
        }

        public override void Update() {
            base.Update();

            // if the plant is currently idle, check if the player is close enough to trigger it.
            if (sprite.CurrentAnimationID == "idle") {
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player != null && (Center - player.Center).LengthSquared() < TRIGGER_RADIUS_SQUARED) {
                    sprite.Play("attack");
                }
            }
        }

        private void checkRange() {
            // the attack or knockout animation is finished. pick if the plant should be attacking or idle now.
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null && (Center - player.Center).LengthSquared() < TRIGGER_RADIUS_SQUARED) {
                sprite.Play("attack");
            } else {
                sprite.Play("idle");
            }
        }

        private void updateHitbox() {
            if (sprite.CurrentAnimationID == "knockout") {
                // when knocked out, the plant has no hitbox.
                Collider = null;
            } else if (sprite.CurrentAnimationID == "idle") {
                // when idle, the hitbox does not move.
                Collider = new Hitbox(15, 11, 1, -15);
            } else {
                // when attacking, the hitbox moves with the plant's head in the animation.
                switch (sprite.CurrentAnimationFrame) {
                    case 5:
                        Collider = new Hitbox(15, 11, -9, -10);
                        break;
                    case 6:
                        Collider = new Hitbox(15, 11, -16, -7);
                        break;
                    case 11:
                        Collider = new Hitbox(15, 11, -12, -8);
                        break;
                    case 12:
                        Collider = new Hitbox(15, 11, -9, -10);
                        break;
                    case 13:
                        Collider = new Hitbox(15, 11, -7, -12);
                        break;
                    case 14:
                        Collider = new Hitbox(15, 11, -5, -12);
                        break;
                    case 15:
                        Collider = new Hitbox(15, 11, -1, -14);
                        break;
                    default:
                        Collider = new Hitbox(15, 11, 1, -15);
                        break;
                }
            }

            updateBounceCollider();
        }

        public void updateBounceCollider() {
            if (Collider == null) {
                bounceCollider.Collider = null;
            } else {
                // update the bounce collider position to place it on top of the kill collider.
                bounceCollider.Collider = new Hitbox(Collider.Width, 2f, Collider.Left, Collider.Top);
                Collider.Height -= 2f;
                Collider.Top += 2f;
            }
        }

        private void onCollideWithPlayer(Player player) {
            // kill
            Vector2 deathDirection = player.Center - Center;
            deathDirection.Normalize();
            player.Die(deathDirection);
        }

        private void onJumpOnPlant(Player player) {
            Celeste.Freeze(0.1f);
            player.Bounce(Top);
            Audio.Play("event:/game/general/thing_booped", Position);

            sprite.Play("knockout");
        }
    }
}
