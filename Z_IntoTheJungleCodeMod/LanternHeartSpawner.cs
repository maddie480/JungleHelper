using Celeste.Mod.Entities;
using Celeste.Mod.JungleHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.IntoTheJungleCodeMod {
    [CustomEntity("IntoTheJungleCodeMod/LanternHeartSpawner")]
    class LanternHeartSpawner : Entity {
        private Collider lanternCollider1, lanternCollider2;

        public LanternHeartSpawner(EntityData data, Vector2 offset) : base(data.Position + offset) {
            lanternCollider1 = new Hitbox(16, 16, -60, 52);
            lanternCollider2 = new Hitbox(16, 16, 44, 52);

            ColliderList list = new ColliderList();
            Collider = list;
            list.Add(lanternCollider1);
            list.Add(lanternCollider2);
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            if (SceneAs<Level>().Session.HeartGem) {
                // heart was already collected
                RemoveSelf();
            } else if (SceneAs<Level>().Session.GetFlag("lantern_heart_unlocked")) {
                // heart was already unlocked, but not collected yet, so spawn it instead.
                Scene.Add(new HeartGem(Position));
                RemoveSelf();
            }
        }

        public override void Update() {
            base.Update();

            if (!SceneAs<Level>().Session.GetFlag("lantern_heart_unlocked")) {
                // check if there is a lantern in both hitboxes.
                List<Lantern> lanternsInScene = Scene.Tracker.GetEntities<Lantern>().OfType<Lantern>().ToList();
                if (lanternsInScene.Any(lantern => lanternCollider1.Collide(lantern)) && lanternsInScene.Any(lantern => lanternCollider2.Collide(lantern))) {
                    SceneAs<Level>().Session.SetFlag("lantern_heart_unlocked", true);
                    Add(new Coroutine(activateRoutine()));
                }
            }
        }

        private IEnumerator activateRoutine() {
            yield return 0.533f;

            // sound
            Audio.Play("event:/game/06_reflection/supersecret_heartappear");

            // fake heart entity
            Entity dummy = new Entity(Position) {
                Depth = 1
            };
            Scene.Add(dummy);
            Image white = new Image(GFX.Game["collectables/heartgem/white00"]);
            white.CenterOrigin();
            white.Scale = Vector2.Zero;
            dummy.Add(white);
            BloomPoint glow = new BloomPoint(0f, 16f);
            dummy.Add(glow);

            // absorb orbs (from lantern position to heart position)
            List<Entity> absorbs = new List<Entity>();
            for (int i = 0; i < 20; i++) {
                AbsorbOrb absorbOrb = new AbsorbOrb(i < 10 ? Position + lanternCollider1.Center : Position + lanternCollider2.Center, dummy);
                Scene.Add(absorbOrb);
                absorbs.Add(absorbOrb);
                yield return null;
            }

            // pause
            yield return 0.8f;

            // make the fake heart appear
            float duration = 0.6f;
            for (float p = 0f; p < 1f; p += Engine.DeltaTime / duration) {
                white.Scale = Vector2.One * p;
                glow.Alpha = p;
                (Scene as Level).Shake();
                yield return null;
            }

            // make the actual heart appear in a flash!
            foreach (Entity item in absorbs) {
                item.RemoveSelf();
            }
            (Scene as Level).Flash(Color.White);
            Scene.Remove(dummy);
            Scene.Add(new HeartGem(Position));
        }

    }
}
