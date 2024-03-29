using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.JungleHelper.Entities {
    [CustomEntity("JungleHelper/Cobweb")]
    public class Cobweb : Entity {
        private Sprite sprite;
        private Player stuckPlayer;
        private SoundSource sfx;

        public Cobweb(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Collider = new Circle(10f);
            Add(sprite = JungleHelperModule.CreateReskinnableSprite(data, "cobweb"));
            Add(new PlayerCollider(catchPlayer));
            Add(sfx = new SoundSource());
        }

        private void catchPlayer(Player player) {
            // player gets stuck in cobweb
            stuckPlayer = player;
            Collidable = false;

            // make player invisible, take control away from them
            player.StateMachine.State = 11;
            player.ForceCameraUpdate = true;
            player.Visible = false;
            player.DummyGravity = false;
            sprite.Play("catch");

            sfx.Play("event:/junglehelper/sfx/Cobweb_Entry");

            // stick the player in the middle of the cobweb
            player.Center = Position;
            roundUpPlayerPosition(player);
            player.Speed = Vector2.Zero;

            // ensure the player has at least 1 dash to dash out of the cobweb.
            player.Dashes = Math.Max(player.Dashes, 1);

            if (EnforceSkinController.HasLantern(player.Sprite.Mode)) {
                // drop the lantern to avoid a softlock.
                Lantern.DropLantern(player, destroy: false);
            }
        }

        public override void Update() {
            base.Update();

            if (stuckPlayer != null) {
                // make sure the player is still stuck in the middle of the cobweb.
                stuckPlayer.Center = Position;
                roundUpPlayerPosition(stuckPlayer);
                stuckPlayer.Speed = Vector2.Zero;

                if (stuckPlayer.CanDash) {
                    // player dashes out! make them visible again and throw them in the Dash state.
                    stuckPlayer.StateMachine.State = stuckPlayer.StartDash();
                    stuckPlayer.ForceCameraUpdate = false;
                    stuckPlayer.Visible = true;
                    stuckPlayer.DummyGravity = true;

                    // player is not in the bubble anymore.
                    stuckPlayer = null;

                    // and the cobweb vanishes.
                    sprite.Play("break");
                    sprite.OnFinish = _ => RemoveSelf();

                    sfx.Play("event:/junglehelper/sfx/Cobweb_Snap");
                }
            }
        }

        // keeping player.Position an integer is very important, otherwise weirdness ensues with jumpthrus notably.
        private static void roundUpPlayerPosition(Player player) {
            player.Position = new Vector2((int) player.Position.X, (int) player.Position.Y);
        }
    }
}
