using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.JungleHelper.Entities {
    [CustomEntity("JungleHelper/SpinyPlant")]
    class SpinyPlant : Entity {
        private const int LANTERN_ACTIVATION_RADIUS = 75;

        private int lines;
        private string color;

        public SpinyPlant(EntityData data, Vector2 offset) : base(data.Position + offset) {
            lines = data.Height / 8;
            color = data.Attr("color", "Blue");
            Depth = -9500;

            Collider = new Hitbox(12f, data.Height - 8f, 6f, 4f);
            Add(new PlayerCollider(killPlayer));
        }

        private void killPlayer(Player player) {
            player?.Die(new Vector2(Math.Sign(player.X - X - 12), 0));
        }

        public override void Awake(Scene scene) {
            if (lines == 2) {
                Collider bak = Collider;
                Collider = new Hitbox(8f, 1f, 8, -1);
                bool topOpen = !CollideCheck<Solid>();
                Collider = new Hitbox(8f, 1f, 8, lines * 8);
                bool bottomOpen = !CollideCheck<Solid>();
                Collider = bak;

                string section;
                if (topOpen) {
                    if (bottomOpen) {
                        section = "Solo";
                    } else {
                        section = "Top";
                    }
                } else {
                    if (bottomOpen) {
                        section = "Bottom";
                    } else {
                        section = "Mid";
                    }
                }
                // special case (height 16 / 2 "lines"): use the "solo" sprite
                GraphicsComponent image = generateSpinyPlantPart(section);
                image.X = section == "Solo" ? 4 : 0;
                Add(image);
            } else {
                for (int i = 0; i < lines; i += 2) {
                    if (i == lines - 1) {
                        i = lines - 2;
                    }

                    string section = "Mid";
                    if (i == 0) {
                        Collider bak = Collider;
                        Collider = new Hitbox(8f, 1f, 8, -1);
                        bool solidAbove = CollideCheck<Solid>();
                        Collider = bak;

                        if (!solidAbove) {
                            section = "Top";
                        } else {
                            Collider.Top -= 4f;
                            Collider.Height += 4f;
                        }
                    } else if (i == lines - 2) {
                        Collider bak = Collider;
                        Collider = new Hitbox(8f, 1f, 8, lines * 8);
                        bool solidBelow = CollideCheck<Solid>();
                        Collider = bak;

                        if (!solidBelow) {
                            section = "Bottom";
                        } else {
                            Collider.Height += 4f;
                        }
                    }

                    GraphicsComponent image = generateSpinyPlantPart(section);
                    image.Y = i * 8;
                    Add(image);
                }
            }
        }

        private GraphicsComponent generateSpinyPlantPart(string section) {
            if (color == "Yellow") {
                // use a sprite so that we can trigger the retract/expand animations.
                return JungleHelperModule.SpriteBank.Create($"spiny_plant_{color.ToLowerInvariant()}_{section.ToLowerInvariant()}");
            } else {
                // use an image because we only have a static image anyway.
                return new Image(GFX.Game[$"JungleHelper/SpinyPlant/Spiny{color}{section}"]);
            }
        }

        public override void Update() {
            base.Update();

            // this is collidable by default, until we figure out that a part of the plant is close enough to the player.
            Collidable = true;

            foreach (Sprite plantPart in Components.GetAll<Sprite>()) {
                float distance = Lantern.GetClosestLanternDistanceTo(TopCenter + plantPart.Position, Scene, out Vector2 objectPosition);

                if (distance < LANTERN_ACTIVATION_RADIUS) {
                    // plant is retracted!
                    Collidable = false;

                    // is this part retracted yet?
                    if (plantPart.CurrentAnimationID.StartsWith("extend")) {
                        // no, so go ahead and retract it.
                        int frame = (plantPart.CurrentAnimationID == "extended" ? 0 : 6 - plantPart.CurrentAnimationFrame);
                        plantPart.Play(Top + plantPart.Position.Y - objectPosition.Y < 0 ? "retract_below" : "retract_above");
                        plantPart.SetAnimationFrame(frame);
                    }
                } else if (plantPart.CurrentAnimationID.StartsWith("retract")) {
                    // we are out of radius and retracting/retracted, so extend.
                    int frame = (plantPart.CurrentAnimationID == "retracted" ? 0 : 6 - plantPart.CurrentAnimationFrame);
                    plantPart.Play(Top + plantPart.Position.Y - objectPosition.Y < 0 ? "extend_below" : "extend_above");
                    plantPart.SetAnimationFrame(frame);
                }
            }
        }
    }
}
