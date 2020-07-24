using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
namespace Celeste.Mod.JungleHelper {
    [CustomEntity("JungleHelper/Gecko")]
    public class Gecco : Entity {

        public Sprite Sprite;

        public Vector2 StartPosition;

        public VertexLight Light;

        public bool AutoFly;

        public EntityID EntityID;

        public bool FlyAwayUp = true;

        public float WaitForLightningPostDelay;

        public bool DisableFlapSfx;

        private Coroutine tutorialRoutine;


        private Coroutine walkRoutine;


        private BirdTutorialGui gui;

        private Level level;

        private Vector2[] nodes;

        private StaticMover staticMover;
        public float range;
        private bool onlyIfPlayerLeft;
        private bool hostile;
        private PlayerCollider pc;

        public bool moving = true;
        public float delay = 0.5f;
        public bool left = false;

        public Gecco(Vector2 position, bool hostile, bool showTutorial,bool left ,float range, float delay): base(position) {

            this.hostile = hostile;
            this.delay = delay;
            if (hostile) {
                Add(Sprite = JungleHelperModule.SpriteBank.Create("gecko_hostile"));
            } else {
                Add(Sprite = JungleHelperModule.SpriteBank.Create("gecko_normal"));
            }
            Sprite.Rotation = -1.5f;
            Sprite.UseRawDeltaTime = true;
            this.left = left;
            this.range = range;
            if (showTutorial){
                Add(Light = new VertexLight(new Vector2(0f, -8f), Color.White, 1f, 8, 32));
                Add(tutorialRoutine = new Coroutine(ClimbingTutorial()));
            }
            StartPosition = Position;
            Add(walkRoutine = new Coroutine(Movement()));
            moving = true;
            if (left) {
                Sprite.Scale.Y = -1f;
                Collider = new Hitbox(6f, 18f, -7f, -12f);
                Sprite.X = 1;
            } else {
                Collider = new Hitbox(6f, 18f, 1f, 12f);
                Sprite.X = -1;
            }
            Add(pc = new PlayerCollider(OnCollide));
        }

        public Gecco(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Bool("hostile", false), data.Bool("showTutorial", false), data.Bool("left", false), data.Float("range", 20), data.Float("delay",0.5f)) {
        }

        public void OnCollide(Player player) {
            if (hostile) {
                moving = false;
                Sprite.Play("hit");
                player.Die(new Vector2(0f, -1f));
            }
        }

        public override void Update() {
            base.Update();
        }


        public IEnumerator ShowTutorial(BirdTutorialGui gui) {
            this.gui = gui;
            gui.Open = true;
            Scene.Add(gui);
            while (gui.Scale < 1f) {
                yield return null;
            }
        }

        public IEnumerator HideTutorial() {
            if (gui != null) {
                gui.Open = false;
                while (gui.Scale > 0f) {
                    yield return null;
                }
                Scene.Remove(gui);
                gui = null;
            }
        }
        public bool CheckWall() {
            if (!left) {
                Console.WriteLine(CollideFirst<Solid>(Position + new Vector2(5f, 0)) == null);
                return CollideFirst<Solid>(Position + new Vector2(5f, 0)) == null;
            } else {
                return CollideFirst<Solid>(Position + new Vector2(-5f, 0)) == null;
            }
        }
        private IEnumerator Movement() {
            yield return 0.5f;
            Player p = Scene.Tracker.GetEntity<Player>();
            while (p != null){
                if (moving){
                    Sprite.Play("walk");
                    Sprite.Scale.X = -1;
                    Collider.CenterY = 0;
                }
                while (Position != StartPosition + new Vector2(0, -range)) {
                    yield return null;
                    Y = Calc.Approach(Y, StartPosition.Y - range, 15f * Engine.DeltaTime);
                    if (!moving || CheckWall() || CollideFirst<Solid>(Position + new Vector2(0, -15f * Engine.DeltaTime)) != null) {
                        break;
                    }
                }
                if (moving) Sprite.Play("idle");
                yield return delay;
                if (moving) {
                    Sprite.Play("walk");
                    Sprite.Scale.X = 1;
                    Collider.CenterY = 4;
                }
                while (Position != StartPosition + new Vector2(0, 2*range)) {
                    yield return null;
                    Y = Calc.Approach(Y, StartPosition.Y + 2 * range, 20f * Engine.DeltaTime);
                    if (!moving || CollideFirst<Solid>(Position + new Vector2(0, 20f * Engine.DeltaTime)) != null) {
                        break;
                    }
                }
                if (moving) Sprite.Play("idle");
                yield return delay;
                if (moving) {
                    Sprite.Play("walk");
                    Sprite.Scale.X = -1;
                    Collider.CenterY = 0;
                }
                while (Position != StartPosition) {
                    yield return null;
                    Y = Calc.Approach(Y, StartPosition.Y, 15f * Engine.DeltaTime);
                    if (!moving || CheckWall() || CollideFirst<Solid>(Position + new Vector2(0, -15f * Engine.DeltaTime)) != null) {
                        break;
                    }
                }
            }
            yield break;
        }
        private IEnumerator ClimbingTutorial() {
            yield return delay / 2;
            Player p = Scene.Tracker.GetEntity<Player>();
            while (Y > StartPosition.Y - range) {
                yield return null;
            }
            BirdTutorialGui tut = new BirdTutorialGui(this, new Vector2(0f, -16f), Dialog.Clean("tutorial_climb"), Dialog.Clean("tutorial_hold"), Input.Grab);
            yield return ShowTutorial(tut);
            moving = false;
            Sprite.Play("idle");
            do {
                yield return null;
            }
            while (p.Scene != null && p.StateMachine.State != 1 && (Math.Abs(p.X-X) != 0));
            moving = true;
            yield return HideTutorial();
        }



    }
}