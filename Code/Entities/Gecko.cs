using Celeste.Mod.Entities;
using Celeste.Mod.JungleHelper.Triggers;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.JungleHelper.Entities {
    [CustomEntity("JungleHelper/Gecko")]
    [Tracked]
    public class Gecko : Entity {
        // doing birdTutorial.gui works, but it actually doesn't, because it retrieves it from BirdNPC, not from CustomBirdTutorial!
        // as usual, stuff from Everest entities is NOT publicized.
        private static readonly FieldInfo birdTutorialGuiField = typeof(CustomBirdTutorial).GetField("gui", BindingFlags.NonPublic | BindingFlags.Instance);

        private Sprite sprite;
        private Vector2 startPosition;

        private Vector2 node;

        private bool moving = true;
        private bool showedTutorial = false;
        private bool triggered = false;

        private readonly float delay;
        private readonly bool left = false;
        private readonly BirdTutorialGui[] guis;
        public readonly string GeckoId;
        private readonly bool hostile;

        private Coroutine birdTutorialScrollRoutine;

        public Gecko(Vector2 position, bool hostile, bool left, Vector2 node, float delay, string geckoId, string info, string controls, string reskinName) : base(position) {
            this.node = node;

            this.hostile = hostile;
            this.delay = delay;
            GeckoId = geckoId;
            this.left = left;
            if (hostile) {
                Add(sprite = JungleHelperModule.CreateReskinnableSprite(reskinName, "gecko_hostile"));
            } else {
                Add(sprite = JungleHelperModule.CreateReskinnableSprite(reskinName, "gecko_normal"));
            }
            sprite.Rotation = -1.5f;
            sprite.UseRawDeltaTime = true;
            sprite.Position.Y = -4f;

            if (!string.IsNullOrEmpty(info) || !string.IsNullOrEmpty(controls)) {
                Add(new VertexLight(new Vector2(0f, -8f), Color.White, 1f, 8, 32));

                // split infos and controls separated in multiple screens with | characters.
                string[] infoPages = info.Split('|');
                string[] controlPages = controls.Split('|');

                guis = new BirdTutorialGui[Math.Max(infoPages.Length, controlPages.Length)];
                for (int i = 0; i < guis.Length; i++) {
                    // if info or controls gets out of bounds, just consider them empty.
                    string thisInfo = infoPages.Length > i ? infoPages[i] : "";
                    string thisControl = controlPages.Length > i ? controlPages[i] : "";

                    // just leave Custom Bird Tutorial deal with the parsing, then steal the result from it.
                    CustomBirdTutorial caw = new CustomBirdTutorial(new EntityData() {
                        Values = new Dictionary<string, object>() { { "info", thisInfo }, { "controls", thisControl } },
                        Level = AreaData.Areas[0].Mode[0].MapData.Levels[0], // I just want some random level data to dodge NREs please
                        ID = -1
                    }, Vector2.Zero);
                    guis[i] = (BirdTutorialGui) birdTutorialGuiField.GetValue(caw);
                    guis[i].Entity = this;
                    guis[i].Position = new Vector2(left ? -4f : 4f, -20f);
                }
            }

            startPosition = Position;

            // if the two positions of the gecko are not overlapping, it should move.
            if (startPosition != node) {
                Add(new Coroutine(movement()));
            }

            if (left) {
                sprite.Scale.Y = -1f;
                Collider = new Hitbox(6f, 18f, -7f, -8f);
                sprite.X = 1;
            } else {
                Collider = new Hitbox(6f, 18f, 1f, 16f);
                sprite.X = -1;
            }
            Collider.CenterY = -3;

            Add(new PlayerCollider(onCollide));
        }

        public Gecko(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Bool("hostile", false), data.Bool("left", false), data.Nodes[0] + offset, data.Float("delay", 0.5f),
                  data.Attr("geckoId"), data.Attr("info"), data.Attr("controls"), data.Attr("sprite")) { }

        private void onCollide(Player player) {
            if (hostile) {
                moving = false;
                sprite.Play("hit");
                player.Die(new Vector2(left ? 1f : -1f, 0f));
            }
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            if (scene.Tracker.GetEntities<GeckoTutorialTrigger>().OfType<GeckoTutorialTrigger>()
                .All(trigger => !trigger.ShowTutorial || trigger.GeckoId != GeckoId)) {

                TriggerShowTutorial();
            }
        }

        public void TriggerShowTutorial() {
            if (!triggered && guis != null) {
                triggered = true;

                if (guis.Length == 1) {
                    // open the tutorial
                    Scene.Add(guis[0]);
                    guis[0].Open = true;
                } else {
                    // run the routine that will switch between all pages.
                    Add(birdTutorialScrollRoutine = new Coroutine(showAllTutorialsCoroutine()));
                }

                moving = false;
                sprite.Play("idle");
            }
        }

        public void TriggerHideTutorial() {
            if (triggered && !showedTutorial && guis != null) {
                showedTutorial = true;

                if (guis.Length == 1) {
                    // close the tutorial
                    guis[0].Open = false;
                } else {
                    // close all tutorials and stop the coroutine that switches between them.
                    birdTutorialScrollRoutine?.RemoveSelf();
                    foreach (BirdTutorialGui gui in guis) {
                        gui.Open = false;
                    }
                }

                moving = true;
            }
        }

        private IEnumerator showAllTutorialsCoroutine() {
            while (true) {
                // show first page
                Scene.Add(guis[0]);
                guis[0].Open = true;
                while (guis[0].Scale < 1f) {
                    yield return null;
                }

                // wait
                yield return 2f;

                for (int i = 1; i < guis.Length; i++) {
                    // show page N
                    guis[i].Open = true;
                    guis[i].Scale = 1f;
                    Scene.Add(guis[i]);
                    yield return null;

                    // hide page N - 1
                    guis[i - 1].Open = false;
                    guis[i - 1].Scale = 0f;
                    Scene.Remove(guis[i - 1]);

                    // wait
                    yield return 2f;
                }

                // hide last page
                int lastGui = guis.Length - 1;
                guis[lastGui].Open = false;
                while (guis[lastGui].Scale > 0f) {
                    yield return null;
                }
                Scene.Remove(guis[lastGui]);

                // wait
                yield return 2f;
            }
        }

        private void idle() {
            Random random = new Random();
            random.Range(0, 100);
            if (random.NextFloat() > 0.03 && random.NextFloat() < 0.04)
                sprite.Play("dance");
            else
                sprite.Play("idle");
        }

        private IEnumerator movement() {
            yield return 0.5f;
            Player p = Scene.Tracker.GetEntity<Player>();
            while (p != null) {
                while (!moving) {
                    yield return null;
                }

                sprite.Play("walk");
                sprite.Scale.X = Math.Sign(node.Y - Position.Y);

                while (Position != node) {
                    yield return null;
                    if (moving) {
                        Position = Calc.Approach(Position, node, 20f * Engine.DeltaTime);
                    }
                    if (CollideCheck<Solid>(Position + new Vector2(0, Math.Sign(node.Y - Position.Y)))) {
                        break;
                    }
                }

                idle();
                yield return delay;

                while (!moving) {
                    yield return null;
                }

                sprite.Play("walk");
                sprite.Scale.X = Math.Sign(startPosition.Y - Position.Y);

                while (Position != startPosition) {
                    yield return null;
                    if (moving) {
                        Position = Calc.Approach(Position, startPosition, 20f * Engine.DeltaTime);
                    }
                    if (CollideCheck<Solid>(Position + new Vector2(0, Math.Sign(startPosition.Y - Position.Y)))) {
                        break;
                    }
                }

                idle();
                yield return delay;
            }
            yield break;
        }

        public static Gecko FindById(Level level, string geckoId) {
            return level.Tracker.GetEntities<Gecko>()
                .OfType<Gecko>()
                .Where(gecko => gecko.GeckoId == geckoId)
                .FirstOrDefault();
        }
    }
}