using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.JungleHelper.Entities {
    [CustomEntity("JungleHelper/Snake")]
    class Snake : Actor {
        private const float ACTIVATION_RADIUS = 100f;
        private const float LOST_RADIUS = 200f;

        private const float SPEED = 100f;

        private readonly Vector2 startingPosition;
        private readonly Vector2 scaredPosition;

        private StateMachine stateMachine;
        private Sprite sprite;

        public Snake(EntityData data, Vector2 offset) : base(data.Position + offset) {
            startingPosition = Position;
            scaredPosition = data.NodesOffset(offset)[0];

            stateMachine = new StateMachine();
            stateMachine.SetCallbacks(0, waitingForPlayerUpdate, null, waitingForPlayerBegin);
            stateMachine.SetCallbacks(1, attackingPlayerUpdate);
            stateMachine.SetCallbacks(2, returningToStartPointUpdate, null, returningToStartPointBegin);
            stateMachine.SetCallbacks(3, runningAwayUpdate, null, runningAwayBegin);
            stateMachine.SetCallbacks(4, hiddenUpdate, null, hiddenBegin);
            Add(stateMachine);

            Random snakeRandom = new Random();

            // this is an overlay that can randomly be slapped over the "idle_aggro" animation, and disappears as soon as the animation is done.
            Sprite spriteHissOverlay = JungleHelperModule.SpriteBank.Create("snek");
            spriteHissOverlay.Visible = false;
            spriteHissOverlay.OnFinish = _ => spriteHissOverlay.Visible = false;

            // this is the main sprite.
            sprite = JungleHelperModule.SpriteBank.Create("snek");
            sprite.FlipX = data.Bool("left", false);
            sprite.OnLoop = animation => {
                if (animation == "idle_aggro" && snakeRandom.Next(10) == 1) {
                    // play the "hiss" overlay animation.
                    spriteHissOverlay.Play("idle_hiss");
                    spriteHissOverlay.Visible = true;
                    spriteHissOverlay.FlipX = sprite.FlipX;
                }
            };
            sprite.OnChange = (from, to) => {
                if (from == "idle_aggro" && to != "idle_aggro") {
                    // exiting "idle_aggro", hide the "hiss" overlay animation if it is visible.
                    spriteHissOverlay.Visible = false;
                }
            };

            Add(sprite);
            Add(spriteHissOverlay);

            Collider = new Hitbox(58, 6, 3, 10);
            Add(new PlayerCollider(onPlayer));
        }

        private void onPlayer(Player player) {
            // kill the player.
            if (player.Center != Center) {
                player.Die(Vector2.Normalize(player.Center - Center));
            } else {
                player.Die(Vector2.Zero);
            }
        }

        private void waitingForPlayerBegin() {
            sprite.Play("idle_aggro");
        }

        private int waitingForPlayerUpdate() {
            // is player here yet?
            Player player = Scene.Tracker.GetEntity<Player>();

            if (player != null && (Center - player.Center).LengthSquared() < ACTIVATION_RADIUS * ACTIVATION_RADIUS && Math.Abs(player.Bottom - Bottom) < 5f) {
                // it is! attack >:(
                return 1;
            }

            // it isn't, continue with waiting
            return 0;
        }

        private int attackingPlayerUpdate() {
            Player player = Scene.Tracker.GetEntity<Player>();

            if (player == null || (Center - player.Center).LengthSquared() > LOST_RADIUS * LOST_RADIUS) {
                // player either disappeared or outran the snake... make it return to its starting point.
                return 2;
            }

            // move towards the player.
            float startingX = ExactPosition.X;
            float startingXApprox = Position.X;

            // compute the movement we want the snake to follow.
            float toX = Calc.Approach(ExactPosition.X, player.Position.X - 32f, SPEED * Engine.DeltaTime);

            // make the hitbox 1px wide, on the front of the snake.
            Collider.Width = 1f;
            if (toX > ExactPosition.X) {
                Collider.Position.X = 60;
            }

            // check if the front of the snake will be on ground, and actually move if it is the case.
            if (OnGround(new Vector2(toX, ExactPosition.Y))) {
                MoveToX(toX);
            }

            // restore the original hitbox.
            Collider.Width = 58f;
            Collider.Position.X = 3f;

            if (startingX != ExactPosition.X) {
                // we moved. play the moving animation
                if (!sprite.CurrentAnimationID.StartsWith("moving_")) {
                    sprite.Play("moving_aggro");
                }

                if (Position.X != startingXApprox) {
                    // and flip the sprite accordingly if it moved by at least 1 pixel.
                    sprite.FlipX = (startingX > ExactPosition.X);
                }
            } else {
                // we didn't move. play the idle animation
                if (!sprite.CurrentAnimationID.StartsWith("idle_")) {
                    sprite.Play("idle_aggro");
                }
            }

            // and continue attacking.
            return 1;
        }

        private void returningToStartPointBegin() {
            sprite.Play("moving_aggro");

            // flip the sprite accordingly
            sprite.FlipX = (startingPosition.X < Position.X);
        }

        private int returningToStartPointUpdate() {
            // move towards the starting point.
            float targetForThisFrame = Calc.Approach(Position.X, startingPosition.X, SPEED * Engine.DeltaTime);
            NaiveMove(new Vector2(targetForThisFrame - Position.X, 0f));

            if (Position.X == startingPosition.X) {
                // we're done! we reached the starting point.
                return 0;
            }

            // continue moving.
            return 2;
        }

        private void runningAwayBegin() {
            sprite.Play("shock"); // omg
        }

        private int runningAwayUpdate() {
            if (sprite.CurrentAnimationID == "shock") {
                // let's wait until the shocked animation is over.
                return 3;
            }

            // flip the sprite according to moving direction.
            sprite.FlipX = (scaredPosition.X < Position.X);

            // move towards the hiding point.
            float targetForThisFrame = Calc.Approach(Position.X, scaredPosition.X, SPEED * Engine.DeltaTime);
            NaiveMove(new Vector2(targetForThisFrame - Position.X, 0f));

            if (Position.X == scaredPosition.X) {
                // we're done! we reached our hideout.
                return 4;
            }

            // continue moving.
            return 3;
        }

        private void hiddenBegin() {
            sprite.Play("idle_scared");
        }

        private int hiddenUpdate() {
            Player player = Scene.Tracker.GetEntity<Player>();

            if (player != null && player.Sprite.Mode == Lantern.SpriteModeMadelineLantern
                && (startingPosition - player.Center).LengthSquared() < ACTIVATION_RADIUS * ACTIVATION_RADIUS) {

                // player is still here! stay hidden.
                return 4;
            }

            // player went away. return to starting point.
            return 2;
        }

        public override void Update() {
            base.Update();

            // this check applies to all states where the snake is aggressive (waiting for player, attacking, returning to starting point).
            if (sprite.CurrentAnimationID.Contains("_aggro")) {
                Player player = Scene.Tracker.GetEntity<Player>();

                if (player != null && player.Sprite.Mode == Lantern.SpriteModeMadelineLantern
                    && (startingPosition - player.Center).LengthSquared() < ACTIVATION_RADIUS * ACTIVATION_RADIUS) {

                    // oh h the player is coming with the lantern! snake is now shocked.
                    stateMachine.State = 3;
                }
            }
        }
    }
}
