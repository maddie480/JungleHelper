using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.JungleHelper.Entities {
    [CustomEntity("JungleHelper/InvisibleJumpthruPlatform")]
    public class InvisibleJumpthruPlatform : JumpthruPlatform {
        public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            entityData.Values["texture"] = "JungleHelper/Invisible";
            return new InvisibleJumpthruPlatform(entityData, offset);
        }

        public InvisibleJumpthruPlatform(EntityData data, Vector2 offset) : base(data, offset) {
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            List<Component> componentsToRemove = new List<Component>();
            foreach (Component component in this) {
                if (component is Image) {
                    componentsToRemove.Add(component);
                }
            }
            foreach (Component toRemove in componentsToRemove) {
                toRemove.RemoveSelf();
            }
        }
    }
}
