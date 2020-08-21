using Celeste.Mod.Entities;
using Celeste.Mod.JungleHelper.Triggers;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.JungleHelper.Entities {
    [CustomEntity("JungleHelper/Gecko")]
    [Tracked]
    public class Gecko : Entity {
        private Sprite sprite;
        private Vector2 startPosition;

        private Vector2 node;

        private bool moving = true;
        private bool showedTutorial = false;
        private bool triggered = false;

        private readonly float delay;
        private readonly bool left = false;
        private readonly BirdTutorialGui gui;
        public readonly string GeckoId;
        private readonly bool hostile;

        public Gecko(Vector2 position, bool hostile, bool left, Vector2 node, float delay, string geckoId, string info, string controls) : base(position) {
            this.node = node;

            this.hostile = hostile;
            this.delay = delay;
            GeckoId = geckoId;
            this.left = left;
            if (hostile) {
                Add(sprite = JungleHelperModule.SpriteBank.Create("gecko_hostile"));
            } else {
                Add(sprite = JungleHelperModule.SpriteBank.Create("gecko_normal"));
            }
            sprite.Rotation = -1.5f;
            sprite.UseRawDeltaTime = true;
            sprite.Position.Y = -4f;

            if (!string.IsNullOrEmpty(info) || !string.IsNullOrEmpty(controls)) {
                Add(new VertexLight(new Vector2(0f, -8f), Color.White, 1f, 8, 32));

                // just leave Custom Bird Tutorial deal with the parsing, then steal the result from it.
                CustomBirdTutorial caw = new CustomBirdTutorial(new EntityData() {
                    Values = new Dictionary<string, object>() { { "info", info }, { "controls", controls } },
                    Level = AreaData.Areas[0].Mode[0].MapData.Levels[0], // I just want some random level data to dodge NREs please
                    ID = -1
                }, Vector2.Zero);
                gui = new DynData<CustomBirdTutorial>(caw).Get<BirdTutorialGui>("gui");
                gui.Entity = this;
                gui.Position = new Vector2(left ? -4f : 4f, -20f);
            }

            startPosition = Position;
            Add(new Coroutine(movement()));

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
                  data.Attr("geckoId"), data.Attr("info"), data.Attr("controls")) { }

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

            if (gui != null) {
                scene.Add(gui);
            }
        }

        public void TriggerShowTutorial() {
            if (!triggered && gui != null) {
                triggered = true;
                gui.Open = true;

                moving = false;
                sprite.Play("idle");
            }
        }

        public void TriggerHideTutorial() {
            if (triggered && !showedTutorial && gui != null) {
                showedTutorial = true;
                gui.Open = false;

                moving = true;
            }
        }

        private void idle() {
            Random random = new Random();
            random.Range(0, 100);
            Console.WriteLine(random.NextFloat());
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