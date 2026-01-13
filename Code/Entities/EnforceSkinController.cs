using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System.Reflection;
using Monocle;
using System;
using Microsoft.Xna.Framework;
using System.Linq;
using MonoMod.Cil;
using System.Collections;
using Mono.Cecil;
using System.Collections.Generic;
using System.Xml;
using Mono.Cecil.Cil;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.JungleHelper.Entities {
    public static class EnforceSkinController {
        private const PlayerSpriteMode SpriteModeMadelineLantern = (PlayerSpriteMode) 444482;
        private const PlayerSpriteMode SpriteModeBadelineLantern = (PlayerSpriteMode) 444483;

        private static Hook hookVariantMode;

        private static bool showSkinWarningPostcard = false;
        private static Postcard skinWarningPostcard;

        private static string actualSpriteId = null;
        private static HashSet<string> overwrittenSpriteBanks = new HashSet<string>();
        private static bool isSpriteBankOverwritingSkinActive = false;

        public static void Load() {
            On.Celeste.PlayerSprite.ctor += onPlayerSpriteConstructor;
            On.Celeste.Player.ctor += onPlayerConstructor;

            On.Celeste.LevelEnter.Go += onLevelEnter;
            On.Celeste.LevelLoader.ctor += onLevelLoad;

            IL.Celeste.Player.UpdateHair += patchSpriteModeChecks;
            IL.Celeste.Player.DashUpdate += patchSpriteModeChecks;
            IL.Celeste.Player.GetTrailColor += patchSpriteModeChecks;

            On.Celeste.LevelEnter.Routine += addSkinWarningPostcard;
            On.Celeste.LevelEnter.BeforeRender += addSkinWarningPostcardRendering;

            IL.Monocle.SpriteBank.GetSpriteBankExcludingVanillaCopyPastes += detectSpriteBankOverwrites;
            On.Celeste.GameLoader._GetNextScene += commitSpriteBankOverwrites;

            // the method called when changing the "Other Self" variant is a method defined inside Level.VariantMode(). patching it requires a bit of _fun_
            hookVariantMode = new Hook(findOutVariantModeType().GetMethod("<VariantMode>b__9", BindingFlags.NonPublic | BindingFlags.Instance),
                typeof(EnforceSkinController).GetMethod("levelChangePlayAsBadeline", BindingFlags.NonPublic | BindingFlags.Static));

            // don't print out a warning about enforce skin controller "failing to load" since it acts with a hook looking for it.
            ((HashSet<string>) typeof(Level).GetField("_LoadStrings", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null)).Add("JungleHelper/EnforceSkinController");

            using (new DetourConfigContext(new DetourConfig("JungleHelper_BeforeAll").WithPriority(int.MinValue)).Use()) {
                On.Monocle.SpriteBank.CreateOn += onSpriteBankCreateOn;
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
            On.Celeste.Player.ctor -= onPlayerConstructor;

            On.Celeste.LevelEnter.Go -= onLevelEnter;
            On.Celeste.LevelLoader.ctor -= onLevelLoad;

            IL.Celeste.Player.UpdateHair -= patchSpriteModeChecks;
            IL.Celeste.Player.DashUpdate -= patchSpriteModeChecks;
            IL.Celeste.Player.GetTrailColor -= patchSpriteModeChecks;

            On.Celeste.LevelEnter.Routine -= addSkinWarningPostcard;
            On.Celeste.LevelEnter.BeforeRender -= addSkinWarningPostcardRendering;

            IL.Monocle.SpriteBank.GetSpriteBankExcludingVanillaCopyPastes -= detectSpriteBankOverwrites;
            On.Celeste.GameLoader._GetNextScene -= commitSpriteBankOverwrites;

            hookVariantMode?.Dispose();
            hookVariantMode = null;

            On.Monocle.SpriteBank.CreateOn -= onSpriteBankCreateOn;
        }

        private static void onLevelEnter(On.Celeste.LevelEnter.orig_Go orig, Session session, bool fromSaveData) {
            checkForSkins(session);

            orig(session, fromSaveData);
        }

        private static void onLevelLoad(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startPosition) {
            orig(self, session, startPosition);

            // we want Madeline's sprite to load its metadata (that is, her hair positions on her frames of animation).
            PlayerSprite.CreateFramesMetadata("junglehelper_madeline_lantern");
            PlayerSprite.CreateFramesMetadata("junglehelper_badeline_lantern");
        }

        private static void checkForSkins(Session session) {
            if (AreaData.Areas.Count > session.Area.ID && AreaData.Areas[session.Area.ID].Mode.Length > (int) session.Area.Mode
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

                // check if a controller was found in the level.
                if (controllerData != null) {
                    if (isSpriteBankOverwritingSkinActive) {
                        showSkinWarningPostcard = true;
                        return;
                    }

                    // is there a skin that overwrites Maddy's sprites, but does not overwrite lantern sprites?
                    bool isThereATextureOverwritingSkin = Everest.Content.Map.Keys.Any(asset => {
                        bool result = EnforceSkinVanillaSpriteDump.VanillaPlayerSprites.Contains(asset);
                        if (result) {
                            Logger.Log("JungleHelper/EnforceSkinController", $"Vanilla texture {asset} has been detected to be overwritten!");
                        }
                        return result;
                    });
                    bool isLanternOverwrittenAsWell = Everest.Content.Map.Keys.Any(asset => {
                        bool result = asset.StartsWith("Graphics/Atlases/Gameplay/JungleHelper/Lantern/")
                            && Everest.Content.Map[asset].Source?.Mod != null
                            && Everest.Content.Map[asset].Source.Mod.Name != "JungleHelper";

                        if (result) {
                            Logger.Log("JungleHelper/EnforceSkinController", $"Lantern texture {asset} has been detected to be overwritten.");
                        }
                        return result;
                    });

                    if (isThereATextureOverwritingSkin && !isLanternOverwrittenAsWell) {
                        showSkinWarningPostcard = true;
                        return;
                    }

                    // is there a skin that redefines player or player_no_backpack, but not junglehelper_madeline_lantern or junglehelper_badeline_lantern?
                    bool isMaddyXMLOverwritten = getActualAppliedSpriteXML(PlayerSpriteMode.Madeline) != "player"
                        || getActualAppliedSpriteXML(PlayerSpriteMode.MadelineNoBackpack) != "player_no_backpack"
                        || getActualAppliedSpriteXML(PlayerSpriteMode.MadelineAsBadeline) != "player_badeline";
                    bool isMaddyLanternXMLOverwritten = getActualAppliedSpriteXML(SpriteModeMadelineLantern) != "junglehelper_madeline_lantern"
                        || getActualAppliedSpriteXML(SpriteModeBadelineLantern) != "junglehelper_badeline_lantern";

                    if (isMaddyXMLOverwritten && !isMaddyLanternXMLOverwritten) {
                        showSkinWarningPostcard = true;
                    }
                }
            }
        }

        private static void detectSpriteBankOverwrites(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(instr => instr.MatchLdstr("Sprite \""))
                && cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<XmlNode>("get_Name"))) {

                Logger.Log("JungleHelper/EnforceSkinController", $"Injecting check for overwritten sprites at {cursor.Index} in IL for SpriteBank.GetSpriteBankExcludingVanillaCopyPastes");
                cursor.Emit(OpCodes.Dup);
                cursor.EmitDelegate<Action<string>>(addOverwrittenSpriteBank);
            }
        }

        private static void addOverwrittenSpriteBank(string s) {
            overwrittenSpriteBanks.Add(s);
        }

        private static Scene commitSpriteBankOverwrites(On.Celeste.GameLoader.orig__GetNextScene orig, Overworld.StartMode startMode, HiresSnow snow) {
            bool hasMaddySprites = overwrittenSpriteBanks.Contains("player") || overwrittenSpriteBanks.Contains("player_no_backpack") || overwrittenSpriteBanks.Contains("player_badeline");
            bool hasLanternSprites = overwrittenSpriteBanks.Contains("junglehelper_madeline_lantern") || overwrittenSpriteBanks.Contains("junglehelper_badeline_lantern");

            if (hasMaddySprites && !hasLanternSprites) {
                Logger.Log("JungleHelper/EnforceSkinController", $"Overwritten sprite banks {string.Join(", ", overwrittenSpriteBanks)} contain Madeline sprites but no lantern sprites!");
                isSpriteBankOverwritingSkinActive = true;
            }

            return orig(startMode, snow);
        }

        private static Sprite onSpriteBankCreateOn(On.Monocle.SpriteBank.orig_CreateOn orig, SpriteBank self, Sprite sprite, string id) {
            // this is just a hook as close to the vanilla method as possible, to check if any other mod changed the sprite ID along the way.
            actualSpriteId = id;
            return orig(self, sprite, id);
        }

        private static string getActualAppliedSpriteXML(PlayerSpriteMode mode) {
            new PlayerSprite(mode);
            Logger.Log("JungleHelper/EnforceSkinController", $"Actual sprite applied when applying sprite mode {mode} is {actualSpriteId}");
            return actualSpriteId;
        }

        private static IEnumerator addSkinWarningPostcard(On.Celeste.LevelEnter.orig_Routine orig, LevelEnter self) {
            if (showSkinWarningPostcard) {
                showSkinWarningPostcard = false;

                // let's show a postcard to let the player know they should disable their skins.
                self.Add(skinWarningPostcard = new Postcard(Dialog.Get("JUNGLEHELPER_POSTCARD_SKINWARNING"),
                    "event:/ui/main/postcard_csides_in", "event:/ui/main/postcard_csides_out"));
                yield return skinWarningPostcard.DisplayRoutine();
                skinWarningPostcard = null;
            }

            // just go on with vanilla behavior (other postcards, B-side intro, etc)
            yield return new SwapImmediately(orig(self));
        }

        private static void addSkinWarningPostcardRendering(On.Celeste.LevelEnter.orig_BeforeRender orig, LevelEnter self) {
            orig(self);

            if (skinWarningPostcard != null)
                skinWarningPostcard.BeforeRender();
        }

        private static void patchSpriteModeChecks(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<PlayerSprite>("get_Mode"))) {
                Logger.Log("JungleHelper/EnforceSkinController", $"Fixing Madeline hair color with custom sprite modes at {cursor.Index} in IL for Player.{il.Method.Name}");

                cursor.EmitDelegate<Func<PlayerSpriteMode, PlayerSpriteMode>>(masqueradeAsBadeline);
            }
        }

        private static PlayerSpriteMode masqueradeAsBadeline(PlayerSpriteMode orig) {
            if (orig == SpriteModeBadelineLantern) {
                // this is a Badeline sprite mode; trick the game into thinking we're using the vanilla MadelineAsBadeline sprite mode.
                return PlayerSpriteMode.MadelineAsBadeline;
            }
            return orig;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool HasLantern(PlayerSpriteMode mode) {
            return mode == SpriteModeMadelineLantern || mode == SpriteModeBadelineLantern;
        }

        private static void onPlayerSpriteConstructor(On.Celeste.PlayerSprite.orig_ctor orig, PlayerSprite self, PlayerSpriteMode mode) {
            PlayerSpriteMode requestedMode = mode < 0 ? (mode + (1 << 31)) : mode;
            bool customSprite = requestedMode == SpriteModeMadelineLantern || requestedMode == SpriteModeBadelineLantern;

            if (customSprite) {
                // build regular Madeline with backpack as a reference.
                mode = PlayerSpriteMode.Madeline;
            }

            orig(self, mode);

            bool lookUpAnimTweak = false;

            if (customSprite) {
                switch (requestedMode) {
                    case SpriteModeMadelineLantern:
                        GFX.SpriteBank.CreateOn(self, "junglehelper_madeline_lantern");
                        lookUpAnimTweak = true;
                        break;
                    case SpriteModeBadelineLantern:
                        GFX.SpriteBank.CreateOn(self, "junglehelper_badeline_lantern");
                        lookUpAnimTweak = true;
                        break;
                }

                self.Mode = requestedMode;

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

        private static void onPlayerConstructor(On.Celeste.Player.orig_ctor orig, Player self, Vector2 pos, PlayerSpriteMode mode) {
            orig(self, pos, mode);

            // cancel out the idle animations that are forced by vanilla's Sprite.OnLastFrame.
            self.Sprite.OnLastFrame += anim => {
                if (HasLantern(self.Sprite.Mode) && self.Sprite.LastAnimationID.StartsWith("idle") && self.Sprite.LastAnimationID != "idle_carry") {
                    self.Sprite.Play("idle");
                }
            };
        }
    }
}
