using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System.Collections.Generic;

namespace Celeste.Mod.JungleHelper.Entities {
    [CustomEntity("JungleHelper/AttachTriggerController")]
    public class AttachTriggerController : Entity {
        private string entityFilter = null;
        private string triggerFilter = null;
        private Vector2 entityPosition, triggerPosition;

        public AttachTriggerController(EntityData data, Vector2 offset) : base(data.Position + offset) {
            triggerPosition = Position;
            entityPosition = data.NodesOffset(offset)[0];

            triggerFilter = data.Attr("triggerFilter");
            entityFilter = data.Attr("entityFilter");
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            foreach (Platform platform in scene.CollideAll<Platform>(new Rectangle((int) entityPosition.X, (int) entityPosition.Y, 8, 8))) {
                if (string.IsNullOrEmpty(entityFilter) || platform.GetType().Name.ToLowerInvariant().Contains(entityFilter.ToLowerInvariant())) {
                    foreach (Trigger trigger in scene.CollideAll<Trigger>(new Rectangle((int) triggerPosition.X, (int) triggerPosition.Y, 8, 8))) {
                        if (string.IsNullOrEmpty(triggerFilter) || trigger.GetType().Name.ToLowerInvariant().Contains(triggerFilter.ToLowerInvariant())) {
                            // attach this entity to this trigger through a StaticMover.
                            List<StaticMover> staticMovers = new DynData<Platform>(platform).Get<List<StaticMover>>("staticMovers");

                            StaticMover triggerMover = new StaticMover();
                            trigger.Add(triggerMover);
                            staticMovers.Add(triggerMover);
                        }
                    }
                }
            }

            // the controller isn't needed anymore now, let's eject it.
            RemoveSelf();
        }
    }
}
