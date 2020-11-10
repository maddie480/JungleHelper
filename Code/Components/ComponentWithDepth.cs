using Monocle;
using MonoMod.Utils;
using System.Collections.Generic;

namespace Celeste.Mod.JungleHelper.Components {
    /// <summary>
    /// A component with customizable depth. Useful to display a visual component with a depth different from the entity it is attached to.
    /// </summary>
    /// <typeparam name="T">The type of the component</typeparam>
    class ComponentWithDepth<T> : Component where T : Component {
        private class ComponentEntity : Entity {
            public T Component;
            public ComponentEntity(T component) {
                AddTag(Tags.Persistent); // this entity should only be discarded when the parent entity is.
                Add(Component = component);
            }
        }

        public int Depth = 0;
        public T Component => componentEntity.Component;

        private ComponentEntity componentEntity;

        public ComponentWithDepth(T component) : base(true, false) {
            componentEntity = new ComponentEntity(component);
        }

        // adds a component to the component entity
        public void Add(Component component) {
            componentEntity.Add(component);
        }

        public override void Added(Entity entity) {
            // one entity at a time please!
            Entity?.Remove(this);

            base.Added(entity);

            if (entity.Scene != null) {
                // in case the component is added before the entity gets added to the scene.
                addEntityToScene(entity.Scene);
            }
        }

        public override void EntityAdded(Scene scene) {
            base.EntityAdded(scene);

            // in case the component is added after the entity gets added to the scene.
            addEntityToScene(scene);
        }

        private void addEntityToScene(Scene scene) {
            // add entity to scene
            scene.Add(componentEntity);

            // ensure it's not about to be removed with a quality reflection hack
            new DynData<EntityList>(scene.Entities).Get<List<Entity>>("toRemove").Remove(componentEntity);
            new DynData<EntityList>(scene.Entities).Get<HashSet<Entity>>("removing").Remove(componentEntity);

            // run Update() once so that the position is correct
            Update();
        }

        public override void Removed(Entity entity) {
            base.Removed(entity);
            componentEntity.RemoveSelf();
        }

        public override void EntityRemoved(Scene scene) {
            base.EntityRemoved(scene);
            componentEntity.RemoveSelf();
        }

        public override void Update() {
            base.Update();

            componentEntity.Depth = Depth;
            componentEntity.Position = Entity.Position;
            componentEntity.Visible = Entity.Visible;
        }
    }
}
