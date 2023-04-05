using Celeste.Mod.Entities;
using Celeste.Mod.JungleHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.JungleHelper.Entities {
    [CustomEntity("JungleHelper/PredatorPlant")]
    public class PredatorPlant : Entity {
        private enum Color {
            Pink, Blue, Yellow
        };

        private readonly bool facingRight;
        private readonly string spritePath;

        private Vector2 imageOffset;

        private bool isIdle => sprite.CurrentAnimationID == "idle" || sprite.CurrentAnimationID == "idleAlt";

        private Sprite sprite;
        private PlayerCollider bounceCollider;
        private SlopedColliderWithRectangles triggerCollider;
        private BlockField blockfield;

        private string cassetteColor;

        public PredatorPlant(EntityData data, Vector2 offset) : base(data.Position + offset) {
            facingRight = data.Bool("facingRight");
            spritePath = data.Attr("sprite");

            Add(sprite = JungleHelperModule.CreateReskinnableSprite(data, $"predator_plant_{data.Enum("color", Color.Pink).ToString().ToLowerInvariant()}"));
            sprite.Y = -4;
            sprite.OnFinish = _ => checkRange();
            sprite.OnFrameChange = _ => updateHitbox();
            sprite.Scale.X = (facingRight ? -1 : 1);

            Add(new PlayerCollider(onCollideWithPlayer));
            Add(bounceCollider = new PlayerCollider(onJumpOnPlant));
            Add(new StaticMover() {
                OnShake = OnShake,
                SolidChecker = CheckAttachToSolid,
                JumpThruChecker = jt => jt.CollideRect(new Rectangle((int) (Position.X + (facingRight ? -16 : 0)), (int) Position.Y + 8, 16, 1)),
                OnMove = move => {
                    Position += move;
                    blockfield.Position += move;
                    triggerCollider.Move(move);
                },
                OnEnable = OnEnable,
                OnDisable = OnDisable
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
                if (player != null && Collidable && triggerCollider.Collide(player.Collider)) {
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
                if (player != null && Collidable && triggerCollider.Collide(player.Collider)) {
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
                // displayed frame changes on frames 0, 8, 10, 11, 12, 13, 22, 24, 26, 28 and 30
                switch (sprite.CurrentAnimationFrame) {
                    case 10:
                        Collider = new Hitbox(8, 9, -4, -10);
                        break;
                    case 11:
                        Collider = new Hitbox(8, 9, -10, -8);
                        break;
                    case 12:
                    case 13:
                        Collider = new Hitbox(8, 9, -15, -5);
                        break;
                    case 22:
                        Collider = new Hitbox(8, 9, -11, -6);
                        break;
                    case 24:
                        Collider = new Hitbox(8, 9, -8, -8);
                        break;
                    case 26:
                        Collider = new Hitbox(8, 9, -6, -10);
                        break;
                    case 28:
                        Collider = new Hitbox(8, 9, -4, -10);
                        break;
                    case 30:
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

        // handling of predator plants attached to cassette blocks

        private bool CheckAttachToSolid(Solid solid) {
            bool collides = solid.CollideRect(new Rectangle((int) (Position.X + (facingRight ? -16 : 0)), (int) Position.Y + 8, 16, 1));
            if (collides && solid is CassetteBlock cassetteBlock) {
                // check the color of the cassette block we're attached to, to apply it to the plant.
                switch (cassetteBlock.Index) {
                    case 0:
                    default:
                        cassetteColor = "blue";
                        break;
                    case 1:
                        cassetteColor = "pink";
                        break;
                    case 2:
                        cassetteColor = "yellow";
                        break;
                    case 3:
                        cassetteColor = "green";
                        break;
                }
            }
            return collides;
        }

        private void OnEnable() {
            // make the plant collidable (to enable it).
            Collidable = true;

            if (cassetteColor != null) {
                // change the animation to the active one.
                string currentAnimationId = sprite.CurrentAnimationID;
                int currentAnimationFrame = sprite.CurrentAnimationFrame;
                JungleHelperModule.CreateReskinnableSpriteOn(sprite, string.IsNullOrEmpty(spritePath) ? "" : spritePath + "_cassette_active", $"cassette_predator_plant_{cassetteColor}_active");
                sprite.Y = -4;
                sprite.Play(currentAnimationId);
                sprite.SetAnimationFrame(currentAnimationFrame);
            } else {
                // we're not attached to a cassette block: make the plant visible instead.
                Visible = true;
            }
        }

        private void OnDisable() {
            // make the plant uncollidable (to disable it).
            Collidable = false;

            if (cassetteColor != null) {
                // change the animation to the inactive one.
                string currentAnimationId = sprite.CurrentAnimationID;
                int currentAnimationFrame = sprite.CurrentAnimationFrame;
                JungleHelperModule.CreateReskinnableSpriteOn(sprite, string.IsNullOrEmpty(spritePath) ? "" : spritePath + "_cassette_inactive", $"cassette_predator_plant_{cassetteColor}_inactive");
                sprite.Y = -4;
                sprite.Play(currentAnimationId);
                sprite.SetAnimationFrame(currentAnimationFrame);
            } else {
                // we're not attached to a cassette block: make the plant invisible instead.
                Visible = false;
            }
        }
    }
}
