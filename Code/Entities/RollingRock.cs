using Celeste.Mod.Entities;
using Celeste.Mod.JungleHelper.Components;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;

namespace Celeste.Mod.JungleHelper.Entities {
    [CustomEntity("JungleHelper/RollingRock")]
    class RollingRock : Actor {
        public static void Load() {
            IL.Celeste.DashBlock.Removed += onDashBlockRemove;
        }

        public static void Unload() {
            IL.Celeste.DashBlock.Removed -= onDashBlockRemove;
        }

        // component used to tell dash blocks not to freeze the game on remove
        private class NoFreezeComponent : Component {
            public NoFreezeComponent() : base(false, false) { }
        }

        private static void onDashBlockRemove(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(0.05f))) {
                Logger.Log("JungleHelper/RollingRock", $"Modifying freeze at {cursor.Index} in IL for DashBlock.Remove");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, DashBlock, float>>((orig, self) => {
                    if (self.Get<NoFreezeComponent>() != null) {
                        return 0f; // no freeze!
                    }
                    return orig;
                });
            }
        }

        // configuration constants
        private const float SPEED = 100f;
        private const float FALLING_SPEED = 200f;
        private const float FALLING_ACCELERATION = 400f;
        private const float ROTATION_SPEED = 2f;

        // state info
        private Sprite sprite;
        private bool rolling = false;
        private bool falling = false;
        private bool shattered = false;
        private float fallingSpeed = 0f;

        private readonly string debrisSpriteDirectory;
        private readonly string flag;

        private Rectangle levelBounds;

        public RollingRock(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Add(sprite = JungleHelperModule.CreateReskinnableSprite(data.Attr("spriteXmlName"), "rolling_rock"));
            if (data.Bool("cracked")) {
                sprite.Play("rolling_cracked");
            }

            debrisSpriteDirectory = data.Attr("debrisSpriteDirectory", "JungleHelper/RollingRock");
            flag = data.Attr("flag");

            Collider = new CircleColliderWithRectangles(32);
            Add(new PlayerCollider(onPlayer));

            IgnoreJumpThrus = true;
            AllowPushing = false;

            Add(new TransitionListener {
                OnOutBegin = () => {
                    if (!levelBounds.Contains(Collider.Bounds)) {
                        // boulder went at least partially off-screen, hide it so that it doesn't pop out on transition end.
                        Visible = false;
                    }
                },
                OnInBegin = () => {
                    if (Scene != null && !SceneAs<Level>().Bounds.Contains(Collider.Bounds)) {
                        // boulder isn't entirely onscreen: hide it for the time of the transition, or the player will be able to see it.
                        Visible = false;
                    }
                },
                OnInEnd = () => Visible = true
            });
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            levelBounds = SceneAs<Level>().Bounds;
        }

        private void onPlayer(Player player) {
            // kill the player.
            if (player.Position != Position) {
                player.Die(Vector2.Normalize(player.Position - Position));
            } else {
                player.Die(Vector2.Zero);
            }
        }

        protected override void OnSquish(CollisionData data) {
            // just being safe: we don't want the boulder to disappear when it gets squished, because this is just silly
        }

        public override void Update() {
            base.Update();

            if (shattered) {
                // continue moving right until the break animation ends.
                Position.X += SPEED * Engine.DeltaTime;
                return;
            }

            if (falling) {
                // move down.
                fallingSpeed = Calc.Approach(fallingSpeed, FALLING_SPEED, FALLING_ACCELERATION * Engine.DeltaTime);
                MoveV(fallingSpeed * Engine.DeltaTime, hitGroundWhileFalling);
            } else if (rolling) {
                // check if we *should* be falling.
                if (!CollideCheck<Solid>(Position + Vector2.UnitY)) {
                    falling = true;
                }
            }

            if (rolling) {
                // move right.
                MoveH(SPEED * Engine.DeltaTime, hitSolidWhileMovingForward);

                // rotate the boulder sprite.
                sprite.Rotation += ROTATION_SPEED * Engine.DeltaTime;
            }

            if (!falling && !rolling) {
                // we are waiting. fall down when player moved and is on the right.
                Player player = Scene.Tracker.GetEntity<Player>();
                if ((string.IsNullOrEmpty(flag) || SceneAs<Level>().Session.GetFlag(flag)) && player != null && !player.JustRespawned && player.X > Right) {
                    falling = true;
                }
            }
        }

        private void hitGroundWhileFalling(CollisionData collisionData) {
            if (collisionData.Hit is DashBlock dashBlock) {
                // we landed on a dash block: break it!
                breakDashBlock(dashBlock);

                // we continue falling, but the dash block "stopped" the boulder.
                fallingSpeed = 0f;
            } else {
                // we don't fall anymore.
                falling = false;
                fallingSpeed = 0f;

                // BOOM
                Audio.Play("event:/junglehelper/sfx/BoulderBoss_impact", BottomCenter);
                SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, BottomCenter, -1.57079637f);

                // if we weren't moving forward, we are now!
                if (!rolling) {
                    rolling = true;
                }
            }
        }

        private void hitSolidWhileMovingForward(CollisionData collisionData) {
            if (collisionData.Hit is DashBlock dashBlock) {
                // we want the boulder to break dash blocks.
                breakDashBlock(dashBlock);
            } else {
                // but if it touches anything else, it shatters.
                shatter();
            }
        }

        private void breakDashBlock(DashBlock dashBlock) {
            dashBlock.Break(Center, Vector2.UnitY, true);
            SceneAs<Level>().Shake(0.2f);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            dashBlock.Add(new NoFreezeComponent());
        }

        private void shatter() {
            shattered = true;
            Collidable = false;

            Audio.Play("event:/junglehelper/sfx/BoulderBoss_Break", Center);

            sprite.Play("break");
            sprite.OnFinish += _ => {
                // Spawn the debris. Welcome to the Hardcode Zone!
                Scene.Add(new RockDebris(Position, sprite.Rotation, debrisSpriteDirectory, "00", new Vector2(42, 41)));
                Scene.Add(new RockDebris(Position, sprite.Rotation, debrisSpriteDirectory, "01", new Vector2(63, 38)));
                Scene.Add(new RockDebris(Position, sprite.Rotation, debrisSpriteDirectory, "02", new Vector2(63, 61)));
                Scene.Add(new RockDebris(Position, sprite.Rotation, debrisSpriteDirectory, "03", new Vector2(47, 53)));
                Scene.Add(new RockDebris(Position, sprite.Rotation, debrisSpriteDirectory, "04", new Vector2(47, 69)));
                Scene.Add(new RockDebris(Position, sprite.Rotation, debrisSpriteDirectory, "05", new Vector2(40, 75)));
                Scene.Add(new RockDebris(Position, sprite.Rotation, debrisSpriteDirectory, "06", new Vector2(29, 63)));
                Scene.Add(new RockDebris(Position, sprite.Rotation, debrisSpriteDirectory, "07", new Vector2(14, 64)));
                Scene.Add(new RockDebris(Position, sprite.Rotation, debrisSpriteDirectory, "08", new Vector2(19, 50)));
                Scene.Add(new RockDebris(Position, sprite.Rotation, debrisSpriteDirectory, "09", new Vector2(29, 38)));
                Scene.Add(new RockDebris(Position, sprite.Rotation, debrisSpriteDirectory, "10", new Vector2(10, 39)));
                Scene.Add(new RockDebris(Position, sprite.Rotation, debrisSpriteDirectory, "11", new Vector2(19, 27)));
                Scene.Add(new RockDebris(Position, sprite.Rotation, debrisSpriteDirectory, "12", new Vector2(23, 14)));
                Scene.Add(new RockDebris(Position, sprite.Rotation, debrisSpriteDirectory, "13", new Vector2(25, 10)));
                Scene.Add(new RockDebris(Position, sprite.Rotation, debrisSpriteDirectory, "14", new Vector2(40, 34)));
                Scene.Add(new RockDebris(Position, sprite.Rotation, debrisSpriteDirectory, "15", new Vector2(45, 17)));
                Scene.Add(new RockDebris(Position, sprite.Rotation, debrisSpriteDirectory, "16", new Vector2(63, 16)));

                // And remove the rock. The debris will deal with the rest.
                RemoveSelf();
            };
        }

        private class RockDebris : Entity {
            private Vector2 speed;
            private Image image;
            private float fade = 1f;

            public RockDebris(Vector2 position, float rockOrientation, string spriteDirectory, string index, Vector2 debrisCenter) : base(position) {
                // the debris will fly in the direction of the debris compared to the center.
                speed = (debrisCenter - new Vector2(41, 41)).Rotate(rockOrientation) * 2 + SPEED * Vector2.UnitX + 20 * Vector2.UnitY;

                // spawn a static image oriented like the rock.
                image = new Image(GFX.Game[$"{spriteDirectory}/debris_{index}"]);
                image.Rotation = rockOrientation;
                image.CenterOrigin();
                Add(image);
            }

            public override void Update() {
                base.Update();

                // apply speed.
                Position += speed * Engine.DeltaTime;

                // apply gravity.
                speed.Y = Calc.Approach(speed.Y, 160, 400 * Engine.DeltaTime);

                // fade out.
                fade = Calc.Approach(fade, 0f, Engine.DeltaTime * 2.5f);
                image.Color = Color.White * fade;

                if (fade == 0f) {
                    // finished fading out!
                    RemoveSelf();
                }
            }
        }
    }
}
