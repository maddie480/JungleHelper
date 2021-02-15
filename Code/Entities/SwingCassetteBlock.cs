using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Reflection;

namespace Celeste.Mod.JungleHelper.Entities {
    [CustomEntity("JungleHelper/SwingCassetteBlock")]
    [Tracked]
    [TrackedAs(typeof(CassetteBlock))]
    class SwingCassetteBlock : CassetteBlock {
        public static void Load() {
            IL.Celeste.CassetteBlockManager.AdvanceMusic += swingCassetteMusic;
        }

        public static void Unload() {
            IL.Celeste.CassetteBlockManager.AdvanceMusic -= swingCassetteMusic;
        }

        private static void swingCassetteMusic(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(0.166666672f))) {
                Logger.Log("JungleHelper/SwingCassetteBlock", $"Changing cassette block tempo at {cursor.Index} in IL for CassetteBlockManager.AdvanceMusic");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, typeof(CassetteBlockManager).GetField("beatIndex", BindingFlags.NonPublic | BindingFlags.Instance));
                cursor.EmitDelegate<Func<float, CassetteBlockManager, int, float>>((orig, self, beatIndex) => {
                    // don't change anything unless there's a swing cassette block in the room.
                    if (self.Scene?.Tracker.CountEntities<SwingCassetteBlock>() == 0) {
                        return orig;
                    }

                    // if there is... it's time to swing!
                    if (beatIndex % 2 == 0) {
                        return orig * 1.32f;
                    } else {
                        return orig * 0.68f;
                    }
                });
            }
        }

        public SwingCassetteBlock(EntityData data, Vector2 offset, EntityID id) : base(data, offset, id) { }
    }
}
