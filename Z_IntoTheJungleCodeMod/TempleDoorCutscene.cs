using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.IntoTheJungleCodeMod {
    class TempleDoorCutscene : CutsceneEntity {
        private Player player;
        private TempleDoor door;
        private TempleDoorRocks rocks;

        public TempleDoorCutscene(Player player, TempleDoor door, TempleDoorRocks rocks) {
            this.player = player;
            this.door = door;
            this.rocks = rocks;
        }

        public override void OnBegin(Level level) {
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level) {
            // remove control from the player
            player.StateMachine.State = 11;
            player.StateMachine.Locked = true;
            player.ForceCameraUpdate = true;

            // walk to door
            yield return player.DummyWalkToExact((int) door.X);

            // wait for a bit because why not...
            yield return 0.5f;

            // zoom on Maddy (and snap the camera to be sure it is in the position we want)
            level.Camera.Position = player.CameraTarget;
            yield return level.ZoomTo(player.Position - level.Camera.Position - Vector2.UnitY * 20f, 2f, 0.5f);
            yield return 0.5f;

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
            rocks.Sprite.Play("close");
            Audio.Play("event:/junglehelper/sfx/templedoor_cutscene_slam", door.Position);

            while (door.Sprite.CurrentAnimationFrame < 14) {
                yield return null;
            }

            yield return 0.2f;

            // done!
            EndCutscene(level);
        }

        public override void OnEnd(Level level) {
            level.CompleteArea(true, false);

            if (!WasSkipped) {
                SpotlightWipe.Modifier = 70f;
                SpotlightWipe.FocusPoint = new Vector2(160f, 94f);
            }
        }
    }
}
