using Celeste.Mod.Registry.DecalRegistryHandlers;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Xml;

namespace Celeste.Mod.JungleHelper.Components {
    public class RainbowDecalComponent : Component {
        public RainbowDecalComponent() : base(active: false, visible: false) { }

        private class DecalHandler : DecalRegistryHandler {
            public override void Parse(XmlAttributeCollection xml) { }
            public override void ApplyTo(Decal decal) => decal.Add(new RainbowDecalComponent());
            public override string Name => "jungleHelper_rainbow";
        }

        public static void Load() {
            IL.Celeste.Decal.DecalImage.Render += rainbowifyDecal;
            DecalRegistry.AddPropertyHandler<DecalHandler>();
        }

        public static void Unload() {
            IL.Celeste.Decal.DecalImage.Render -= rainbowifyDecal;
        }

        private static void rainbowifyDecal(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall<Color>("get_White"))) {
                Logger.Log("JungleHelper/RainbowDecalComponent", $"Injecting call to make decal rainbow at {cursor.Index} in IL for DecalImage.Render");
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<Color, Component, Color>>(makeDecalRainbow);
            }
        }

        private static Color makeDecalRainbow(Color orig, Component self) {
            RainbowDecalComponent component = self.Entity.Get<RainbowDecalComponent>();
            if (component != null) {
                return component.getHue(self.Entity.Position);
            }
            return orig;
        }

        // replicates vanilla rainbow spinners, and gets hooked by Helping Hand if needed... controlling both sides is handy.
        private Color getHue(Vector2 position) {
            float value = (position.Length() + Scene.TimeActive * 50f) % 280f / 280f;
            float hue = 0.4f + Calc.YoYo(value) * 0.4f;
            return Calc.HsvToColor(hue, 0.4f, 0.9f);
        }
    }
}
