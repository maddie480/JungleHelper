
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using Celeste.Mod.Entities;
using MonoMod.Utils;
using Celeste.Mod.JungleHelper.Components;
using System;
using System.Linq;

namespace Celeste.Mod.JungleHelper.Entities {
    [CustomEntity("JungleHelper/Hawk")]
    [Tracked]
    public class Hawk : Entity {
        public static void Load() {
            On.Celeste.Player.Update += onPlayerUpdate;
        }

        public static void Unload() {
            On.Celeste.Player.Update -= onPlayerUpdate;
        }

        private static void onPlayerUpdate(On.Celeste.Player.orig_Update orig, Player self) {
            bool isBeingFlinged = self.Scene != null && self.Scene.Tracker.GetEntities<Hawk>().Any(hawk => ((Hawk) hawk).carryingPlayer);
            Vector2 origSpeed = self.Speed;

            // if the player is being flinged, we are managing their speed ourselves: temporarily make the speed 0 to prevent the player from moving because of their speed.
            if (isBeingFlinged) {
                self.Speed = Vector2.Zero;
            }

            orig(self);

            if (isBeingFlinged) {
                self.Speed = origSpeed;
            }
        }

        private enum States {
            Wait,
            Fling,
            Move
        }
        private static readonly Vector2 spriteOffset = new Vector2(0f, 8f);

        private Sprite sprite;

        private States state = States.Wait;
        private Vector2 flingSpeed;

        private float flingAccel;

        private float hawkSpeed;
        private readonly float speedWithPlayer;
        private readonly float speedWithoutPlayer;
        private readonly float initialY;
        private PlayerCollider playerCollider;

        private bool carryingPlayer = false;

        public Hawk(EntityData data, Vector2 levelOffset) : base(data.Position + levelOffset) {
            Tag |= Tags.TransitionUpdate;
            Position = data.Position + levelOffset;
            speedWithPlayer = data.Float("mainSpeed");
            speedWithoutPlayer = data.Float("slowerSpeed");
            initialY = Y;
            Add(sprite = JungleHelperModule.CreateReskinnableSprite(data, "hawk"));
            sprite.Play("hover");
            sprite.Position = spriteOffset;
            sprite.OnFrameChange = delegate {
                BirdNPC.FlapSfxCheck(sprite);
            };
            Collider = new CircleColliderWithRectangles(16);
            Add(playerCollider = new PlayerCollider(OnPlayer));
            Add(new TransitionListener {
                OnOutBegin = delegate {
                    // make hawk invisible during this
                    Visible = false;
                    //if we're transitioning out of a room while still attached to the hawk...
                    if (state == States.Fling) {
                        // do the usual throw!

                        Player player = SceneAs<Level>()?.Tracker.GetEntity<Player>();
                        if (player != null) {
                            player.StateMachine.State = 0;
                            player.DummyGravity = true;
                            player.DummyFriction = true;
                            player.ForceCameraUpdate = false;
                            player.DummyAutoAnimate = true;
                            playerLaunch(player);
                            player.Speed = new Vector2(hawkSpeed * 0.7f, 0);

                        }
                    };
                    RemoveSelf();
                }
            });
        }

        private void OnPlayer(Player player) {
            if ((CollideFirst<Solid>(Position) == null) && (state == States.Wait || state == States.Move)) {
                flingSpeed = player.Speed * 0.4f;
                flingSpeed.Y = 120f;
                flingAccel = 1000f;
                player.Speed = Vector2.Zero;
                state = States.Fling;
                if (hawkSpeed == 0f) {
                    sprite.Play("throw");
                }
                Add(new Coroutine(doFlingRoutine(player)));
            }
        }

        private IEnumerator hitboxDelay() {
            Collidable = false;
            yield return 0.4f;
            Collidable = true;
        }

        public override void Update() {
            base.Update();

            if (state != States.Wait) {
                sprite.Position = Calc.Approach(sprite.Position, spriteOffset, 32f * Engine.DeltaTime);
            }

            switch (state) {
                case States.Move:
                    // move without player
                    X += speedWithoutPlayer * Engine.DeltaTime;

                    // drag hawk towards its initial Y position.
                    Y = Calc.Approach(Y, initialY, 20f * Engine.DeltaTime);
                    break;
                case States.Wait:
                    // wait for the player
                    break;
                case States.Fling:
                    // carry the player: apply the momentum from the player and drag it progressively to 0
                    if (flingAccel > 0f) {
                        flingSpeed = Calc.Approach(flingSpeed, Vector2.Zero, flingAccel * Engine.DeltaTime);
                    }
                    Position += flingSpeed * Engine.DeltaTime;
                    break;
            }

            // don't catch the player if the hawk is inside a solid.
            playerCollider.Active = !CollideCheck<Solid>();

            if (X >= (SceneAs<Level>().Bounds.Right + 5)) {
                // bird is off-screen! drop the player.
                RemoveSelf();

                Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
                if (player != null) {
                    if (state == States.Fling && player.StateMachine.State == 11) {
                        player.StateMachine.State = 0;
                        player.DummyGravity = true;
                        player.DummyFriction = true;
                        player.ForceCameraUpdate = false;
                        player.DummyAutoAnimate = true;
                    }
                }
            }
        }

        private IEnumerator doFlingRoutine(Player player) {
            sprite.Play("fly");

            hawkSpeed = 0;
            sprite.Scale.X = 1f;

            carryingPlayer = true;

            while (state == States.Fling) {
                yield return null;

                // stop the fling when player dies.
                if (player.Dead)
                    break;

                // drag hawk towards its initial Y position.
                Y = Calc.Approach(Y, initialY, 20f * Engine.DeltaTime);

                // make speed approach the target speed.
                if (hawkSpeed != speedWithPlayer) {
                    hawkSpeed = Calc.Approach(hawkSpeed, speedWithPlayer, speedWithPlayer / 10);
                }
                X += hawkSpeed * Engine.DeltaTime;

                // make sure the player is in a dummy state.
                player.StateMachine.State = 11;
                player.DummyMoving = false;
                player.DummyMaxspeed = false;
                player.DummyGravity = false;
                player.DummyFriction = false;
                player.ForceCameraUpdate = true;
                player.DummyAutoAnimate = false;
                player.Facing = Facings.Right;
                player.Sprite.Play("fallSlow_carry");

                // move the player.
                float oldPlayerPositionY = player.ExactPosition.Y;
                bool hitSomething = false;
                player.MoveToX(X, collision => hitSomething = true);
                player.MoveToY(Y + 16, collision => hitSomething = true);
                float playerMoveAmountY = player.ExactPosition.Y - oldPlayerPositionY;

                // update their speed. it's not used for moving the player, but still used, for example by spikes.
                player.Speed = new Vector2(hawkSpeed, playerMoveAmountY / Engine.DeltaTime);

                if (hitSomething) {
                    // player hit something while getting moved! drop them.
                    player.StateMachine.State = 0;
                    player.Speed = Vector2.Zero;
                    break;
                }

                if (Input.Jump.Pressed) {
                    // player escapes!
                    player.StateMachine.State = 0;
                    playerLaunch(player);
                    player.Speed = new Vector2(hawkSpeed * 0.7f, 0);
                    break;
                }

                if ((Input.DashPressed || Input.CrouchDashPressed) && player.CanDash) {
                    // player dashes out of hawk, let them do that.
                    player.StateMachine.State = player.StartDash();
                    break;
                }
            }

            carryingPlayer = false;

            if (!player.Dead) {
                // reset dummy settings to default.
                player.DummyMoving = false;
                player.DummyMaxspeed = true;
                player.DummyGravity = true;
                player.DummyFriction = true;
                player.ForceCameraUpdate = false;
                player.DummyAutoAnimate = true;
            }

            // get back to the "moving" state.
            Add(new Coroutine(hitboxDelay()));
            Add(new Coroutine(moveRoutine()));
        }

        private void playerLaunch(Player player) {
            DynData<Player> playerData = new DynData<Player>(player);
            player.StateMachine.State = 0;
            playerData.Set("forceMoveX", 1);
            playerData.Set("forceMoveXTimer", 0.2f);
            playerData.Set("launched", true);
        }

        private IEnumerator moveRoutine() {
            sprite.Play("fly");
            sprite.Rotation = 0f;
            sprite.Scale = Vector2.One;
            X += 80f * Engine.DeltaTime;
            yield return 0.1f;
            state = States.Move;
        }
    }
}