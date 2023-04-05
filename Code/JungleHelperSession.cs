using YamlDotNet.Serialization;

namespace Celeste.Mod.JungleHelper {
    public class JungleHelperSession : EverestModuleSession {
        public bool GrablessBerryFlewAway { get; set; } = false;

        [YamlIgnore]
        public bool GrablessBerryWillFlyAway { get; set; } = false;
    }
}
