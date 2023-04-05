using Celeste.Editor;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.IntoTheJungleCodeMod {
    public class IntoTheJungleCodeModule : EverestModule {
        public override Type SaveDataType => typeof(IntoTheJungleSaveData);
        public static IntoTheJungleCodeModule Instance;

        public static IntoTheJungleSaveData ModSaveData => (IntoTheJungleSaveData) Instance._SaveData;
        public static HashSet<string> EndingDialogueBlacklist => ((IntoTheJungleSaveData) Instance._SaveData).EndingDialogueBlacklist;

        private static FieldInfo mapEditorLevelList = typeof(MapEditor).GetField("levels", BindingFlags.Instance | BindingFlags.NonPublic);

        private static ILHook hookOuiFileSelectRender;
        public static SpriteBank SpriteBank;

        public IntoTheJungleCodeModule() {
            Instance = this;
        }

        public override void Load() {
            On.Celeste.Editor.MapEditor.ctor += modHideMap;
            IL.Celeste.Overworld.Update += modOverworldUpdate;
            On.Celeste.SaveData.AfterInitialize += updateChapter5Name;

            hookOuiFileSelectRender = new ILHook(typeof(OuiFileSelectSlot).GetMethod("orig_Render"), patchChapter5NameOnFileSelectSlot);
        }

        public override void Unload() {
            On.Celeste.Editor.MapEditor.ctor -= modHideMap;
            IL.Celeste.Overworld.Update -= modOverworldUpdate;
            On.Celeste.SaveData.AfterInitialize -= updateChapter5Name;

            hookOuiFileSelectRender?.Dispose();
            hookOuiFileSelectRender = null;
        }

        public override void LoadContent(bool firstLoad) {
            base.LoadContent(firstLoad);
            SpriteBank = new SpriteBank(GFX.Game, "Graphics/IntoTheJungleCodeMod/Sprites.xml");
        }

        public override void PrepareMapDataProcessors(MapDataFixup context) {
            base.PrepareMapDataProcessors(context);

            context.Add<IntoTheJungleMapDataProcessor>();
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
                    // Remove room "Filler5" in ch4 A-side
                    if (area.GetSID() == "Into The Jungle/ch4" && area.Mode == AreaMode.Normal && mapList[i].Name == "Filler5") {
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

        // for use in Lua cutscenes
        public static void DirectionalShakeRegardlessOfSetting(Level level, Vector2 dir, float time = 0.3f) {
            DynData<Level> levelData = new DynData<Level>(level);

            levelData["shakeDirection"] = dir.SafeNormalize();
            levelData["lastDirectionalShake"] = 0;
            levelData["shakeTimer"] = Math.Max(levelData.Get<float>("shakeTimer"), time);
        }

        private void updateChapter5Name(On.Celeste.SaveData.orig_AfterInitialize orig, SaveData self) {
            orig(self);

            AreaData chapter5 = AreaData.Get("Into The Jungle/ch5");
            if (chapter5 != null) {
                if (self.FoundAnyCheckpoints(chapter5.ToKey()) || (self.GetAreaStatsFor(chapter5.ToKey())?.Cassette ?? false)) {
                    chapter5.Name = "Into_The_Jungle_ch5";
                } else {
                    chapter5.Name = "Into_The_Jungle_ch5_unrevealed";
                }
            }
        }

        private void patchChapter5NameOnFileSelectSlot(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<AreaData>("Name"))) {
                Logger.Log("IntoTheJungleCodeMod", $"Patching file select slot chapter name at {cursor.Index} in IL for OuiFileSelectSlot.orig_Render");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<string, OuiFileSelectSlot, string>>((orig, self) => {
                    if (orig == "Into The Jungle/ch5" || orig == "Into_The_Jungle_ch5" || orig == "Into_The_Jungle_ch5_unrevealed") {
                        // show or hide the name depending on if a checkpoint was found or not.
                        AreaData chapter5 = AreaData.Get("Into The Jungle/ch5");
                        if (chapter5 != null) {
                            if (self.SaveData.FoundAnyCheckpoints(chapter5.ToKey()) || (self.SaveData.GetAreaStatsFor(chapter5.ToKey())?.Cassette ?? false)) {
                                return "Into_The_Jungle_ch5";
                            } else {
                                return "Into_The_Jungle_ch5_unrevealed";
                            }
                        }
                    }

                    return orig;
                });
            }
        }
    }
}
