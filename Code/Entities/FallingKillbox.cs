using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JungleHelper.Entities {
    /// <summary>
    /// A killbox, except it's falling. :wow:
    /// </summary>
    [CustomEntity("JungleHelper/FallingKillbox")]
    [TrackedAs(typeof(Killbox))]
    class FallingKillbox : Killbox {
        private readonly float fallingSpeed;

        public FallingKillbox(EntityData data, Vector2 offset) : base(data, offset) {
            fallingSpeed = data.Float("fallingSpeed", 100f);
        }

        public override void Update() {
            base.Update();

            // fall
            Position.Y += fallingSpeed * Engine.DeltaTime;
        }
    }
}
