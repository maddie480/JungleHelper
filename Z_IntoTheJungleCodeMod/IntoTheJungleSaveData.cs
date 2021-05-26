using System.Collections.Generic;

namespace Celeste.Mod.IntoTheJungleCodeMod {
    public class IntoTheJungleSaveData : EverestModuleSaveData {
        public HashSet<string> EndingDialogueBlacklist { get; set; } = new HashSet<string>();
        public int NoRareEndingCounter { get; set; } = 0;
    }
}
