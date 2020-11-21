using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.JungleHelper.Entities {
    // functionally identical to an ice wall, except it looks like moss and can be dissolved with a lantern.
    [CustomEntity("JungleHelper/MossyWall")]
    class MossyWall : Entity {
        private const int LANTERN_ACTIVATION_RADIUS = 75;

        // colors used when player gets close but moss isn't dissolved yet (from closest to furthest, 1 line per pixel).
        private static readonly Color[] DISTANCE_BASED_COLORS = {
            Calc.HexToColor("7A612D") * 0.1f,
            Calc.HexToColor("7A612D") * 0.2f,
            Calc.HexToColor("7A612D") * 0.3f,
            Calc.HexToColor("7A612D") * 0.4f,
            Calc.HexToColor("7A612D") * 0.5f,
            Calc.HexToColor("7A612D") * 0.6f,
            Calc.HexToColor("7A612D") * 0.7f,
            Calc.HexToColor("7A612D") * 0.8f,
            Calc.HexToColor("7A612D") * 0.9f,
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
        private List<Hitbox> hitboxes = new List<Hitbox>();

        private Vector2 topCenter;
        private Vector2 shake;

        public MossyWall(EntityData data, Vector2 offset) : base(data.Position + offset) {
            bool left = data.Bool("left");

            Depth = -20000; // FG tiles have depth -10000

            Add(new StaticMover {
                SolidChecker = solid => CollideCheck(solid, Position + (left ? -2 : 2) * Vector2.UnitX),
                OnMove = move => {
                    Position += move;
                    topCenter += move;
                },
                OnShake = move => shake += move
            });
            Add(new ClimbBlocker(edge: false));

            for (int i = 0; i < data.Height; i += 8) {
                string id = (i == 0) ? "moss_top" : (i + 16 <= Height ? "moss_mid" + Calc.Random.Next(1, 3) : "moss_bottom");
                Image sprite = new Image(GFX.Game["JungleHelper/Moss/" + id]);
                sprite.Position = new Vector2(0f, i);
                sprite.FlipX = !left;
                sprite.Color = REGULAR_COLOR;
                Add(sprite);

                mossParts.Add(sprite);

                Hitbox hitbox;
                if (left) {
                    hitbox = new Hitbox(2f, 8f, 8f, i);
                } else {
                    hitbox = new Hitbox(2f, 8f, -2f, i);
                }
                hitboxes.Add(hitbox);
            }

            Collider = new ColliderList(hitboxes.ToArray());
            topCenter = TopCenter;
        }

        public override void Update() {
            base.Update();

            List<Hitbox> enabledHitboxes = new List<Hitbox>();

            for (int i = 0; i < mossParts.Count; i++) {
                Image mossPart = mossParts[i];
                Hitbox hitbox = hitboxes[i];

                float distance = Lantern.GetClosestLanternDistanceTo(topCenter + mossPart.Position, Scene, out _);

                if (distance < LANTERN_ACTIVATION_RADIUS) {
                    // moss is dissolved, make it uncollidable.
                    mossPart.Visible = false;
                } else if (distance - LANTERN_ACTIVATION_RADIUS < DISTANCE_BASED_COLORS.Length) {
                    // moss has a particular fade of color, make it collidable.
                    mossPart.Visible = true;
                    mossPart.Color = DISTANCE_BASED_COLORS[(int) Math.Floor(distance - LANTERN_ACTIVATION_RADIUS)];
                    enabledHitboxes.Add(hitbox);
                } else {
                    // moss is green (player is too far or not here), make it collidable.
                    mossPart.Visible = true;
                    mossPart.Color = REGULAR_COLOR;
                    enabledHitboxes.Add(hitbox);
                }
            }

            Collider = new ColliderList(enabledHitboxes.ToArray());
        }

        public override void Render() {
            Vector2 position = Position;
            Position += shake;
            base.Render();
            Position = position;
        }
    }
}
