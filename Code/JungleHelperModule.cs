using Celeste.Mod.JungleHelper.Components;
using Celeste.Mod.JungleHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JungleHelper {
    public class JungleHelperModule : EverestModule {
        public static JungleHelperModule Instance;

        public override System.Type SessionType => typeof(JungleHelperSession);
        public static JungleHelperSession Session => (JungleHelperSession) Instance._Session;

        public JungleHelperModule() {
            Instance = this;
        }

        public override void Load() {
            Logger.SetLogLevel("JungleHelper", LogLevel.Info);
            ClimbableOneWayPlatform.Load();
            TheoStatueGate.Load();
            UnrandomizedCrumblePlatform.Load();
            Lantern.Load();
            SpiderBoss.Load();
            EnforceSkinController.Load();
            SpriteWipe.Load();
            RollingRock.Load();
            GrablessGoldenBerry.Load();
            SwingCassetteBlock.Load();
            MossyWall.Load();
            CassetteCustomPreviewMusic.Load();
            RainbowDecalComponent.Load();

            // let's tell Everest that sprite wipe textures loading on a separate thread is nothing to worry about.
            AssetReloadHelper.SilentThreadList.Add("Jungle Helper Sprite Wipe Loading Thread");
        }

        public override void Unload() {
            ClimbableOneWayPlatform.Unload();
            TheoStatueGate.Unload();
            UnrandomizedCrumblePlatform.Unload();
            Lantern.Unload();
            SpiderBoss.Unload();
            EnforceSkinController.Unload();
            SpriteWipe.Unload();
            RollingRock.Unload();
            GrablessGoldenBerry.Unload();
            SwingCassetteBlock.Unload();
            MossyWall.Unload();
            CassetteCustomPreviewMusic.Unload();
            RainbowDecalComponent.Unload();
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

        public override void LoadSession(int index, bool forceNew) {
            base.LoadSession(index, forceNew);

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
