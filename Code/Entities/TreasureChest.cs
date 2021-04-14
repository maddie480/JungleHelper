using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.JungleHelper.Entities {
    [CustomEntity("JungleHelper/TreasureChest")]
    class TreasureChest : Entity {
        private readonly EntityID id;
        private readonly string spriteName;

        private Sprite sprite;

        public TreasureChest(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset) {
            this.id = id;
            spriteName = data.Attr("sprite");

            Add(sprite = JungleHelperModule.CreateReskinnableSprite(data, "treasure_chest"));
            Add(new PlayerCollider(onPlayer, new Circle(60f)));
            Depth = 100;
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            if (SceneAs<Level>().Session.GetFlag("chest_open_" + id.Key)) {
                Collidable = false;
                if (SceneAs<Level>().Session.HeartGem) {
                    // heart already collected
                    sprite.Play("collected");
                } else {
                    // chest open, but heart not collected yet
                    sprite.Play("open");
                    Scene.Add(new HeartGem(Center - Vector2.UnitY * 15f));
                }
            }
        }

        public override void Update() {
            base.Update();

            if (sprite.CurrentAnimationID == "open" && SceneAs<Level>().Session.HeartGem) {
                sprite.Play("collected");
            }
        }

        private void onPlayer(Player player) {
            // does the player carry a key that is not currently in use?
            foreach (Follower follower in player.Leader.Followers) {
                if (follower.Entity is Key && !(follower.Entity as Key).StartedUsing) {
                    // try opening the chest with it.
                    tryOpen(player, follower);
                    break;
                }
            }
        }

        private void tryOpen(Player player, Follower fol) {
            if (!Scene.CollideCheck<Solid>(player.Center, Center)) {
                // nothing is in the way: use the key!
                (fol.Entity as Key).StartedUsing = true;
                Add(new Coroutine(unlockRoutine(fol)));
                Collidable = false;
            }
        }

        private IEnumerator unlockRoutine(Follower fol) {
            // sound starts playing
            SoundEmitter emitter = SoundEmitter.Play("event:/game/03_resort/key_unlock", this);
            emitter.Source.DisposeOnTransition = true;

            // use key
            Level level = SceneAs<Level>();
            Key key = fol.Entity as Key;
            yield return key.UseRoutine(Center + new Vector2(0f, 14f));

            // open the chest and consider the key as used.
            level.Session.SetFlag("chest_open_" + id.Key, true);
            key.RegisterUsed();
            sprite.Play("openChest");

            // spawn the treasure chest face sprite.
            Sprite face = JungleHelperModule.CreateReskinnableSprite(spriteName, "treasure_chest");
            face.Play("face");
            Entity faceHolder = new Entity(Position);
            faceHolder.Depth = -50;
            faceHolder.Add(face);
            Scene.Add(faceHolder);

            // spawn the crystal heart, between the chest and the face.
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            HeartGem heart = new HeartGem(Center + Vector2.UnitY * 5f);
            heart.Depth = 75;
            Scene.Add(heart);

            // move it up (if it doesn't get collected in the meantime).
            float p = 0f;
            while (p < 1f && heart.Get<Coroutine>() == null) {
                heart.Y = Center.Y + 5f - Ease.CubeOut(p) * 20f;
                yield return null;
                p += Engine.DeltaTime;
            }

            // make crystal heart depth normal and get rid of the face.
            Scene.Remove(faceHolder);
            if (heart.Get<Coroutine>() == null) {
                heart.Y = Center.Y - 15f;
                heart.Depth = 0;
            }
        }
    }
}
