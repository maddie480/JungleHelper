using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.JungleHelper {
    /// <summary>
    /// Circle collider, except using multiple rectangles instead, allowing them to collide with tiles for example.
    /// </summary>
    class CircleColliderWithRectangles : ColliderList {
        private int radius;
        private float x;
        private float y;

        public CircleColliderWithRectangles(int radius, float x = 0f, float y = 0f) : base() {
            this.radius = radius;
            this.x = x;
            this.y = y;

            for (int i = -radius; i < radius; i++) {
                // I wasn't missing trigonometry hh
                float chunkWidth = (float) Math.Abs(Math.Cos(Math.Asin((double) i / radius)) * radius);
                Add(new Hitbox(chunkWidth * 2, 1, -chunkWidth + x, i + y));
            }
        }

        public override void Render(Camera camera, Color color) {
            Draw.Circle(AbsolutePosition + new Vector2(x, y), radius, color, 4);
        }
    }
}
