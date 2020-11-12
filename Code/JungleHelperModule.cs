using Celeste.Mod.JungleHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JungleHelper {
    public class JungleHelperModule : EverestModule {
        public static JungleHelperModule Instance;

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
        }

        public override void LoadContent(bool firstLoad) {
            SpriteBank = new SpriteBank(GFX.Game, "Graphics/JungleHelper/CustomSprites.xml");

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

        public static SpriteBank SpriteBank;
    }
}
