﻿using Celeste.Editor;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.IntoTheJungleCodeMod {
    public class IntoTheJungleCodeModule : EverestModule {
        private static FieldInfo mapEditorLevelList = typeof(MapEditor).GetField("levels", BindingFlags.Instance | BindingFlags.NonPublic);

        public override void Load() {
            On.Celeste.Editor.MapEditor.ctor += modHideMap;
            IL.Celeste.Overworld.Update += modOverworldUpdate;
        }

        public override void Unload() {
            On.Celeste.Editor.MapEditor.ctor -= modHideMap;
            IL.Celeste.Overworld.Update -= modOverworldUpdate;
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

        private void modOverworldUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Overworld>("IsCurrent"))) {
                // this is one of the checks to prevent custom music from leaking into the main menu
                // ... but we *want* it to play on the main menu, so we need to skip that check if we are in the Into The Jungle code mod.
                cursor.EmitDelegate<Func<bool, bool>>(orig => {
                    return orig || SaveData.Instance.LevelSet == "Into The Jungle";
                });
            }
        }
    }
}
