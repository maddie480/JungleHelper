using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.IntoTheJungleCodeMod {
    class TempleDoorCutscene : CutsceneEntity {
        private Player player;
        private TempleDoor door;

        public TempleDoorCutscene(Player player, TempleDoor door) {
            this.player = player;
            this.door = door;
        }

        public override void OnBegin(Level level) {
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level) {
            // remove control from the player
            player.StateMachine.State = 11;
            player.StateMachine.Locked = true;

            // walk to door
            yield return player.DummyWalkTo(door.X);

            // wait for a bit because why not...
            yield return 1f;

            // and enter the door! this is where it gets fun.
            // let's create some sprites!
            Sprite playerEntering = IntoTheJungleCodeModule.SpriteBank.Create("maddy_entering_temple");
            Sprite hairEntering = IntoTheJungleCodeModule.SpriteBank.Create("maddy_entering_temple");
            if (player.Facing == Facings.Right) {
                playerEntering.Play("left_to_right");
                hairEntering.Play("left_to_right_hair");
            } else {
                playerEntering.Play("right_to_left");
                hairEntering.Play("right_to_left_hair");
            }

            // now we are going to... replace Maddy with those sprites.
            VertexLight light = new VertexLight(new Vector2(0f, -8f), Color.White, 1f, 32, 64);
            player.Visible = false;
            Add(light);
            Add(playerEntering);
            Add(hairEntering);

            // we want to tint the hair too (that's the whole point of having the hair separated).
            hairEntering.Color = player.Hair.Color;

            // those sprites are going to be placed relatively to the cutscene entity's position so uh...
            // we're going to move the cutscene. :theoreticalwoke:
            Position = player.Position;

            // now we just wait for the animation to finish. last frame is 9
            while (playerEntering.CurrentAnimationFrame < 9) {
                yield return null;
                light.Alpha = 0.9f - playerEntering.CurrentAnimationFrame / 10f;
            }

            yield return 0.2f;

            // door closes behind Madeline.
            door.Sprite.Play("close");

            // delay the sound by a bit.
            yield return 0.1f;
            Audio.Play("event:/game/05_mirror_temple/gate_main_close", door.Position);

            while (door.Sprite.CurrentAnimationFrame < 14) {
                yield return null;
            }

            yield return 0.2f;

            // done!
            EndCutscene(level);
        }

        public override void OnEnd(Level level) {
            level.CompleteArea(true, false);
        }
    }
}
