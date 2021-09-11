using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JungleHelper.Entities {
    [CustomEntity("JungleHelper/Torch")]
    public class Torch : Entity {
        private readonly string flag;
        private Sprite sprite;

        public Torch(EntityData data, Vector2 offset) : base(data.Position + offset) {
            flag = data.Attr("flag");

            Add(sprite = JungleHelperModule.CreateReskinnableSprite(data, "torch"));
            Add(new PlayerCollider(onPlayer));

            Collider = new Hitbox(17f, 22f, -9f, -22f);
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            if (SceneAs<Level>().Session.GetFlag(flag)) {
                // the torch was already lit earlier in session! light it.
                lightTorch();
            }
        }

        private void onPlayer(Player player) {
            // light the torch and play the lighting sound.
            lightTorch();
            Add(new SoundSource(new Vector2(-1f, -18f), "event:/game/05_mirror_temple/torch_activate") { RemoveOnOneshotEnd = true });

            // set the session flag tied to this torch.
            SceneAs<Level>().Session.SetFlag(flag);
        }

        private void lightTorch() {
            // light the torch and add some special effects to it.
            sprite.Play("on");
            Add(new SoundSource(new Vector2(-1f, -18f), "event:/game/05_mirror_temple/mainmirror_torch_loop"));
            Add(new VertexLight(new Vector2(-1f, -18f), Color.White, 1f, 40, 64));
            Add(new BloomPoint(new Vector2(-1f, -18f), 0.5f, 16f));

            // the player cannot interact with a lit torch.
            Collidable = false;
        }
    }
}
