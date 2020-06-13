using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.JungleHelper {
    [CustomEntity("JungleHelper/SpinyPlant")]
    class SpinyPlant : Entity {
        private int lines;
        private string color;

        public SpinyPlant(EntityData data, Vector2 offset) : base(data.Position + offset) {
            lines = data.Height / 8;
            color = data.Attr("color", "Blue");
            Depth = -60;

            Collider = new Hitbox(12f, data.Height - 8f, 6f, 4f);
            Add(new PlayerCollider(killPlayer));
        }

        private void killPlayer(Player player) {
            player?.Die(new Vector2(Math.Sign(player.X - X - 12), 0));
        }

        public override void Awake(Scene scene) {
            MTexture top = GFX.Game[$"JungleHelper/SpinyPlant/Spiny{color}Top"];
            MTexture middle = GFX.Game[$"JungleHelper/SpinyPlant/Spiny{color}Mid"];
            MTexture bottom = GFX.Game[$"JungleHelper/SpinyPlant/Spiny{color}Bottom"];

            if (lines == 2) {
                // special case (height 16 / 2 "lines"): use the "solo" sprite
                Image image = new Image(GFX.Game[$"JungleHelper/SpinyPlant/Spiny{color}Solo"]);
                image.X = 4;
                Add(image);
            } else {
                for (int i = 0; i < lines; i += 2) {
                    if (i == lines - 1) {
                        i = lines - 2;
                    }

                    MTexture texture = middle;
                    if (i == 0) {
                        Collider bak = Collider;
                        Collider = new Hitbox(12f, 1f, 0, -1);
                        bool solidAbove = CollideCheck<Solid>();
                        Collider = bak;

                        if (!solidAbove) {
                            texture = top;
                        } else {
                            Collider.Top -= 4f;
                            Collider.Height += 4f;
                        }
                    } else if (i == lines - 2) {
                        Collider bak = Collider;
                        Collider = new Hitbox(12f, 1f, 0, lines * 8);
                        bool solidBelow = CollideCheck<Solid>();
                        Collider = bak;

                        if (!solidBelow) {
                            texture = bottom;
                        } else {
                            Collider.Height += 4f;
                        }
                    }

                    Image image = new Image(texture);
                    image.Y = i * 8;
                    Add(image);
                }
            }
        }
    }
}
