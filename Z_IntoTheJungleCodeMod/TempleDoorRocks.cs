using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.IntoTheJungleCodeMod {
    // this is literally just a sprite, but it needs to be its own entity because it has a different depth compared to the door.
    class TempleDoorRocks : Entity {
        public readonly Sprite Sprite;

        public TempleDoorRocks(Vector2 position) : base(position) {
            Add(Sprite = IntoTheJungleCodeModule.SpriteBank.Create("temple_door_rocks"));
            Depth = 8997; // in front of the totems
        }
    }
}