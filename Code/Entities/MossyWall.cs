using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.JungleHelper.Entities {
    // functionally identical to an ice wall, except it looks like moss and can be dissolved with a lantern.
    [CustomEntity("JungleHelper/MossyWall")]
    class MossyWall : Entity {
        // colors used when player gets closed but moss isn't dissolved yet (from closest to furthest, 1 line per pixel).
        private static readonly Color[] DISTANCE_BASED_COLORS = {
            new Color(Calc.HexToColor("7A612D"), 25),
            new Color(Calc.HexToColor("7A612D"), 50),
            new Color(Calc.HexToColor("7A612D"), 75),
            new Color(Calc.HexToColor("7A612D"), 100),
            new Color(Calc.HexToColor("7A612D"), 125),
            new Color(Calc.HexToColor("7A612D"), 150),
            new Color(Calc.HexToColor("7A612D"), 175),
            new Color(Calc.HexToColor("7A612D"), 200),
            new Color(Calc.HexToColor("7A612D"), 225),
            Calc.HexToColor("7A612D"),
            Color.Lerp(Calc.HexToColor("7A612D"), Calc.HexToColor("AABF3D"), 0.1f),
            Color.Lerp(Calc.HexToColor("7A612D"), Calc.HexToColor("AABF3D"), 0.2f),
            Color.Lerp(Calc.HexToColor("7A612D"), Calc.HexToColor("AABF3D"), 0.3f),
            Color.Lerp(Calc.HexToColor("7A612D"), Calc.HexToColor("AABF3D"), 0.4f),
            Color.Lerp(Calc.HexToColor("7A612D"), Calc.HexToColor("AABF3D"), 0.5f),
            Color.Lerp(Calc.HexToColor("7A612D"), Calc.HexToColor("AABF3D"), 0.6f),
            Color.Lerp(Calc.HexToColor("7A612D"), Calc.HexToColor("AABF3D"), 0.7f),
            Color.Lerp(Calc.HexToColor("7A612D"), Calc.HexToColor("AABF3D"), 0.8f),
            Color.Lerp(Calc.HexToColor("7A612D"), Calc.HexToColor("AABF3D"), 0.9f),
            Calc.HexToColor("AABF3D"),
            Color.Lerp(Calc.HexToColor("AABF3D"), Calc.HexToColor("33C111"), 0.1f),
            Color.Lerp(Calc.HexToColor("AABF3D"), Calc.HexToColor("33C111"), 0.2f),
            Color.Lerp(Calc.HexToColor("AABF3D"), Calc.HexToColor("33C111"), 0.3f),
            Color.Lerp(Calc.HexToColor("AABF3D"), Calc.HexToColor("33C111"), 0.4f),
            Color.Lerp(Calc.HexToColor("AABF3D"), Calc.HexToColor("33C111"), 0.5f),
            Color.Lerp(Calc.HexToColor("AABF3D"), Calc.HexToColor("33C111"), 0.6f),
            Color.Lerp(Calc.HexToColor("AABF3D"), Calc.HexToColor("33C111"), 0.7f),
            Color.Lerp(Calc.HexToColor("AABF3D"), Calc.HexToColor("33C111"), 0.8f),
            Color.Lerp(Calc.HexToColor("AABF3D"), Calc.HexToColor("33C111"), 0.9f)
        };

        // color used when the player is too far from the mossy wall, or doesn't hold the lantern.
        private static readonly Color REGULAR_COLOR = Calc.HexToColor("33C111");

        private List<Image> mossParts = new List<Image>();

        public MossyWall(EntityData data, Vector2 offset) : base(data.Position + offset) {
            float height = data.Height;
            bool left = data.Bool("left");

            Depth = -20000; // FG tiles have depth -10000

            if (left) {
                Collider = new Hitbox(2f, height, 8f);
            } else {
                Collider = new Hitbox(2f, height, -2f);
            }

            Add(new StaticMover());
            Add(new ClimbBlocker(edge: false));

            for (int i = 0; i < Height; i += 8) {
                string id = (i == 0) ? "moss_top" : (i + 16 <= Height ? "moss_mid" + Calc.Random.Next(1, 3) : "moss_bottom");
                Image sprite = new Image(GFX.Game["JungleHelper/Moss/" + id]);
                sprite.Position = new Vector2(0f, i);
                sprite.FlipX = !left;
                sprite.Color = REGULAR_COLOR;
                Add(sprite);

                mossParts.Add(sprite);
            }
        }

        public override void Update() {
            base.Update();

            Player maddy = Scene.Tracker.GetEntity<Player>();
            if (maddy?.Sprite.Mode != Lantern.SpriteModeMadelineLantern) {
                maddy = null; // Maddy has no torch = Maddy is not here.
            }

            // this is collidable by default, until we figure out that a moss part is close enough to the player.
            Collidable = true;

            foreach (Image mossPart in mossParts) {
                float distance = float.MaxValue;
                if (maddy != null) {
                    distance = (TopCenter + mossPart.Position - maddy.Position).Length();
                }

                if (distance < Lantern.ACTIVATION_RADIUS) {
                    // moss is dissolved, make it uncollidable.
                    mossPart.Color.A = 0;
                    Collidable = false;
                } else if (distance - Lantern.ACTIVATION_RADIUS < DISTANCE_BASED_COLORS.Length) {
                    // moss has a particular fade of color.
                    mossPart.Color = DISTANCE_BASED_COLORS[(int) Math.Floor(distance - Lantern.ACTIVATION_RADIUS)];
                } else {
                    // moss is green (player is too far or not here).
                    mossPart.Color = REGULAR_COLOR;
                }
            }
        }
    }
}
