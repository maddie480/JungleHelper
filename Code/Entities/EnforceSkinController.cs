using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System.Reflection;
using Monocle;
using System;
using Microsoft.Xna.Framework;
using System.Linq;
using MonoMod.Cil;
using System.Collections;
using FMOD.Studio;
using Mono.Cecil;

namespace Celeste.Mod.JungleHelper.Entities {
    static class EnforceSkinController {
        private const PlayerSpriteMode SpriteModeMadelineNormal = (PlayerSpriteMode) 444480;
        private const PlayerSpriteMode SpriteModeBadelineNormal = (PlayerSpriteMode) 444481;
        private const PlayerSpriteMode SpriteModeMadelineLantern = (PlayerSpriteMode) 444482;
        private const PlayerSpriteMode SpriteModeBadelineLantern = (PlayerSpriteMode) 444483;

        private static FieldInfo playerNextSpriteMode = typeof(Player).GetField("nextSpriteMode", BindingFlags.NonPublic | BindingFlags.Instance);

        private static Hook hookVariantMode;
        private static Hook hookEmoteMod;

        private static bool disabledMarioSkin = false;
        private static bool forceMarioSkinDisabled = false;

        private static bool disabledBananaSkin = false;
        private static bool forceBananaSkinDisabled = false;

        private static bool disabledKaydenSkin = false;
        private static bool forceKaydenSkinDisabled = false;

        private static string disabledSkinModHelperSkin = null;
        private static bool forceSkinModHelperDisabled = false;

        private static bool skinsDisabled = false;

        private static bool showForceSkinsDisabledPostcard = false;
        private static Postcard forceSkinsDisabledPostcard;

        public static void Load() {
            On.Celeste.PlayerSprite.ctor += onPlayerSpriteConstructor;

            On.Celeste.LevelEnter.Go += onLevelEnter;
            On.Celeste.LevelLoader.ctor += onLevelLoad;
            On.Celeste.LevelExit.ctor += onLevelExit;

            IL.Celeste.Player.UpdateHair += onPlayerUpdateHair;

            On.Celeste.LevelEnter.Routine += addForceSkinsDisabledPostcard;
            On.Celeste.LevelEnter.BeforeRender += addForceSkinsDisabledPostcardRendering;

            On.Celeste.Mod.EverestModule.CreateModMenuSection += greyOutCodeModSkinToggles;

            // the method called when changing the "Other Self" variant is a method defined inside Level.VariantMode(). patching it requires a bit of _fun_
            hookVariantMode = new Hook(findOutVariantModeType().GetMethod("<VariantMode>b__9", BindingFlags.NonPublic | BindingFlags.Instance),
                typeof(EnforceSkinController).GetMethod("levelChangePlayAsBadeline", BindingFlags.NonPublic | BindingFlags.Static));
        }

        public static void Initialize() {
            if (Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "EmoteMod", Version = new Version(1, 5, 0) })) {
                MethodInfo playerResetSpriteHook = Everest.Modules.Where(module => module.GetType().FullName == "Celeste.Mod.EmoteMod.EmoteModMain")
                    .First().GetType().Assembly.GetType("Celeste.Mod.EmoteMod.BackpackModule").GetMethod("Player_ResetSprite", BindingFlags.Static | BindingFlags.NonPublic);

                hookEmoteMod = new Hook(playerResetSpriteHook, typeof(EnforceSkinController).GetMethod("hookEmoteModBackpackHook", BindingFlags.NonPublic | BindingFlags.Static));
            }
        }

        private static Type findOutVariantModeType() {
            // the "display class" type that contains the Play as Badeline code is used for the first variable in the Level.VariantMode method. Find this out!
            ModuleDefinition celeste = Everest.Relinker.SharedRelinkModuleMap["Celeste.Mod.mm"];
            MethodDefinition variantModeMethod = celeste.GetType("Celeste.Level").FindMethod("System.Void VariantMode(System.Int32,System.Boolean)");
            Type resolvedType = variantModeMethod.Body.Variables[0].VariableType.ResolveReflection();
            Logger.Log("JungleHelper/EnforceSkinController", "Nested type associated to Level.VariantMode is: " + resolvedType.FullName);
            return resolvedType;
        }

        public static void Unload() {
            On.Celeste.PlayerSprite.ctor -= onPlayerSpriteConstructor;

            On.Celeste.LevelEnter.Go -= onLevelEnter;
            On.Celeste.LevelLoader.ctor -= onLevelLoad;
            On.Celeste.LevelExit.ctor -= onLevelExit;

            IL.Celeste.Player.UpdateHair -= onPlayerUpdateHair;

            On.Celeste.LevelEnter.Routine -= addForceSkinsDisabledPostcard;
            On.Celeste.LevelEnter.BeforeRender -= addForceSkinsDisabledPostcardRendering;

            On.Celeste.Mod.EverestModule.CreateModMenuSection -= greyOutCodeModSkinToggles;

            hookVariantMode?.Dispose();
            hookVariantMode = null;

            hookEmoteMod?.Dispose();
            hookEmoteMod = null;
        }

        private static void hookEmoteModBackpackHook(Action<On.Celeste.Player.orig_Update, Player> orig, On.Celeste.Player.orig_Update origOrig, Player self) {
            if (HasLantern(self.Sprite.Mode) || HasLantern(((PlayerSpriteMode?) playerNextSpriteMode.GetValue(self)) ?? PlayerSpriteMode.Madeline)) {
                // we don't want EmoteMod to mess with the player sprite mode, otherwise it will break the lantern entirely. :a:
                origOrig(self);
            } else {
                // don't change anything.
                orig(origOrig, self);
            }
        }

        private static void onLevelEnter(On.Celeste.LevelEnter.orig_Go orig, Session session, bool fromSaveData) {
            checkForSkinReset(session);

            orig(session, fromSaveData);
        }

        private static void onLevelLoad(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startPosition) {
            if (skinsDisabled && Engine.Scene is Level) {
                reenableSkins();
            }

            if (!skinsDisabled) {
                checkForSkinReset(session);
            }

            orig(self, session, startPosition);

            // we want Madeline's sprite to load its metadata (that is, her hair positions on her frames of animation).
            PlayerSprite.CreateFramesMetadata("junglehelper_madeline_copy");
            PlayerSprite.CreateFramesMetadata("junglehelper_player_badeline_copy");
            PlayerSprite.CreateFramesMetadata("junglehelper_player_badeline_copy_vanilla");
            PlayerSprite.CreateFramesMetadata("junglehelper_madeline_lantern");
            PlayerSprite.CreateFramesMetadata("junglehelper_badeline_lantern");
            PlayerSprite.CreateFramesMetadata("junglehelper_madeline_lantern_no_override");
            PlayerSprite.CreateFramesMetadata("junglehelper_badeline_lantern_no_override");
        }

        private static void checkForSkinReset(Session session) {
            if (!skinsDisabled && !forceMarioSkinDisabled && !forceBananaSkinDisabled && !forceKaydenSkinDisabled && !forceSkinModHelperDisabled
                && AreaData.Areas.Count > session.Area.ID && AreaData.Areas[session.Area.ID].Mode.Length > (int) session.Area.Mode
                && AreaData.Areas[session.Area.ID].Mode[(int) session.Area.Mode] != null) {

                // look for the first Enforce Skin Controller we can find.
                EntityData controllerData = null;
                foreach (LevelData levelData in session.MapData.Levels) {
                    controllerData = levelData.Entities.FirstOrDefault(entityData => entityData.Name == "JungleHelper/EnforceSkinController");
                    if (controllerData != null) {
                        // we found it! stop searching.
                        break;
                    }
                }

                // check if there is a controller, or a lantern.
                if (controllerData != null || session.MapData.Levels.Exists(levelData => levelData.Entities.Exists(entityData => entityData.Name == "JungleHelper/Lantern"))) {
                    bool isThereASkin = Everest.Content.Map.Keys.Any(asset => EnforceSkinVanillaSpriteDump.VanillaPlayerSprites.Contains(asset));
                    bool isLanternReskinned = Everest.Content.Map.Keys.Any(asset => asset.StartsWith("Graphics/Atlases/Gameplay/JungleHelper/Lantern/")
                        && Everest.Content.Map[asset].Source?.Mod != null
                        && Everest.Content.Map[asset].Source.Mod.Name != "JungleHelper");

                    if (isThereASkin && !isLanternReskinned) {
                        Logger.Log(LogLevel.Info, "JungleHelper/EnforceSkinController", "Skins are disabled from now");
                        skinsDisabled = true;
                    }

                    if (Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "MarioSkin", Version = new Version(1, 0) })) {
                        resetMarioSkin();
                    }

                    if (Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "ProBananaSkin", Version = new Version(1, 0) })) {
                        resetBananaSkin();
                    }

                    if (Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "KaydenFoxSkin", Version = new Version(1, 0) })) {
                        resetKaydenSkin();
                    }

                    if (Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "SkinModHelper", Version = new Version(0, 5) })) {
                        resetSkinModHelperSkin();
                    }

                    if (skinsDisabled || disabledMarioSkin || disabledBananaSkin || disabledKaydenSkin || disabledSkinModHelperSkin != null) {
                        showForceSkinsDisabledPostcard = controllerData == null || controllerData.Bool("showPostcard", true);
                    }
                }
            }
        }

        private static IEnumerator addForceSkinsDisabledPostcard(On.Celeste.LevelEnter.orig_Routine orig, LevelEnter self) {
            if (showForceSkinsDisabledPostcard) {
                showForceSkinsDisabledPostcard = false;

                // let's show a postcard to let the player know skins have been force disabled.
                self.Add(forceSkinsDisabledPostcard = new Postcard(Dialog.Get("JUNGLEHELPER_POSTCARD_SKINSFORCEDISABLED"),
                    "event:/ui/main/postcard_csides_in", "event:/ui/main/postcard_csides_out"));
                yield return forceSkinsDisabledPostcard.DisplayRoutine();
                forceSkinsDisabledPostcard = null;
            }

            // just go on with vanilla behavior (other postcards, B-side intro, etc)
            yield return new SwapImmediately(orig(self));
        }

        private static void addForceSkinsDisabledPostcardRendering(On.Celeste.LevelEnter.orig_BeforeRender orig, LevelEnter self) {
            orig(self);

            if (forceSkinsDisabledPostcard != null)
                forceSkinsDisabledPostcard.BeforeRender();
        }

        private static void reenableSkins() {
            if (skinsDisabled) {
                Logger.Log(LogLevel.Info, "JungleHelper/EnforceSkinController", "Skins are not disabled anymore");
                skinsDisabled = false;
            }

            if (forceMarioSkinDisabled) {
                restoreMarioSkin();
            }

            if (forceBananaSkinDisabled) {
                restoreBananaSkin();
            }

            if (disabledKaydenSkin) {
                restoreKaydenSkin();
            }

            if (disabledSkinModHelperSkin != null) {
                restoreSkinModHelperSkin();
            }

            forceMarioSkinDisabled = false;
            forceBananaSkinDisabled = false;
            forceKaydenSkinDisabled = false;
            forceSkinModHelperDisabled = false;
        }

        private static void onLevelExit(On.Celeste.LevelExit.orig_ctor orig, LevelExit self, LevelExit.Mode mode, Session session, HiresSnow snow) {
            reenableSkins();

            orig(self, mode, session, snow);
        }

        private static void onPlayerUpdateHair(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<PlayerSprite>("get_Mode"))) {
                Logger.Log("JungleHelper/EnforceSkinController", $"Fixing Madeline hair color with custom sprite modes at {cursor.Index} in IL for Player.UpdateHair");

                cursor.EmitDelegate<Func<PlayerSpriteMode, PlayerSpriteMode>>(orig => {
                    if (orig == SpriteModeBadelineNormal || orig == SpriteModeBadelineLantern) {
                        // this is a Badeline sprite mode; trick the game into thinking we're using the vanilla MadelineAsBadeline sprite mode.
                        return PlayerSpriteMode.MadelineAsBadeline;
                    }
                    return orig;
                });
            }
        }

        public static bool HasLantern(PlayerSpriteMode mode) {
            return mode == SpriteModeMadelineLantern || mode == SpriteModeBadelineLantern;
        }

        private static void onPlayerSpriteConstructor(On.Celeste.PlayerSprite.orig_ctor orig, PlayerSprite self, PlayerSpriteMode mode) {
            PlayerSpriteMode requestedMode = mode;
            if (skinsDisabled) {
                // if the given mode is a default sprite mode, use Jungle Helper's own copy of it to disable any skin.
                switch (requestedMode) {
                    case PlayerSpriteMode.Madeline:
                    case PlayerSpriteMode.MadelineNoBackpack:
                        requestedMode = SpriteModeMadelineNormal;
                        break;
                    case PlayerSpriteMode.MadelineAsBadeline:
                        requestedMode = SpriteModeBadelineNormal;
                        break;
                }
            }

            bool customSprite = (requestedMode == SpriteModeMadelineNormal || requestedMode == SpriteModeBadelineNormal ||
                requestedMode == SpriteModeMadelineLantern || requestedMode == SpriteModeBadelineLantern);

            if (customSprite) {
                // build regular Madeline with backpack as a reference.
                mode = PlayerSpriteMode.Madeline;
            }

            orig(self, mode);

            bool lookUpAnimTweak = false;

            if (customSprite) {
                switch (requestedMode) {
                    case SpriteModeMadelineNormal:
                        GFX.SpriteBank.CreateOn(self, "junglehelper_madeline_copy");
                        break;
                    case SpriteModeBadelineNormal:
                        GFX.SpriteBank.CreateOn(self, "junglehelper_player_badeline_copy");
                        break;
                    case SpriteModeMadelineLantern:
                        GFX.SpriteBank.CreateOn(self, skinsDisabled ? "junglehelper_madeline_lantern" : "junglehelper_madeline_lantern_no_override");
                        lookUpAnimTweak = true;
                        break;
                    case SpriteModeBadelineLantern:
                        GFX.SpriteBank.CreateOn(self, skinsDisabled ? "junglehelper_badeline_lantern" : "junglehelper_badeline_lantern_no_override");
                        break;
                }

                new DynData<PlayerSprite>(self)["Mode"] = requestedMode;

                // replay the "idle" sprite to make it apply immediately.
                self.Play("idle", restart: true);

                if (lookUpAnimTweak) {
                    // when the look up animation finishes, rewind it to frame 7: this way we are getting 7-11 playing in a loop.
                    self.OnFinish = anim => {
                        if (anim == "lookUp") {
                            self.Play("lookUp", restart: true);
                            self.SetAnimationFrame(5);
                        }
                    };
                }
            }
        }

        private delegate void orig_ChangePlayAsBadeline(object self, bool on);
        private static void levelChangePlayAsBadeline(orig_ChangePlayAsBadeline orig, object self, bool on) {
            Player player = Engine.Scene.Tracker.GetEntity<Player>();
            bool hasLantern = player != null && HasLantern(player.Sprite.Mode);

            orig(self, on);

            if (hasLantern) {
                // give the lantern back to the player! Messing with the Other Self variant shouldn't make them lose the lantern.
                ChangePlayerSpriteMode(player, hasLantern: true);
            }
        }

        // Skin code mod stuff

        private static void resetMarioSkin() {
            Logger.Log(LogLevel.Info, "JungleHelper/EnforceSkinController", "Mario skin is force-disabled from now");
            disabledMarioSkin = MarioSkin.MarioSkin.Settings.SkinEnabled;
            MarioSkin.MarioSkin.Settings.SkinEnabled = false;
            forceMarioSkinDisabled = true;
        }

        private static void restoreMarioSkin() {
            Logger.Log(LogLevel.Info, "JungleHelper/EnforceSkinController", $"Mario skin can be enabled again, restoring its old status {disabledMarioSkin}");

            MarioSkin.MarioSkin.Settings.SkinEnabled = disabledMarioSkin;
            disabledMarioSkin = false;
        }

        private static void resetBananaSkin() {
            Logger.Log(LogLevel.Info, "JungleHelper/EnforceSkinController", "Banana skin is force-disabled from now");
            disabledBananaSkin = ProBananaSkin.ProBananaSkinModule.Settings.Enabled;
            ProBananaSkin.ProBananaSkinModule.Settings.Enabled = false;
            forceBananaSkinDisabled = true;
        }

        private static void restoreBananaSkin() {
            Logger.Log(LogLevel.Info, "JungleHelper/EnforceSkinController", $"Banana skin can be enabled again, restoring its old status {disabledBananaSkin}");

            ProBananaSkin.ProBananaSkinModule.Settings.Enabled = disabledBananaSkin;
            disabledBananaSkin = false;
        }

        private static void resetKaydenSkin() {
            Logger.Log(LogLevel.Info, "JungleHelper/EnforceSkinController", "Kayden skin is force-disabled from now");
            disabledKaydenSkin = KaydenSpriteMod.KaydenSpriteModule.Settings.Enabled;
            KaydenSpriteMod.KaydenSpriteModule.Settings.Enabled = false;
            forceKaydenSkinDisabled = true;
        }

        private static void restoreKaydenSkin() {
            Logger.Log(LogLevel.Info, "JungleHelper/EnforceSkinController", $"Restoring old status of Kayden skin {disabledKaydenSkin}");

            KaydenSpriteMod.KaydenSpriteModule.Settings.Enabled = disabledKaydenSkin;
            disabledKaydenSkin = false;
        }

        private static void resetSkinModHelperSkin() {
            Logger.Log(LogLevel.Info, "JungleHelper/EnforceSkinController", "Skin Mod Helper is force-disabled from now");
            disabledSkinModHelperSkin = SkinModHelper.Module.SkinModHelperModule.Settings.SelectedSkinMod;
            if (disabledSkinModHelperSkin == SkinModHelper.SkinModHelperConfig.DEFAULT_SKIN) {
                disabledSkinModHelperSkin = null;
            }
            SkinModHelper.Module.SkinModHelperModule.Settings.SelectedSkinMod = SkinModHelper.SkinModHelperConfig.DEFAULT_SKIN;
            forceSkinModHelperDisabled = true;
        }

        private static void restoreSkinModHelperSkin() {
            Logger.Log(LogLevel.Info, "JungleHelper/EnforceSkinController", $"Restoring old status of Skin Mod Helper: {disabledSkinModHelperSkin}");

            SkinModHelper.Module.SkinModHelperModule.Settings.SelectedSkinMod = disabledSkinModHelperSkin;
            disabledSkinModHelperSkin = null;
        }

        private static void greyOutCodeModSkinToggles(On.Celeste.Mod.EverestModule.orig_CreateModMenuSection orig, EverestModule self, TextMenu menu, bool inGame, EventInstance snapshot) {
            orig(self, menu, inGame, snapshot);

            if (forceMarioSkinDisabled && self.GetType().FullName == "Celeste.Mod.MarioSkin.MarioSkin") {
                // disable the Mario Skin toggle.
                menu.Items[menu.Items.Count - 1].Disabled = true;
            }

            if (forceBananaSkinDisabled && self.GetType().FullName == "Celeste.Mod.ProBananaSkin.ProBananaSkinModule") {
                // disable the Banana Skin toggle.
                menu.Items[menu.Items.Count - 1].Disabled = true;
            }

            if (forceSkinModHelperDisabled && self.GetType().FullName == "SkinModHelper.Module.SkinModHelperModule") {
                // disable the Skin Mod Helper toggle.
                menu.Items[menu.Items.Count - 1].Disabled = true;
            }
        }

        public static void ChangePlayerSpriteMode(Player player, bool hasLantern) {
            PlayerSpriteMode spriteMode;
            if (hasLantern) {
                spriteMode = SaveData.Instance.Assists.PlayAsBadeline ? SpriteModeBadelineLantern : SpriteModeMadelineLantern;
            } else {
                spriteMode = SaveData.Instance.Assists.PlayAsBadeline ? PlayerSpriteMode.MadelineAsBadeline : player.DefaultSpriteMode;
            }

            if (player.Active) {
                player.ResetSpriteNextFrame(spriteMode);
            } else {
                player.ResetSprite(spriteMode);
            }
        }
    }
}
