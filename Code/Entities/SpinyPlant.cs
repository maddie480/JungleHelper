using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.JungleHelper.Entities {
    [CustomEntity("JungleHelper/SpinyPlant")]
    class SpinyPlant : Entity {
        private const int LANTERN_ACTIVATION_RADIUS = 75;

        private int lines;
        private string color;

        private Vector2 topCenter;
        private List<Sprite> plantParts;
        private List<Hitbox> hitboxes;

        public SpinyPlant(EntityData data, Vector2 offset) : base(data.Position + offset) {
            lines = data.Height / 8;
            color = data.Attr("color", "Blue");
            Depth = -9500;

            Add(new PlayerCollider(killPlayer));
        }

        private void killPlayer(Player player) {
            player?.Die(new Vector2(Math.Sign(player.X - X - 12), 0));
        }

        public override void Awake(Scene scene) {
            plantParts = new List<Sprite>();
            hitboxes = new List<Hitbox>();

            if (lines == 2) {
                Collider = new Hitbox(8f, 1f, 8, -1);
                bool topOpen = !CollideCheck<Solid>();
                Collider = new Hitbox(8f, 1f, 8, lines * 8);
                bool bottomOpen = !CollideCheck<Solid>();

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
                hitboxes.Add(generateHitbox(section, image));
                if (image is Sprite sprite) {
                    plantParts.Add(sprite);
                }
            } else {
                for (int i = 0; i < lines; i += 2) {
                    if (i == lines - 1) {
                        i = lines - 2;
                    }

                    string section = "Mid";
                    if (i == 0) {
                        Collider = new Hitbox(8f, 1f, 8, -1);
                        bool solidAbove = CollideCheck<Solid>();

                        if (!solidAbove) {
                            section = "Top";
                        }
                    } else if (i == lines - 2) {
                        Collider = new Hitbox(8f, 1f, 8, lines * 8);
                        bool solidBelow = CollideCheck<Solid>();

                        if (!solidBelow) {
                            section = "Bottom";
                        }
                    }

                    GraphicsComponent image = generateSpinyPlantPart(section);
                    image.Y = i * 8;
                    Add(image);
                    hitboxes.Add(generateHitbox(section, image));
                    if (image is Sprite sprite) {
                        plantParts.Add(sprite);
                    }
                }
            }

            Collider = new ColliderList(hitboxes.ToArray());
            topCenter = TopCenter;
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

        private Hitbox generateHitbox(string section, GraphicsComponent image) {
            switch (section) {
                case "Top":
                    return new Hitbox(12f, 12f, 6f, 4f + image.Y);
                case "Mid":
                    return new Hitbox(12f, 16f, 6f, image.Y);
                case "Bottom":
                    return new Hitbox(12f, 12f, 6f, image.Y);
                default: // case "Solo"
                    return new Hitbox(12f, 8f, 6f, 4f + image.Y);
            }
        }

        public override void Update() {
            base.Update();

            if (plantParts.Count == 0) {
                // this plant doesn't support retracting/expanding, so skip over everything.
                return;
            }

            List<Hitbox> activeHitboxes = new List<Hitbox>();

            for (int i = 0; i < plantParts.Count; i++) {
                Sprite plantPart = plantParts[i];
                Hitbox hitbox = hitboxes[i];

                float distance = Lantern.GetClosestLanternDistanceTo(topCenter + plantPart.Position, Scene, out Vector2 objectPosition);

                if (distance < LANTERN_ACTIVATION_RADIUS) {
                    // plant is retracted!

                    // is this part retracted yet?
                    if (plantPart.CurrentAnimationID.StartsWith("extend")) {
                        // no, so go ahead and retract it.
                        int frame = (plantPart.CurrentAnimationID == "extended" ? 0 : 6 - plantPart.CurrentAnimationFrame);
                        plantPart.Play(topCenter.Y + plantPart.Position.Y - objectPosition.Y < 0 ? "retract_below" : "retract_above");
                        plantPart.SetAnimationFrame(frame);
                    }
                } else {
                    // plant is extended, so it hurts the player.
                    activeHitboxes.Add(hitbox);

                    if (plantPart.CurrentAnimationID.StartsWith("retract")) {
                        // we are out of radius and retracting/retracted, so extend.
                        int frame = (plantPart.CurrentAnimationID == "retracted" ? 0 : 6 - plantPart.CurrentAnimationFrame);
                        plantPart.Play(topCenter.Y + plantPart.Position.Y - objectPosition.Y < 0 ? "extend_below" : "extend_above");
                        plantPart.SetAnimationFrame(frame);
                    }
                }
            }

            Collider = new ColliderList(activeHitboxes.ToArray());
        }
    }
}
