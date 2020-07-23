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
        public bool left = false;

        public Gecco(Vector2 position, bool hostile, bool showTutorial,bool left ,float range): base(position) {
            Add(Sprite = GFX.SpriteBank.Create("bird"));
            Sprite.UseRawDeltaTime = true;
            Collider = new Hitbox(12f, 20f, -6f, -16f);
            this.hostile = hostile;
            this.left = left;
            this.range = range;
            Add(pc = new PlayerCollider(OnCollide));
            if (showTutorial){
                Add(Light = new VertexLight(new Vector2(0f, -8f), Color.White, 1f, 8, 32));
                Add(tutorialRoutine = new Coroutine(ClimbingTutorial()));
            }
            StartPosition = Position;
            Add(walkRoutine = new Coroutine(Movement()));
            moving = true;
            if (left) {
                Sprite.Scale.X = -1f;
            }
        }

        public Gecco(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Bool("hostile", false), data.Bool("showTutorial", false), data.Bool("left", false), data.Float("range", 20)) {
        }

        public void OnCollide(Player player) {
            if (hostile) {
                player.Die(new Vector2(0f, -1f));
                moving = false;
            }
        }
        //public override void Added(Scene scene) {
        //    base.Added(scene);
        //}

        //public override void Awake(Scene scene) {
        //    base.Awake(scene);
        //}

        //public override bool IsRiding(Solid solid) {
        //    return base.Scene.CollideCheck(new Rectangle((int) base.X, (int) base.Y - 4, 8, 2), solid);
        //}

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
        public void CreateSlideP() {
            Vector2 center = base.Center;
            if (!left) {
                center += new Vector2(5f, 4f);
            } else {
                center += new Vector2(-5f, 4f);
            }
            Dust.Burst(center, -(float) Math.PI / 2f, 1, ParticleTypes.Dust);
        }
        private IEnumerator Movement() {
            yield return 0.5f;
            Player p = Scene.Tracker.GetEntity<Player>();
            while (p != null){
                while (Position != StartPosition + new Vector2(0, -range)) {
                    yield return null;
                    Y = Calc.Approach(Y, StartPosition.Y - range, 15f * Engine.DeltaTime);
                    if (!moving || CheckWall() || CollideFirst<Solid>(Position + new Vector2(0, -15f * Engine.DeltaTime)) != null) {
                        break;
                    }
                }
                yield return 0.5f;
                while (Position != StartPosition + new Vector2(0, 2*range)) {
                    yield return null;
                    if (Engine.FrameCounter % 2 == 0) {
                        CreateSlideP();
                    }
                    Y = Calc.Approach(Y, StartPosition.Y + 2 * range, 20f * Engine.DeltaTime);
                    if (!moving || CollideFirst<Solid>(Position + new Vector2(0, 20f * Engine.DeltaTime)) != null) {
                        break;
                    }
                    if (CheckWall()) {
                        Add(new Coroutine(FallRoutine()));
                        yield break;
                    }
                }
                yield return 0.5f;
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
        private IEnumerator FallRoutine() {
            yield return 0.5f;
            float speed = 0f;
            while (true) {
                speed = Calc.Approach(speed, 30f, 8f * Engine.DeltaTime);
                Y += 30f * Engine.DeltaTime;
                if (CheckWall())
                    break;
            }
            StartPosition = Position + new Vector2 (0, -4f);
            Add(new Coroutine(Movement()));
            yield break;
        }
        private IEnumerator ClimbingTutorial() {
            yield return 0.25f;
            Player p = Scene.Tracker.GetEntity<Player>();
            while (Math.Abs(p.X - X) > 60f) {
                yield return null;
            }
            BirdTutorialGui tut = new BirdTutorialGui(this, new Vector2(0f, -16f), Dialog.Clean("tutorial_climb"), Dialog.Clean("tutorial_hold"), Input.Grab);
            yield return ShowTutorial(tut);
            do {
                yield return null;
            }
            while (p.Scene != null && p.StateMachine.State != 1);
            yield return HideTutorial();
        }



    }
}