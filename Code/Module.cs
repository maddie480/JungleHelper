using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Monocle;

namespace Celeste.Mod.JungleHelper {
    public class JungleHelperModule : EverestModule {
        public static JungleHelperModule Instance;

        public JungleHelperModule() {
            Instance = this;
        }
        public override void LoadContent(bool firstLoad) {
            SpriteBank = new SpriteBank(GFX.Game, "Graphics/JungleHelper/CustomSprites.xml");
        }
        public override void Load() {
            ClimbableOneWayPlatform.Load();
        }
        public override void Unload() {
            ClimbableOneWayPlatform.Unload();
        }
        public static SpriteBank SpriteBank;
    }
}
