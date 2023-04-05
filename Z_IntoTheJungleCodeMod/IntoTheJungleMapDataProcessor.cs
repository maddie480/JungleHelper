using System;
using System.Collections.Generic;

namespace Celeste.Mod.IntoTheJungleCodeMod {
    class IntoTheJungleMapDataProcessor : EverestMapDataProcessor {
        public override void Reset() {
            // nothing needed
        }

        public override void End() {
            // nothing needed
        }

        public override Dictionary<string, Action<BinaryPacker.Element>> Init()
            => new Dictionary<string, Action<BinaryPacker.Element>>() {
                // detect the lantern heart like a crystal heart.
                { "entity:IntoTheJungleCodeMod/LanternHeartSpawner", entity => {
                    MapData.DetectedHeartGem = true;
                } }
            };
    }
}
