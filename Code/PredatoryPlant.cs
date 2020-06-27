using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JungleHelper {
    [CustomEntity("JungleHelper/PredatorPlant")]
    class PredatoryPlant : Entity {
        private const int TRIGGER_RADIUS_SQUARED = 40 * 40;
        private bool killPlayer = false;

        private enum Color {
            Pink, Blue, Yellow
        };

        private readonly bool facingRight;

        private bool isIdle => sprite.CurrentAnimationID == "idle" || sprite.CurrentAnimationID == "idleAlt";

        private Sprite sprite;
        private PlayerCollider bounceCollider;

        public PredatoryPlant(EntityData data, Vector2 offset) : base(data.Position + offset) {
            facingRight = data.Bool("facingRight");

            Add(sprite = JungleHelperModule.SpriteBank.Create($"predator_plant_{data.Enum("color", Color.Pink).ToString().ToLowerInvariant()}"));
            sprite.Y = -4;
            sprite.OnFinish = _ => checkRange();
            sprite.OnFrameChange = _ => updateHitbox();
            sprite.Scale.X = (facingRight ? -1 : 1);

            Add(new PlayerCollider(onCollideWithPlayer));
            Add(bounceCollider = new PlayerCollider(onJumpOnPlant));

            Collider = new Hitbox(8, 7, 3, -13);
            updateBounceCollider();
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            // spawn a block field in the plant's range.
            scene.Add(new BlockField(Position - new Vector2(16, 16), 32, 24));
        }

        public override void Update() {
            base.Update();

            // if the plant is currently idle, check if the player is close enough to trigger it.
            if (isIdle) {
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player != null && (Center - player.Center).LengthSquared() < TRIGGER_RADIUS_SQUARED &&
                    ((!facingRight && player.Left < Right) || (facingRight && player.Right > Left))) {

                    sprite.Play("attack");
                }
            }
        }

        private void checkRange() {
            if (!isIdle) {
                // the attack or knockout animation is finished. pick if the plant should be attacking or idle now.
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player != null && (Center - player.Center).LengthSquared() < TRIGGER_RADIUS_SQUARED &&
                    ((!facingRight && player.Left < Right) || (facingRight && player.Right > Left))) {

                    sprite.Play("attack");
                } else {
                    sprite.Play("idle");
                }
            }
        }

        public override void DebugRender(Camera camera) {
            base.DebugRender(camera);

            if (Collider != null && !killPlayer) {
                Collider.Render(camera, Microsoft.Xna.Framework.Color.Green);
            }
        }

        private void updateHitbox() {
            if (sprite.CurrentAnimationID == "knockout") {
                // when knocked out, the plant has no hitbox.
                Collider = null;
            } else if (isIdle) {
                // when idle, the hitbox does not move.
                Collider = new Hitbox(8, 7, 3, -13);
                killPlayer = false;
            } else {
                // when attacking, the hitbox moves with the plant's head in the animation.
                switch (sprite.CurrentAnimationFrame) {
                    case 5:
                        Collider = new Hitbox(8, 7, -8, -8);
                        break;
                    case 6:
                        Collider = new Hitbox(8, 7, -15, -5);
                        break;
                    case 11:
                        Collider = new Hitbox(8, 7, -11, -6);
                        break;
                    case 12:
                        Collider = new Hitbox(8, 7, -8, -8);
                        break;
                    case 13:
                        Collider = new Hitbox(8, 7, -6, -10);
                        break;
                    case 14:
                        Collider = new Hitbox(8, 7, -4, -10);
                        break;
                    case 15:
                        Collider = new Hitbox(8, 7, 0, -12);
                        break;
                    default:
                        Collider = new Hitbox(8, 7, 3, -13);
                        break;
                }

                killPlayer = sprite.CurrentAnimationFrame >= 5 && sprite.CurrentAnimationFrame < 11;
            }

            updateBounceCollider();
        }

        public void updateBounceCollider() {
            if (Collider == null) {
                bounceCollider.Collider = null;
            } else {
                if (facingRight) {
                    // reflect the hitbox horizontally.
                    Collider.Left = -Collider.Right;
                }

                // update the bounce collider position to place it on top of the kill collider.
                bounceCollider.Collider = new Hitbox(Collider.Width, 2f, Collider.Left, Collider.Top);
                Collider.Height -= 2f;
                Collider.Top += 2f;

                // if the plant is attacking, make the bounce hitbox wider.
                if (sprite.CurrentAnimationID == "attack") {
                    bounceCollider.Collider.Width += 5f;
                    bounceCollider.Collider.Left -= (facingRight ? 3f : 2f);
                }
            }
        }

        private void onCollideWithPlayer(Player player) {
            if (killPlayer) {
                // kill
                Vector2 deathDirection = player.Center - Center;
                deathDirection.Normalize();
                player.Die(deathDirection);
            }
        }

        private void onJumpOnPlant(Player player) {
            if (!CollideCheck<Player>() && player.Speed.Y > 0f) {
                // player is moving down on the plant and doesn't collide with it > bop
                Celeste.Freeze(0.1f);
                player.Bounce(Top);
                Audio.Play("event:/game/general/thing_booped", Position);

                sprite.Play("knockout");
            }
        }
    }
}
