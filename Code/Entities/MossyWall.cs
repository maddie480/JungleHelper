using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JungleHelper.Entities {
    // This is WallBooster, except with only the code from the Ice Wall mode, and with the
    // animated sprite replaced with static images.
    // So... yes, there is not much left to see here.
    [CustomEntity("JungleHelper/MossyWall")]
    class MossyWall : Entity {
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
                string id = (i == 0) ? "mossTop" : (i + 16 <= Height ? "mossMid" : "mossBottom");
                Image sprite = new Image(GFX.Game["JungleHelper/Moss/" + id]);
                sprite.Position = new Vector2(left ? 4f : -4f, i);
                sprite.FlipX = !left;
                Add(sprite);
            }
        }
    }
}
