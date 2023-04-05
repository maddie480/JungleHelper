using System;
using System.Collections.Generic;

namespace Celeste.Mod.JungleHelper {
    public class JungleHelperMapDataProcessor : EverestMapDataProcessor {
        private int checkpoint;

        public override void Reset() {
            checkpoint = 0;
        }

        public override void End() {
            // nothing needed
        }

        public override Dictionary<string, Action<BinaryPacker.Element>> Init()
            => new Dictionary<string, Action<BinaryPacker.Element>>() {
                { "entity:checkpoint", entity => {
                    checkpoint++;
                } },

                // make sure our custom cassette is detected as a cassette.
                { "entity:JungleHelper/CassetteCustomPreviewMusic", entity => {
                    if (AreaData.CassetteCheckpointIndex < 0) {
                        AreaData.CassetteCheckpointIndex = checkpoint;
                    }
                    if (ParentAreaData.CassetteCheckpointIndex < 0) {
                        ParentAreaData.CassetteCheckpointIndex = checkpoint + (ParentMode.Checkpoints?.Length ?? 0);
                    }

                    MapData.DetectedCassette = true;
                    ParentMapData.DetectedCassette = true;
                } },

                // the treasure chest contains a heart: have it detected like a heart.
                { "entity:JungleHelper/TreasureChest", entity => {
                    MapData.DetectedHeartGem = true;
                } }
            };
    }
}
