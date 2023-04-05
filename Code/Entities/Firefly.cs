using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.JungleHelper.Entities {
    // reskinned moon creature without a tail
    [CustomEntity("JungleHelper/Firefly")]
    public class Firefly : Entity {
        private Vector2 start;
        private Vector2 target;

        private float targetTimer;

        private Vector2 speed;
        private Vector2 bump;

        private Player following;
        private Vector2 followingOffset;
        private float followingTime;

        private Sprite sprite;
        private VertexLight light;

        private readonly int spawn;
        private readonly string reskinName;

        private Rectangle originLevelBounds;

        public Firefly(Vector2 position, string reskinName) {
            Tag = Tags.TransitionUpdate;
            Depth = -13010;

            Collider = new Hitbox(20f, 20f, -10f, -10f);

            start = position;
            targetTimer = 0f;
            getRandomTarget();
            Position = target;

            Add(new PlayerCollider(onPlayer));
            Add(sprite = JungleHelperModule.CreateReskinnableSprite(reskinName, "firefly"));
            Add(light = new VertexLight(Color.White, 1f, 4, 8));

            this.reskinName = reskinName;
        }

        public Firefly(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Attr("sprite")) {

            spawn = data.Int("number", 1) - 1;
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            for (int i = 0; i < spawn; i++) {
                // spawn more fireflies according to the "number" property.
                scene.Add(new Firefly(Position + new Vector2(Calc.Random.Range(-4, 4), Calc.Random.Range(-4, 4)), reskinName));
            }

            originLevelBounds = (scene as Level).Bounds;
        }

        private void onPlayer(Player player) {
            Vector2 playerDirection = (Position - player.Center).SafeNormalize(player.Speed.Length() * 0.3f);
            if (playerDirection.LengthSquared() > bump.LengthSquared()) {
                bump = playerDirection;
                if ((player.Center - start).Length() < 200f) {
                    following = player;
                    followingTime = Calc.Random.Range(6f, 12f);
                    getFollowOffset();
                }
            }
        }

        private void getFollowOffset() {
            followingOffset = new Vector2(Calc.Random.Choose(-1, 1) * Calc.Random.Range(8, 16), Calc.Random.Range(-20f, 0f));
        }

        private void getRandomTarget() {
            Vector2 originalTarget = target;
            do {
                float length = Calc.Random.NextFloat(32f);
                float angleRadians = Calc.Random.NextFloat((float) Math.PI * 2f);
                target = start + Calc.AngleToVector(angleRadians, length);
            }
            while ((originalTarget - target).Length() < 8f);
        }

        public override void Update() {
            base.Update();
            if (following == null) {
                targetTimer -= Engine.DeltaTime;
                if (targetTimer <= 0f) {
                    targetTimer = Calc.Random.Range(0.8f, 4f);
                    getRandomTarget();
                }
            } else {
                followingTime -= Engine.DeltaTime;
                targetTimer -= Engine.DeltaTime;
                if (targetTimer <= 0f) {
                    targetTimer = Calc.Random.Range(0.8f, 2f);
                    getFollowOffset();
                }
                target = following.Center + followingOffset;
                if ((Position - start).Length() > 200f || followingTime <= 0f) {
                    following = null;
                    targetTimer = 0f;
                }
            }
            Vector2 distanceToTarget = (target - Position).SafeNormalize();
            speed += distanceToTarget * ((following == null) ? 90f : 120f) * Engine.DeltaTime;
            speed = speed.SafeNormalize() * Math.Min(speed.Length(), (following == null) ? 40f : 70f);
            bump = bump.SafeNormalize() * Calc.Approach(bump.Length(), 0f, Engine.DeltaTime * 80f);
            Position += (speed + bump) * Engine.DeltaTime;
            X = Calc.Clamp(X, originLevelBounds.Left + 4, originLevelBounds.Right - 4);
            Y = Calc.Clamp(Y, originLevelBounds.Top + 4, originLevelBounds.Bottom - 4);

            // update light point depending on current animation frame.
            switch (sprite.Texture.AtlasPath) {
                case "JungleHelper/Firefly/firefly00":
                    light.Visible = true;
                    light.StartRadius = 4;
                    light.EndRadius = 8;
                    break;
                case "JungleHelper/Firefly/firefly01":
                case "JungleHelper/Firefly/firefly04":
                    light.Visible = true;
                    light.StartRadius = 2;
                    light.EndRadius = 4;
                    break;
                case "JungleHelper/Firefly/firefly02":
                case "JungleHelper/Firefly/firefly05":
                    light.Visible = true;
                    light.StartRadius = 1;
                    light.EndRadius = 2;
                    break;
                case "JungleHelper/Firefly/firefly03":
                    light.Visible = false;
                    break;
            }
        }

        public override void Render() {
            Vector2 origPosition = Position;
            Position = Position.Floor();
            base.Render();
            Position = origPosition;
        }
    }
}
