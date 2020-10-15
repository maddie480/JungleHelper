using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System.Reflection;
using Monocle;
using System;
using Microsoft.Xna.Framework;
using System.Linq;
using MonoMod.Cil;

namespace Celeste.Mod.JungleHelper.Entities {
    static class EnforceSkinController {
        private const PlayerSpriteMode SpriteModeMadelineNormal = (PlayerSpriteMode) 444480;
        private const PlayerSpriteMode SpriteModeBadelineNormal = (PlayerSpriteMode) 444481;
        private const PlayerSpriteMode SpriteModeMadelineLantern = (PlayerSpriteMode) 444482;
        private const PlayerSpriteMode SpriteModeBadelineLantern = (PlayerSpriteMode) 444483;

        private static Hook hookVariantMode;

        private static bool disabledMarioSkin = false;
        private static bool skinsDisabled = false;

        public static void Load() {
            On.Celeste.PlayerSprite.ctor += onPlayerSpriteConstructor;

            On.Celeste.LevelEnter.Go += onLevelEnter;
            On.Celeste.LevelLoader.ctor += onLevelLoad;
            On.Celeste.LevelExit.ctor += onLevelExit;

            IL.Celeste.Player.UpdateHair += onPlayerUpdateHair;

            // the method called when changing the "Other Self" variant is a method defined inside Level.VariantMode(). patching it requires a bit of _fun_
            hookVariantMode = new Hook(typeof(Level).GetNestedType("<>c__DisplayClass151_0", BindingFlags.NonPublic).GetMethod("<VariantMode>b__9", BindingFlags.NonPublic | BindingFlags.Instance),
                typeof(EnforceSkinController).GetMethod("levelChangePlayAsBadeline", BindingFlags.NonPublic | BindingFlags.Static));
        }

        public static void Unload() {
            On.Celeste.PlayerSprite.ctor -= onPlayerSpriteConstructor;

            On.Celeste.LevelEnter.Go -= onLevelEnter;
            On.Celeste.LevelLoader.ctor -= onLevelLoad;
            On.Celeste.LevelExit.ctor -= onLevelExit;

            IL.Celeste.Player.UpdateHair -= onPlayerUpdateHair;

            hookVariantMode?.Dispose();
            hookVariantMode = null;
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
            PlayerSprite.CreateFramesMetadata("junglehelper_madeline_lantern");
            PlayerSprite.CreateFramesMetadata("junglehelper_badeline_lantern");
        }

        private static void checkForSkinReset(Session session) {
            if (!skinsDisabled && AreaData.Areas.Count > session.Area.ID && AreaData.Areas[session.Area.ID].Mode.Length > (int) session.Area.Mode) {
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
                    Logger.Log(LogLevel.Info, "JungleHelper/EnforceSkinController", "Skins are disabled from now");
                    skinsDisabled = true;

                    // TODO Mario skin
                    // TODO postcard
                }
            }
        }

        private static void reenableSkins() {
            if (skinsDisabled) {
                Logger.Log(LogLevel.Info, "JungleHelper/EnforceSkinController", "Skins are not disabled anymore");
                skinsDisabled = false;

                // TODO Mario skin
            }
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

            if (customSprite) {
                switch (requestedMode) {
                    case SpriteModeMadelineNormal:
                        GFX.SpriteBank.CreateOn(self, "junglehelper_madeline_copy");
                        break;
                    case SpriteModeBadelineNormal:
                        GFX.SpriteBank.CreateOn(self, "junglehelper_player_badeline_copy");
                        break;
                    case SpriteModeMadelineLantern:
                        GFX.SpriteBank.CreateOn(self, "junglehelper_madeline_lantern");
                        break;
                    case SpriteModeBadelineLantern:
                        GFX.SpriteBank.CreateOn(self, "junglehelper_badeline_lantern");
                        break;
                }

                new DynData<PlayerSprite>(self)["Mode"] = requestedMode;

                // replay the "idle" sprite to make it apply immediately.
                self.Play("idle", restart: true);
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

        public static void ChangePlayerSpriteMode(Player player, bool hasLantern) {
            PlayerSpriteMode spriteMode;
            if (hasLantern) {
                spriteMode = SaveData.Instance.Assists.PlayAsBadeline ? SpriteModeBadelineLantern : SpriteModeMadelineLantern;
            } else {
                spriteMode = SaveData.Instance.Assists.PlayAsBadeline ? SpriteModeBadelineNormal : SpriteModeMadelineNormal;
            }

            if (player.Active) {
                player.ResetSpriteNextFrame(spriteMode);
            } else {
                player.ResetSprite(spriteMode);
            }
        }
    }
}
