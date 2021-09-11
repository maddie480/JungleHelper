using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.JungleHelper.Entities {
    [Tracked]
    [TrackedAs(typeof(CassetteBlock))]
    public class SwingCassetteBlock : CassetteBlock {
        private static ILHook loadLevelILHook;
        public static bool IsSwingCassetteBlock = false;

        public static void Load() {
            loadLevelILHook = new ILHook(typeof(Level).GetMethod("orig_LoadLevel"), hookLoadLevel);
            IL.Celeste.CassetteBlockManager.AdvanceMusic += swingCassetteMusic;

            // don't print out a warning about swing cassette block "failing to load" since they spawn in a weird way (hijacking vanilla cassette block spawn code).
            ((HashSet<string>) typeof(Level).GetField("_LoadStrings", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null)).Add("JungleHelper/SwingCassetteBlock");
        }

        public static void Unload() {
            loadLevelILHook?.Dispose();
            loadLevelILHook = null;

            IL.Celeste.CassetteBlockManager.AdvanceMusic -= swingCassetteMusic;
        }

        private static void hookLoadLevel(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<EntityData>("Name"))) {
                Logger.Log("JungleHelper/SwingCassetteBlock", $"Modding orig_LoadLevel at {cursor.Index} to recognize Swing Cassette Blocks");

                // JungleHelper/SwingCassetteBlock should be replaced with cassetteBlock to get the same behavior as cassette blocks.
                cursor.EmitDelegate<Func<string, string>>(orig => {
                    IsSwingCassetteBlock = (orig == "JungleHelper/SwingCassetteBlock");
                    if (IsSwingCassetteBlock) {
                        return "cassetteBlock";
                    }
                    return orig;
                });
            }

            if (cursor.TryGotoNext(instr => instr.MatchNewobj<CassetteBlock>())) {
                Logger.Log("JungleHelper/SwingCassetteBlock", $"Modding orig_LoadLevel at {cursor.Index} to instantiate Swing Cassette Blocks");

                cursor.Emit(OpCodes.Ldsfld, typeof(SwingCassetteBlock).GetField("IsSwingCassetteBlock"));
                cursor.Emit(OpCodes.Brfalse, cursor.Next); // jump over the instruction we are going to insert.
                cursor.Emit(OpCodes.Newobj, typeof(SwingCassetteBlock).GetConstructor(new Type[] { typeof(EntityData), typeof(Vector2), typeof(EntityID) }));
                cursor.Emit(OpCodes.Br, cursor.Next.Next); // jump over the instruction building a vanilla cassette block.
            }
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
