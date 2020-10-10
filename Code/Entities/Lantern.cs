using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Reflection;

namespace Celeste.Mod.JungleHelper.Entities {
    [CustomEntity("JungleHelper/Lantern")]
    [Tracked]
    class Lantern : Actor {
        private const PlayerSpriteMode SpriteModeMadelineLantern = (PlayerSpriteMode) 444480;

        private static Hook hookCanDash;

        public static void Load() {
            On.Celeste.LevelLoader.ctor += onLevelLoaderConstructor;
            On.Celeste.PlayerSprite.ctor += onPlayerSpriteConstructor;
            On.Celeste.Player.NormalUpdate += onPlayerNormalUpdate;
            On.Celeste.Player.OnTransition += onPlayerTransition;

            hookCanDash = new Hook(typeof(Player).GetMethod("get_CanDash"), typeof(Lantern).GetMethod("playerCanDash", BindingFlags.NonPublic | BindingFlags.Static));
        }

        public static void Unload() {
            On.Celeste.LevelLoader.ctor -= onLevelLoaderConstructor;
            On.Celeste.PlayerSprite.ctor -= onPlayerSpriteConstructor;
            On.Celeste.Player.NormalUpdate -= onPlayerNormalUpdate;
            On.Celeste.Player.OnTransition -= onPlayerTransition;

            hookCanDash?.Dispose();
            hookCanDash = null;
        }


        private static void onLevelLoaderConstructor(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startPosition) {
            orig(self, session, startPosition);

            // we want Madeline's sprite to load its metadata (that is, her hair positions on her frames of animation).
            PlayerSprite.CreateFramesMetadata("junglehelper_madeline_lantern");
        }

        private static void onPlayerSpriteConstructor(On.Celeste.PlayerSprite.orig_ctor orig, PlayerSprite self, PlayerSpriteMode mode) {
            bool lantern = mode == SpriteModeMadelineLantern;
            if (lantern) {
                // build regular Madeline with backpack as a reference.
                mode = PlayerSpriteMode.Madeline;
            }

            orig(self, mode);

            if (lantern) {
                // throw lantern Madeline sprites in the mix.
                GFX.SpriteBank.CreateOn(self, "junglehelper_madeline_lantern");
                new DynData<PlayerSprite>(self)["Mode"] = SpriteModeMadelineLantern;

                // replay the "idle" sprite to make it apply immediately.
                self.Play("idle", restart: true);
            }
        }

        private delegate bool orig_CanDash(Player self);
        private static bool playerCanDash(orig_CanDash orig, Player self) {
            return orig(self) && self.Sprite.Mode != SpriteModeMadelineLantern;
        }

        private static ParticleType P_Regen;

        private float regrabDelay = 0f;
        private float speedY = 0f;
        private float respawnDelay = 0f;

        private Vector2 startingPosition;
        private bool doRespawn = true;

        public Lantern(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Sprite sprite = JungleHelperModule.SpriteBank.Create("lantern");
            sprite.Y = 5;
            Add(sprite);
            Collider = new Hitbox(8, 8, -4, 0);
            startingPosition = Position;

            Add(new PlayerCollider(onPlayer));

            if (P_Regen == null) {
                P_Regen = new ParticleType(Refill.P_Regen) {
                    Color = Calc.HexToColor("FFB564"),
                    Color2 = Calc.HexToColor("FCFFC3")
                };
            }
        }

        private void onPlayer(Player player) {
            if (Input.Grab.Check) {
                // the player grabs the lantern.
                // on a technical level, Maddy's sprite changes to have her holding the lantern, and the lantern disappears.
                changePlayerSpriteMode(player, SpriteModeMadelineLantern);
                RemoveSelf();
                new DynData<Player>(player)["JungleHelper_LanternStartingPosition"] = startingPosition;
                new DynData<Player>(player)["JungleHelper_LanternDoRespawn"] = doRespawn;
            }
        }

        public override void Update() {
            base.Update();

            // enforce the respawn delay.
            if (respawnDelay > 0f) {
                respawnDelay -= Engine.DeltaTime;
                if (respawnDelay <= 0f) {
                    // move back the lantern to its starting point.
                    Position = startingPosition;
                    regrabDelay = 0f;
                    speedY = 0f;
                    Visible = Collidable = true;

                    Audio.Play("event:/game/04_cliffside/arrowblock_reappear", Position);
                    (Scene as Level).ParticlesFG.Emit(P_Regen, 16, Center, Vector2.One * 2f);
                } else {
                    // don't update the lantern (no need to apply gravity and all that, it doesn't exist :a:)
                    return;
                }
            }

            // enforce the regrab delay.
            if (regrabDelay > 0f) {
                if (regrabDelay == 0.25f) {
                    // it was just dropped. let's ensure it is not in the ground.
                    Collidable = true;
                    while (CollideCheck<Solid>()) {
                        Position.Y--;
                    }
                    Collidable = false;
                }

                regrabDelay -= Engine.DeltaTime;
                if (regrabDelay <= 0f) {
                    Collidable = true;
                }
            }

            // enforce gravity.
            if (!OnGround()) {
                float acceleration = 800f;
                if (Math.Abs(speedY) <= 30f) {
                    acceleration *= 0.5f;
                }
                speedY = Calc.Approach(speedY, 200f, acceleration * Engine.DeltaTime);
            }
            MoveV(speedY * Engine.DeltaTime, onCollideV);

            // handle the lantern getting dropped off the level
            if (Top > (Scene as Level).Bounds.Bottom) {
                lanternIsLost();
            }
        }

        private void lanternIsLost() {
            if (doRespawn) {
                respawnDelay = 2f;
                Visible = Collidable = false;
            } else {
                // this happens when the lantern comes from another screen and thus can't respawn
                RemoveSelf();
            }
        }

        private static void changePlayerSpriteMode(Player player, PlayerSpriteMode spriteMode) {
            if (player.Active) {
                player.ResetSpriteNextFrame(spriteMode);
            } else {
                player.ResetSprite(spriteMode);
            }
        }

        private void onCollideV(CollisionData data) {
            if (speedY > 0f) {
                // there is a 99% chance this sound is going to be changed, but here it is.
                Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", Position, "crystal_velocity", 0f);
            }
            if (speedY > 160f) {
                // emit bounce particles
                (Scene as Level).Particles.Emit(TheoCrystal.P_Impact, 12, BottomCenter, Vector2.UnitY * 6f, -(float) (Math.PI / 2f));
            }
            if (speedY > 140f && !(data.Hit is SwapBlock) && !(data.Hit is DashSwitch)) {
                // bounce
                speedY *= -0.4f;
            } else {
                speedY = 0f;
            }
        }

        private static int onPlayerNormalUpdate(On.Celeste.Player.orig_NormalUpdate orig, Player self) {
            if (self.Sprite.Mode == SpriteModeMadelineLantern && Input.Grab.Check && Input.MoveY > 0f) {
                // drop the lantern.
                // technically, this means "Madeline's sprite returns to normal, and we spawn a Lantern entity".
                changePlayerSpriteMode(self, SaveData.Instance.Assists.PlayAsBadeline ? PlayerSpriteMode.MadelineAsBadeline : self.DefaultSpriteMode);

                Lantern lantern = new Lantern(new EntityData { Position = self.Center }, Vector2.Zero);
                lantern.regrabDelay = 0.25f;
                lantern.Collidable = false;

                DynData<Player> playerData = new DynData<Player>(self);
                if (playerData.Data.ContainsKey("JungleHelper_LanternStartingPosition")) {
                    lantern.startingPosition = playerData.Get<Vector2>("JungleHelper_LanternStartingPosition");
                    lantern.doRespawn = playerData.Get<bool>("JungleHelper_LanternDoRespawn");
                }

                self.Scene.Add(lantern);
            }

            return orig(self);
        }

        private static void onPlayerTransition(On.Celeste.Player.orig_OnTransition orig, Player self) {
            orig(self);

            // if the player is holding a lantern, it shouldn't respawn, since it is from another screen.
            new DynData<Player>(self)["JungleHelper_LanternDoRespawn"] = false;
        }

        public static float GetClosestLanternDistanceTo(Vector2 position, Scene scene, out Vector2 objectPosition) {
            float distance = float.MaxValue;
            objectPosition = Vector2.Zero;

            Player maddy = scene.Tracker.GetEntity<Player>();
            if (maddy != null) {
                objectPosition = maddy.Center;
            }
            if (maddy?.Sprite.Mode != SpriteModeMadelineLantern) {
                maddy = null; // Maddy has no torch = Maddy is not here.
            }

            // if Maddy exists and has a lantern, then take her distance into account.
            if (maddy != null) {
                distance = (position - maddy.Center).Length();
            }

            // then take the distance between all lanterns into account.
            foreach (Lantern lantern in scene.Tracker.GetEntities<Lantern>()) {
                float distanceLantern = (position - lantern.Center).Length();
                if (distance > distanceLantern) {
                    distance = distanceLantern;
                    objectPosition = lantern.Center;
                }
            }

            return distance;
        }
    }
}
