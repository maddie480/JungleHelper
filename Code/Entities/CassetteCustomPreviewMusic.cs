using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Reflection;

namespace Celeste.Mod.JungleHelper.Entities {
    [CustomEntity("JungleHelper/CassetteCustomPreviewMusic")]
    public class CassetteCustomPreviewMusic : Cassette {
        private static ILHook hookOnCollectRoutine;

        public static void Load() {
            hookOnCollectRoutine = new ILHook(typeof(Cassette).GetMethod("CollectRoutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), modCollectRoutine);
        }

        public static void Unload() {
            hookOnCollectRoutine?.Dispose();
            hookOnCollectRoutine = null;
        }

        private static void modCollectRoutine(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            modCassetteParam(cursor, cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr("event:/game/general/cassette_preview")),
                custom => custom.musicEvent, "music event");

            modCassetteParam(cursor, cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr("remix")),
                custom => custom.musicParamName, "music param name");

            modCassetteParam(cursor, cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<AreaKey>("ID"), instr => instr.MatchConvR4()),
                custom => custom.musicParamValue, "music param value");
        }

        private static void modCassetteParam<T>(ILCursor cursor, bool tryGotoNextResult, Func<CassetteCustomPreviewMusic, T> paramGetter, string log) {
            if (!tryGotoNextResult) {
                // no match found: don't do anything
                return;
            }

            // praise coroutines :maddyS:
            FieldInfo thisInCoroutine = typeof(Cassette).GetMethod("CollectRoutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget()
                .DeclaringType.GetField("<>4__this");

            Logger.Log("JungleHelper/CassetteCustomPreviewMusic", $"Changing cassette {log} at {cursor.Index} in Cassette.CollectRoutine");
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, thisInCoroutine);
            cursor.EmitDelegate<Func<T, Cassette, T>>((orig, self) => {
                if (self is CassetteCustomPreviewMusic custom) {
                    return paramGetter(custom);
                }
                return orig;
            });
        }

        private readonly string musicEvent;
        private readonly string musicParamName;
        private readonly float musicParamValue;

        public CassetteCustomPreviewMusic(EntityData data, Vector2 offset) : base(data, offset) {
            musicEvent = data.Attr("musicEvent", "event:/game/general/cassette_preview");
            musicParamName = data.Attr("musicParamName", "remix");
            musicParamValue = data.Float("musicParamValue", 1f);
        }
    }
}
