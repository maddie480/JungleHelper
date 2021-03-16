using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JungleHelper.Entities {
    [CustomEntity("JungleHelper/Dragonfly")]
    class Dragonfly : Entity {
        public Dragonfly(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Depth = 5000;

            float frameOffset = (float) Calc.Random.NextDouble();

            Sprite body = JungleHelperModule.CreateReskinnableSprite(data, "dragonfly");
            body.PlayOffset("body", frameOffset);
            Add(body);

            Sprite wings = JungleHelperModule.CreateReskinnableSprite(data, "dragonfly");
            wings.PlayOffset("wings", frameOffset);
            wings.Color = Calc.HexToColor(data.Attr("wingsColor"));
            Add(wings);
        }
    }
}
