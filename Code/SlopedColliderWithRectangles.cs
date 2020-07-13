using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JungleHelper {
    /// <summary>
    /// Slope collider, with horizontal top and bottom sides.
    /// <i>Those are not the "slopes in Celeste" you are looking for.</i>
    /// </summary>
    class SlopedColliderWithRectangles : ColliderList {

        private Vector2 topleft, topright, bottomleft, bottomright;

        public SlopedColliderWithRectangles(float topY, float bottomY, float topleftX, float toprightX, float bottomleftX, float bottomrightX) : base() {
            for (int i = (int) topY; i <= (int) bottomY; i++) {
                float progress = (i - topY) / (bottomY - topY);
                float left = MathHelper.Lerp(topleftX, bottomleftX, progress);
                float right = MathHelper.Lerp(toprightX, bottomrightX, progress);
                Add(new Hitbox(right - left, 1, left, i));
            }

            topleft = new Vector2(topleftX, topY);
            topright = new Vector2(toprightX, topY);
            bottomleft = new Vector2(bottomleftX, bottomY);
            bottomright = new Vector2(bottomrightX, bottomY);
        }

        public override void Render(Camera camera, Color color) {
            Draw.Line(topleft, topright, color);
            Draw.Line(topright, bottomright, color);
            Draw.Line(bottomright, bottomleft, color);
            Draw.Line(bottomleft, topleft, color);
        }

        public void Move(Vector2 move) {
            foreach(Collider collider in colliders) {
                collider.Position += move;
            }
            topleft += move;
            topright += move;
            bottomleft += move;
            bottomright += move;
        }
    }
}
