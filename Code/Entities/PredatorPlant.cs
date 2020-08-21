using Celeste.Mod.Entities;
using Celeste.Mod.JungleHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JungleHelper.Entities {
    [CustomEntity("JungleHelper/PredatorPlant")]
    class PredatorPlant : Entity {
        private enum Color {
            Pink, Blue, Yellow
        };

        private readonly bool facingRight;

        private Vector2 imageOffset;

        private bool isIdle => sprite.CurrentAnimationID == "idle" || sprite.CurrentAnimationID == "idleAlt";

        private Sprite sprite;
        private PlayerCollider bounceCollider;
        private SlopedColliderWithRectangles triggerCollider;
        private BlockField blockfield;

        public PredatorPlant(EntityData data, Vector2 offset) : base(data.Position + offset) {
            facingRight = data.Bool("facingRight");

            Add(sprite = JungleHelperModule.SpriteBank.Create($"predator_plant_{data.Enum("color", Color.Pink).ToString().ToLowerInvariant()}"));
            sprite.Y = -4;
            sprite.OnFinish = _ => checkRange();
            sprite.OnFrameChange = _ => updateHitbox();
            sprite.Scale.X = (facingRight ? -1 : 1);

            Add(new PlayerCollider(onCollideWithPlayer));
            Add(bounceCollider = new PlayerCollider(onJumpOnPlant));
            Add(new StaticMover() {
                OnShake = OnShake,
                SolidChecker = s => s.CollideRect(new Rectangle((int) (Position.X + (facingRight ? -16 : 0)), (int) Position.Y + 8, 16, 1)),
                JumpThruChecker = jt => jt.CollideRect(new Rectangle((int) (Position.X + (facingRight ? -16 : 0)), (int) Position.Y + 8, 16, 1)),
                OnMove = move => {
                    Position += move;
                    blockfield.Position += move;
                    triggerCollider.Move(move);
                }
            });

            Collider = new Hitbox(8, 9, 3, -13);
            updateBounceCollider();

            triggerCollider = new SlopedColliderWithRectangles(Position.Y - 16, Position.Y + 8,
                Position.X - (facingRight ? 16 : 0), Position.X + (facingRight ? 0 : 16), Position.X - 16, Position.X + 16);
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            // spawn a block field in the plant's range.
            scene.Add(blockfield = new BlockField(Position - new Vector2(16, 16), 32, 24));
        }

        public override void Update() {
            base.Update();

            // if the plant is currently idle, check if the player is close enough to trigger it.
            if (isIdle) {
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player != null && triggerCollider.Collide(player.Collider)) {
                    sprite.Play("attack");
                }
            }
        }

        private void OnShake(Vector2 amount) {
            imageOffset += amount;
        }

        public override void Render() {
            Vector2 position = Position;
            Position += imageOffset;
            base.Render();
            Position = position;
        }

        public override void DebugRender(Camera camera) {
            base.DebugRender(camera);

            triggerCollider.Render(camera, Microsoft.Xna.Framework.Color.Blue);
        }

        private void checkRange() {
            if (!isIdle) {
                // the attack or knockout animation is finished. pick if the plant should be attacking or idle now.
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player != null && triggerCollider.Collide(player.Collider)) {
                    sprite.Play("attack");
                } else {
                    sprite.Play("idle");
                }
            }
        }

        private void updateHitbox() {
            if (sprite.CurrentAnimationID == "knockout") {
                // when knocked out, the plant has no hitbox.
                Collider = null;
            } else if (isIdle) {
                // when idle, the hitbox does not move.
                Collider = new Hitbox(8, 9, 3, -13);
            } else {
                // when attacking, the hitbox moves with the plant's head in the animation.
                switch (sprite.CurrentAnimationFrame) {
                    case 5:
                        Collider = new Hitbox(8, 9, -8, -8);
                        break;
                    case 6:
                        Collider = new Hitbox(8, 9, -15, -5);
                        break;
                    case 11:
                        Collider = new Hitbox(8, 9, -11, -6);
                        break;
                    case 12:
                        Collider = new Hitbox(8, 9, -8, -8);
                        break;
                    case 13:
                        Collider = new Hitbox(8, 9, -6, -10);
                        break;
                    case 14:
                        Collider = new Hitbox(8, 9, -4, -10);
                        break;
                    case 15:
                        Collider = new Hitbox(8, 9, 0, -12);
                        break;
                    default:
                        Collider = new Hitbox(8, 9, 3, -13);
                        break;
                }
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
                bounceCollider.Collider = new Hitbox(Collider.Width, 8f, Collider.Left, Collider.Top - 2);
                Collider.Height -= 4f;
                Collider.Top += 6f;
            }
        }

        private void onCollideWithPlayer(Player player) {
            // kill
            Vector2 deathDirection = player.Center - Center;
            deathDirection.Normalize();
            player.Die(deathDirection);
        }

        private void onJumpOnPlant(Player player) {
            if (!CollideCheck<Player>()) {
                // player is moving down on the plant and doesn't collide with it > bop
                Celeste.Freeze(0.1f);
                player.Bounce(Top);
                Audio.Play("event:/game/general/thing_booped", Position);

                sprite.Play("knockout");
            }
        }
    }
}
