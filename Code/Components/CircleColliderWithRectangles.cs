using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.JungleHelper.Components {
    /// <summary>
    /// Circle collider, except using multiple rectangles instead, allowing them to collide with tiles for example.
    /// </summary>
    public class CircleColliderWithRectangles : ColliderList {
        private int radius;
        private float x;
        private float y;

        private Hitbox enclosingRectangle;

        public CircleColliderWithRectangles(int radius, float x = 0f, float y = 0f) : base() {
            this.radius = radius;
            this.x = x;
            this.y = y;

            for (int i = -radius; i < radius; i++) {
                // I wasn't missing trigonometry hh
                float chunkWidth = (float) Math.Abs(Math.Cos(Math.Asin((double) i / radius)) * radius);
                Add(new Hitbox(chunkWidth * 2, 1, -chunkWidth + x, i + y));
            }

            enclosingRectangle = new Hitbox(Width, Height, Left, Top);

            // we need to add the enclosing rectangle to the collider list, so that it is attached to the entity and its absolute position is correct.
            // (and so that it doesn't crash CelesteTAS "show hitboxes" when rendered, too...)
            Add(enclosingRectangle);
        }

        public override void Render(Camera camera, Color color) {
            enclosingRectangle.Render(camera, Color.Blue);
            Draw.Circle(AbsolutePosition + new Vector2(x, y), radius, color, 4);
        }

        // if something doesn't hit the enclosing rectangle, it **cannot** hit the circle, so we can skip all the collide checks with rectangles.
        // so, collide with the enclosing rectangle, then if it collides, remove it temporarily from the collider list, and collide check with the rest of the rectangles.

        public override bool Collide(Vector2 point) {
            if (!enclosingRectangle.Collide(point)) {
                return false;
            }

            Remove(enclosingRectangle);
            bool result = base.Collide(point);
            Add(enclosingRectangle);
            return result;
        }

        public override bool Collide(Rectangle rect) {
            if (!enclosingRectangle.Collide(rect)) {
                return false;
            }

            Remove(enclosingRectangle);
            bool result = base.Collide(rect);
            Add(enclosingRectangle);
            return result;
        }

        public override bool Collide(Vector2 from, Vector2 to) {
            if (!enclosingRectangle.Collide(from, to)) {
                return false;
            }

            Remove(enclosingRectangle);
            bool result = base.Collide(from, to);
            Add(enclosingRectangle);
            return result;
        }

        public override bool Collide(Hitbox hitbox) {
            if (!enclosingRectangle.Collide(hitbox)) {
                return false;
            }

            Remove(enclosingRectangle);
            bool result = base.Collide(hitbox);
            Add(enclosingRectangle);
            return result;
        }

        public override bool Collide(Grid grid) {
            if (!enclosingRectangle.Collide(grid)) {
                return false;
            }

            Remove(enclosingRectangle);
            bool result = base.Collide(grid);
            Add(enclosingRectangle);
            return result;
        }

        public override bool Collide(Circle circle) {
            if (!enclosingRectangle.Collide(circle)) {
                return false;
            }

            Remove(enclosingRectangle);
            bool result = base.Collide(circle);
            Add(enclosingRectangle);
            return result;
        }

        public override bool Collide(ColliderList list) {
            if (!enclosingRectangle.Collide(list)) {
                return false;
            }

            Remove(enclosingRectangle);
            bool result = base.Collide(list);
            Add(enclosingRectangle);
            return result;
        }
    }
}
