using Celeste.Editor;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.IntoTheJungleCodeMod {
    public class IntoTheJungleCodeModule : EverestModule {
        private static FieldInfo mapEditorLevelList = typeof(MapEditor).GetField("levels", BindingFlags.Instance | BindingFlags.NonPublic);

        public override void Load() {
            On.Celeste.Editor.MapEditor.ctor += modHideMap;
        }

        public override void Unload() {
            On.Celeste.Editor.MapEditor.ctor -= modHideMap;
        }

        private static void modHideMap(On.Celeste.Editor.MapEditor.orig_ctor orig, MapEditor self, AreaKey area, bool reloadMapData) {
            orig(self, area, reloadMapData);

            if (area.GetLevelSet() == "Into The Jungle") {
                List<LevelTemplate> mapList = (List<LevelTemplate>) mapEditorLevelList.GetValue(self);
                for (int i = mapList.Count - 1; i >= 0; i--) {
                    // Hide all maps which names end with _HideInMap
                    if (mapList[i].Name.EndsWith("_HideInMap")) {
                        mapList.Remove(mapList[i]);
                    }
                }
            }
        }
    }
}
