using Celeste.Mod.JungleHelper.Components;
using Celeste.Mod.JungleHelper.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;

namespace Celeste.Mod.JungleHelper {
    public class JungleHelperModule : EverestModule {
        public static JungleHelperModule Instance;

        private static ILHook cSidesUnlockCrashFix = null;

        public override Type SessionType => typeof(JungleHelperSession);
        public static JungleHelperSession Session => (JungleHelperSession) Instance._Session;

        public JungleHelperModule() {
            Instance = this;
        }

        public override void Load() {
            Logger.SetLogLevel("JungleHelper", LogLevel.Info);
            ClimbableOneWayPlatform.Load();
            AttachedClimbableOneWayPlatform.Load();
            TheoStatueGate.Load();
            UnrandomizedCrumblePlatform.Load();
            Lantern.Load();
            SpiderBoss.Load();
            EnforceSkinController.Load();
            RollingRock.Load();
            GrablessGoldenBerry.Load();
            SwingCassetteBlock.Load();
            MossyWall.Load();
            CassetteCustomPreviewMusic.Load();
            RainbowDecalComponent.Load();
            RemoteKevin.Load();
            Hawk.Load();

            // fix this bug ourselves if the user is not on the Everest dev branch, until the fix reaches stable.
            if (!Everest.Loader.DependencyLoaded(new EverestModuleMetadata() { Name = "Everest", Version = new Version(1, 3294, 0) })) {
                cSidesUnlockCrashFix = new ILHook(typeof(LevelSetStats).GetMethod("get_MaxAreaMode"), fixCSidesUnlockCrash);
            }
        }

        public override void Unload() {
            ClimbableOneWayPlatform.Unload();
            AttachedClimbableOneWayPlatform.Unload();
            TheoStatueGate.Unload();
            UnrandomizedCrumblePlatform.Unload();
            Lantern.Unload();
            SpiderBoss.Unload();
            EnforceSkinController.Unload();
            RollingRock.Unload();
            GrablessGoldenBerry.Unload();
            SwingCassetteBlock.Unload();
            MossyWall.Unload();
            CassetteCustomPreviewMusic.Unload();
            RainbowDecalComponent.Unload();
            RemoteKevin.Unload();
            Hawk.Unload();

            cSidesUnlockCrashFix?.Dispose();
            cSidesUnlockCrashFix = null;
        }

        private void fixCSidesUnlockCrash(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldloc_S && ((VariableReference) instr.Operand).Index == 5)) {
                Logger.Log("JungleHelper/JungleHelperModule", $"Fixing crash upon unlocking C-sides at {cursor.Index} in IL for LevelSetStats.get_MaxAreaMode");

                cursor.RemoveRange(3);
                cursor.EmitDelegate<Func<ModeProperties, int>>(modeProperties => {
                    if (modeProperties == null) { // this null check is the fix.
                        return 0;
                    }
                    return (int) modeProperties.MapData.Area.Mode;
                });
            }
        }

        public override void LoadContent(bool firstLoad) {
            spriteBank = new SpriteBank(GFX.Game, "Graphics/JungleHelper/CustomSprites.xml");

            RemoteKevin.P_Red = new ParticleType {
                Source = GFX.Game["particles/rect"],
                Color = Calc.HexToColor("FF762B"),
                Color2 = Calc.HexToColor("DB552A"),
                ColorMode = ParticleType.ColorModes.Blink,
                RotationMode = ParticleType.RotationModes.SameAsDirection,
                Size = 0.5f,
                SizeRange = 0.2f,
                DirectionRange = 0.5235988f,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.5f,
                LifeMax = 1.2f,
                SpeedMin = 30f,
                SpeedMax = 50f,
                SpeedMultiplier = 0.4f,
                Acceleration = new Vector2(0f, 10f)
            };

            RemoteKevin.P_Green = new ParticleType {
                Source = GFX.Game["particles/rect"],
                Color = Calc.HexToColor("2C9C19"),
                Color2 = Calc.HexToColor("2BE839"),
                ColorMode = ParticleType.ColorModes.Blink,
                RotationMode = ParticleType.RotationModes.SameAsDirection,
                Size = 0.5f,
                SizeRange = 0.2f,
                DirectionRange = 0.5235988f,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.5f,
                LifeMax = 1.2f,
                SpeedMin = 30f,
                SpeedMax = 50f,
                SpeedMultiplier = 0.4f,
                Acceleration = new Vector2(0f, 10f)
            };
        }

        public override void PrepareMapDataProcessors(MapDataFixup context) {
            base.PrepareMapDataProcessors(context);

            context.Add<JungleHelperMapDataProcessor>();
        }

        public override void DeserializeSession(int index, byte[] data) {
            base.DeserializeSession(index, data);

            // initial value for GrablessBerryWillFlyAway is equel to GrablessBerryFlewAway.
            Session.GrablessBerryWillFlyAway = Session.GrablessBerryFlewAway;
        }

        private static SpriteBank spriteBank;

        public static Sprite CreateReskinnableSprite(EntityData data, string defaultSpriteName) {
            return CreateReskinnableSprite(data.Attr("sprite"), defaultSpriteName);
        }

        public static Sprite CreateReskinnableSprite(string reskinName, string defaultSpriteName) {
            if (string.IsNullOrEmpty(reskinName)) {
                return spriteBank.Create(defaultSpriteName);
            }
            return GFX.SpriteBank.Create(reskinName);
        }

        public static Sprite CreateReskinnableSpriteOn(Sprite sprite, string reskinName, string defaultSpriteName) {
            if (string.IsNullOrEmpty(reskinName)) {
                return spriteBank.CreateOn(sprite, defaultSpriteName);
            }
            return GFX.SpriteBank.CreateOn(sprite, reskinName);
        }
    }
}
