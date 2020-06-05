using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;

namespace Celeste.Mod.JungleHelper {
    public class BouncyShroomModule : EverestModule {
        public static BouncyShroomModule Instance;

        public BouncyShroomModule() {
            Instance = this;
        }

        public override void Load() {
            ClimbableOneWayPlatform.Load();
            TheoStatueGate.Load();
        }
        public override void Unload() {
            ClimbableOneWayPlatform.Unload();
            TheoStatueGate.Unload();
        }
    }
}