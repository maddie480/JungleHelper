using Microsoft.Xna.Framework;
using Monocle;
using System;
using Celeste.Mod.Entities;


namespace Celeste.Mod.JungleHelper {
    [Tracked(false)]
    [CustomEntity(
        "JungleHelper/BouncyShroomUp = BounceUp",
        "JungleHelper/BouncyShroomLeft = BounceLeft",
        "JungleHelper/BouncyShroomRight = BounceRight")]
    public class BouncyShroom : Entity {

        public static Entity BounceUp(Level level, LevelData levelData, Vector2 position, EntityData entityData)
            => new BouncyShroom(entityData, position, Directions.Up);
        public static Entity BounceLeft(Level level, LevelData levelData, Vector2 position, EntityData entityData)
            => new BouncyShroom(entityData, position, Directions.Left);
        public static Entity BounceRight(Level level, LevelData levelData, Vector2 position, EntityData entityData)
            => new BouncyShroom(entityData, position, Directions.Right);

        public enum Directions {
            Up,
            Left,
            Right
        }

        public Directions Direction;

        private Vector2 imageOffset;

        private bool dashedIntoIt;
        private bool collidedWithIt;

        private Vector2 particlePosAdjust;

        private float yeetSpeedCalcX;
        private float yeetSpeedCalcY;
        private Sprite bouncyShroomSprite;

        public BouncyShroom(Vector2 position, Directions direction, int yeetx, int yeety)
            : base(position) {

            Depth = -1;
            Direction = direction;

            // making the bounce particles
            particlePosAdjust = new Vector2(0, 1);

            bouncyShroomSprite = new Sprite(GFX.Game, "objects/BouncyShroom/");

            int[] frames = { 2, 3, 4, 5, 6, 7, 8 };

            switch (Direction) {
                case Directions.Up:
                    bouncyShroomSprite.AddLoop("Idle", "mushroom", 0.15f, 0);
                    bouncyShroomSprite.Add("Bounce", "mushroom", 0.05f, frames);

                    Collider = new ColliderList(
                        new Hitbox(8f, 8f, -16f, -15f),
                        new Hitbox(8f, 8f, -8f, -16f),
                        new Hitbox(8f, 8f, 0f, -15f));

                    yeetSpeedCalcY = yeety;

                    Add(new StaticMover() {
                        OnShake = OnShake,
                        SolidChecker = s => s.CollideRect(new Rectangle((int) Position.X - 10, (int) Position.Y + 8, 12, 1))
                    });
                    break;
                case Directions.Right:
                    bouncyShroomSprite.AddLoop("Idle", "mushroom_rd_", 0.15f, 0);
                    bouncyShroomSprite.Add("Bounce", "mushroom_rd_", 0.05f, frames);

                    Collider = new ColliderList(
                        new Hitbox(6f, 6f, -14f, -14f),
                        new Hitbox(8f, 8f, -8f, -15f),
                        new Hitbox(4f, 8f, 0f, -13f),
                        new Hitbox(4f, 8f, 4f, -9f));

                    yeetSpeedCalcY = yeety;
                    yeetSpeedCalcX = yeetx;

                    Add(new StaticMover() {
                        OnShake = OnShake,
                        SolidChecker = s => s.CollideRect(new Rectangle((int) Position.X - 16, (int) Position.Y + 8, 16, 1))                        
                    });
                    break;
                case Directions.Left:
                    Collider = new ColliderList(
                        new Hitbox(6f, 6f, 0f, -14f),
                        new Hitbox(8f, 8f, -8f, -15f),
                        new Hitbox(4f, 8f, -12f, -13f),
                        new Hitbox(4f, 8f, -16f, -9f));

                    bouncyShroomSprite.AddLoop("Idle", "mushroom_ld_", 0.15f, 0);
                    bouncyShroomSprite.Add("Bounce", "mushroom_ld_", 0.05f, frames);

                    yeetSpeedCalcY = yeety;
                    yeetSpeedCalcX = -yeetx;
                    Add(new StaticMover() {
                        OnShake = OnShake,
                        SolidChecker = s => s.CollideRect(new Rectangle((int) Position.X - 8, (int) Position.Y + 8, 16, 1))
                    });
                    break;
            }

            Add(new PlayerCollider(OnCollide));
        }

        public BouncyShroom(EntityData data, Vector2 offset, Directions dir)
            : this(data.Position + offset, dir, data.Int("yeetx", 200), data.Int("yeety", -290)) {
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            bouncyShroomSprite.Position = -20 * Vector2.UnitX - 20 * Vector2.UnitY;

            Add(bouncyShroomSprite);
            bouncyShroomSprite.Play("Idle");

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

        public void SetOrigins(Vector2 origin) {
            foreach (Component component in Components) {
                if (component is Image image) {
                    Vector2 vector = origin - Position;
                    image.Origin = image.Origin + vector - image.Position;
                    image.Position = vector;
                }
            }
        }

        private void OnCollide(Player player) {
            if (player.Speed.Y >= 0f && player.Bottom <= Bottom) {

                if (Direction == Directions.Right) {
                    if (player.Speed.X < yeetSpeedCalcX) {
                        player.Speed.X = yeetSpeedCalcX;
                    }
                } else
                if (Direction == Directions.Left) {
                    if (player.Speed.X > yeetSpeedCalcX) {
                        player.Speed.X = yeetSpeedCalcX;
                    }
                }

                player.Speed.Y = yeetSpeedCalcY;

                Audio.Play("event:/junglehelper/sfx/Mushroom_boost");

                if (player.DashAttacking == true) {
                    dashedIntoIt = true;
                    SceneAs<Level>().Displacement.AddBurst(player.Position + particlePosAdjust, 0.4f, 8f, 64f, 0.5f, Ease.QuadOut, Ease.QuadOut);
                }

                bouncyShroomSprite.Play("Bounce");
            }

            collidedWithIt = true;
        }

        public override void Update() {
            base.Update();

            if (collidedWithIt == true) {
                if (!CollideCheck<Player>()) {
                    if (dashedIntoIt == true) {
                        SceneAs<Level>().Tracker.GetEntity<Player>().StateMachine.State = 0;
                        dashedIntoIt = false;
                    }
                    if (!SceneAs<Level>().Tracker.GetEntity<Player>().Inventory.NoRefills) {
                        SceneAs<Level>().Tracker.GetEntity<Player>().RefillDash();
                    }
                    SceneAs<Level>().Tracker.GetEntity<Player>().RefillStamina();
                    collidedWithIt = false;
                }
            }
        }
    }
}

