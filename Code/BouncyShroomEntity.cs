using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
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

        private PlayerCollider pc;

        private Vector2 imageOffset;

        private bool dashedintoit;
        private bool intoit;

        private ParticleType particleType = new ParticleType(Player.P_DashA);
        private float particleAngle;
        private Vector2 particlePosAdjust;
        private Vector2 particlePosAdjustTwo;

        private float yeetSpeedCalcX;
        private float yeetSpeedCalcY;
        private float yeetSpeedX;
        private float yeetSpeedYver;
        private float yeetSpeedYhor;
        private Sprite bouncyShroomSprite;

        public BouncyShroom(Vector2 position, Directions direction, int yeetx, int yeety)
            : base(position) {
            base.Depth = -1;
            this.Direction = direction;

            // making the bounce particles
            particleType.Color = Color.LightBlue;
            particleType.Color2 = Color.LightBlue;
            particleType.FadeMode = ParticleType.FadeModes.Late;
            particleType.SpeedMin = 20f;
            particleType.SpeedMax = 25f;
            particleType.LifeMin = 0.5f;
            particleType.LifeMax = 0.7f;
            particleAngle = (float) (11 * Math.PI / 8);
            particlePosAdjust = new Vector2(0, 1);
            particlePosAdjustTwo = -Vector2.UnitX;

            // yeet parameters
            yeetSpeedX = 200f;
            yeetSpeedYver = -325f;
            yeetSpeedYhor = -290f;

            bouncyShroomSprite = new Sprite(GFX.Game, "JungleHelper/BouncyShroom/");
            bouncyShroomSprite.AddLoop("Idle", "mushroom", 0.15f, 0);
            int[] frames = { 2, 3, 4, 5, 6, 7, 8 };
            bouncyShroomSprite.Add("Bounce", "mushroom", 0.05f, frames);

            switch (Direction) {
                case Directions.Up:
                    yeetSpeedCalcY = yeety;
                    //yeetSpeedCalcY = yeetSpeedYver;
                    break;
                case Directions.Right:
                    //yeetSpeedCalcX = yeetSpeedX;
                    //yeetSpeedCalcY = yeetSpeedYhor;
                    yeetSpeedCalcY = yeety;
                    yeetSpeedCalcX = yeetx;
                    break;
                case Directions.Left:
                    //yeetSpeedCalcX = -yeetSpeedX;
                    //yeetSpeedCalcY = yeetSpeedYhor;
                    yeetSpeedCalcY = yeety;
                    yeetSpeedCalcX = -yeetx;
                    break;
            }

            Hitbox hitbox1 = new Hitbox(8f, 8f, -16f, -15f);
            Hitbox hitbox2 = new Hitbox(8f, 8f, -8f, -16f);
            Hitbox hitbox3 = new Hitbox(8f, 8f, 0f, -15f);
            base.Collider = new ColliderList(hitbox1, hitbox2, hitbox3);

            Add(pc = new PlayerCollider(OnCollide));
            Add(new StaticMover {
                OnShake = OnShake,
                SolidChecker = IsRiding,
                JumpThruChecker = IsRiding,

            });
        }

        public BouncyShroom(EntityData data, Vector2 offset, Directions dir)
            : this(data.Position + offset, dir, data.Int("yeetx", 200), data.Int("yeety", -290)) {
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            AreaData areaData = AreaData.Get(scene);

            /*
            string str = Direction.ToString().ToLower();

            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("JungleHelper/mushroomTemplate_" + str);

            Image image = new Image(Calc.Random.Choose(atlasSubtextures));

            image.JustifyOrigin(0.5f, 1f);
            image.Position = -4 * Vector2.UnitX + 8 * Vector2.UnitY;
            */
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
            foreach (Component component in base.Components) {
                Image image = component as Image;
                if (image != null) {
                    Vector2 vector = origin - Position;
                    image.Origin = image.Origin + vector - image.Position;
                    image.Position = vector;
                }
            }
        }

        private void OnCollide(Player player) {
            if (player.Speed.Y >= 0f && player.Bottom <= base.Bottom) {

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
                    dashedintoit = true;
                    SceneAs<Level>().Displacement.AddBurst(player.Position + particlePosAdjust, 0.4f, 8f, 64f, 0.5f, Ease.QuadOut, Ease.QuadOut);
                }

                //Remove(image);
                bouncyShroomSprite.Play("Bounce");
            }

            intoit = true;

        }

        public override void Update() {
            base.Update();

            if (intoit == true) {
                if (!CollideCheck<Player>()) {

                    if (dashedintoit == true) {
                        SceneAs<Level>().Tracker.GetEntity<Player>().StateMachine.State = 0;
                        dashedintoit = false;
                    }
                    if (!SceneAs<Level>().Tracker.GetEntity<Player>().Inventory.NoRefills) {
                        SceneAs<Level>().Tracker.GetEntity<Player>().RefillDash();
                    }
                    SceneAs<Level>().Tracker.GetEntity<Player>().RefillStamina();
                    intoit = false;
                }

            }

        }

        private void OnCertifiedHit(Vector2 hitposition) {
            if (SceneAs<Level>().Tracker.GetEntity<Player>().DashAttacking == true) {
                OnCertifiedDash(hitposition);
            }
            Audio.Play("event:/char/badeline/jump_assisted");
            Emit4Particles(hitposition);
        }

        private void OnCertifiedDash(Vector2 hitposition) {
            dashedintoit = true;
            SceneAs<Level>().Displacement.AddBurst(hitposition + particlePosAdjust, 0.4f, 8f, 64f, 0.5f, Ease.QuadOut, Ease.QuadOut);
        }

        private void Emit4Particles(Vector2 hitposition) {
            SceneAs<Level>().ParticlesFG.Emit(particleType, hitposition + particlePosAdjust + particlePosAdjustTwo, particleAngle); //angle in radians
            SceneAs<Level>().ParticlesFG.Emit(particleType, hitposition + particlePosAdjust - particlePosAdjustTwo, particleAngle + (float) (2 * Math.PI / 8));
            SceneAs<Level>().ParticlesFG.Emit(particleType, hitposition + particlePosAdjust + particlePosAdjustTwo, particleAngle);
            SceneAs<Level>().ParticlesFG.Emit(particleType, hitposition + particlePosAdjust - particlePosAdjustTwo, particleAngle + (float) (2 * Math.PI / 8));
        }

        private void yeet(Player player) {

        }

        private bool IsRiding(Solid solid) {
            return CollideCheckOutside(solid, Position + Vector2.UnitY);
            /*
            switch (Direction)
            {
                default:
                    return false;
                case Directions.Up:
                    return CollideCheckOutside(solid, Position + Vector2.UnitY);
                case Directions.Left:
                    return CollideCheckOutside(solid, Position + Vector2.UnitX);
                case Directions.Right:
                    return CollideCheckOutside(solid, Position - Vector2.UnitX);
            }
            */
        }

        private bool IsRiding(JumpThru jumpThru) {
            if (Direction != 0) {
                return false;
            }
            return CollideCheck(jumpThru, Position + Vector2.UnitY);
        }
    }
}

