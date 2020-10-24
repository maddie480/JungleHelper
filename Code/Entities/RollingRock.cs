using Celeste.Mod.Entities;
using Celeste.Mod.JungleHelper.Components;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
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
        private Image image;
        private MTexture debrisTexture = null;
        private bool rolling = false;
        private bool falling = false;
        private bool shattered = false;
        private float fallingSpeed = 0f;

        public RollingRock(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Add(image = new Image(GFX.Game[$"JungleHelper/RollingRock/{data.Attr("sprite")}"]));
            if (GFX.Game.Has($"JungleHelper/RollingRock/debris_{data.Attr("sprite")}")) {
                debrisTexture = GFX.Game[$"JungleHelper/RollingRock/debris_{data.Attr("sprite")}"];
            }
            image.CenterOrigin();
            Collider = new CircleColliderWithRectangles(32);
            Add(new PlayerCollider(onPlayer));

            IgnoreJumpThrus = true;
            AllowPushing = false;

            // make the boulder pop out of existence on the beginning of transitions because it looks better than it freezing into place.
            Add(new TransitionListener {
                OnOutBegin = () => Visible = false
            });
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
                // shattered rocks do nothing.
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
                image.Rotation += ROTATION_SPEED * Engine.DeltaTime;
            }

            if (!falling && !rolling) {
                // we are waiting. fall down when player moved and is on the right.
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player != null && !player.JustRespawned && player.X > Right) {
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

            for (int i = -3; i < 4; i++) {
                int chunkWidth = (int) Math.Abs(Math.Cos(Math.Asin((double) i / 4)) * 4);
                for (int j = -chunkWidth; j < chunkWidth; j++) {
                    Debris debris = Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + i * 8, 4 + j * 8), '6' /* stone */, true);
                    if (debrisTexture != null) {
                        new DynData<Debris>(debris).Get<Image>("image").Texture = debrisTexture;
                    }
                    Scene.Add(debris.BlastFrom(CenterRight));
                }
            }

            RemoveSelf();
        }
    }
}
