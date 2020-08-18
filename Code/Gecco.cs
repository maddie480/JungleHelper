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
using MonoMod.Utils;
using System.Reflection;

namespace Celeste.Mod.JungleHelper {
    [CustomEntity("JungleHelper/Gecko")]
    [Tracked(true)]
    public class Gecco : Entity {

        public Sprite Sprite;

        public Vector2 StartPosition;

        public string geckoId;

        private bool onlyOnce;


        private bool triggered;

        private bool flewAway;

        private BirdTutorialGui gui;

        private static Dictionary<string, Vector2> directions = new Dictionary<string, Vector2>
        {
        {
            "Left",
            new Vector2(-1f, 0f)
        },
        {
            "Right",
            new Vector2(1f, 0f)
        },
        {
            "Up",
            new Vector2(0f, -1f)
        },
        {
            "Down",
            new Vector2(0f, 1f)
        },
        {
            "UpLeft",
            new Vector2(-1f, -1f)
        },
        {
            "UpRight",
            new Vector2(1f, -1f)
        },
        {
            "DownLeft",
            new Vector2(-1f, 1f)
        },
        {
            "DownRight",
            new Vector2(1f, 1f)
        }
    };
        public VertexLight Light;

        public bool AutoFly;

        public EntityID EntityID;

        public bool FlyAwayUp = true;

        public float WaitForLightningPostDelay;

        public bool DisableFlapSfx;

        private Coroutine tutorialRoutine;

        private Coroutine walkRoutine;

        private Level level;

        private Vector2 node;

        private StaticMover staticMover;
        public float range;
        private bool onlyIfPlayerLeft;
        private bool hostile;
        private PlayerCollider pc;

        public bool moving = true;
        public float delay = 0.5f;
        public string info;
        public string controls;
        public bool left = false;

        public Gecco(Vector2 position, string geckoId, bool onlyOnce, string info, string controls, bool hostile, bool showTutorial, bool left, Vector2 node, float delay) : base(position) {
            this.node = node;

            this.geckoId = geckoId;
            this.onlyOnce = onlyOnce;
            this.info = info;
            this.controls = controls;
            this.hostile = hostile;
            this.delay = delay;
            if (hostile) {
                Add(Sprite = JungleHelperModule.SpriteBank.Create("gecko_hostile"));
            } else {
                Add(Sprite = JungleHelperModule.SpriteBank.Create("gecko_normal"));
            }
            Sprite.Rotation = -1.5f;
            Sprite.UseRawDeltaTime = true;
            Sprite.Position.Y = -4f;
            this.left = left;
            if (showTutorial) {
                Add(Light = new VertexLight(new Vector2(0f, -8f), Color.White, 1f, 8, 32));
            }
            StartPosition = Position;
            Add(walkRoutine = new Coroutine(Movement()));
            moving = true;
            if (left) {
                Sprite.Scale.Y = -1f;
                Collider = new Hitbox(6f, 18f, -7f, -8f);
                Sprite.X = 1;
            } else {
                Collider = new Hitbox(6f, 18f, 1f, 16f);
                Sprite.X = -1;
            }
            Collider.CenterY = -3;
            Add(pc = new PlayerCollider(OnCollide));
        }

        public Gecco(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Attr("geckoId"), data.Bool("onlyOnce"), data.Attr("info"), data.Attr("controls"), data.Bool("hostile", false), data.Bool("showTutorial", false), data.Bool("left", false), data.Nodes[0] + offset, data.Float("delay", 0.5f)) {
            if (data.Bool("showTutorial")) {
                geckoId = data.Attr("geckoId");
                onlyOnce = data.Bool("onlyOnce");
                string text = data.Attr("info");
                object info = (!GFX.Gui.Has(text)) ? ((object) Dialog.Clean(text)) : ((object) GFX.Gui[text]);
                int num = 0;
                string[] array = data.Attr("controls").Split(',');
                object[] array2 = new object[array.Length];
                for (int i = 0; i < array2.Length; i++) {
                    string text2 = array[i];
                    if (GFX.Gui.Has(text2)) {
                        array2[i] = GFX.Gui[text2];
                        continue;
                    }
                    if (directions.ContainsKey(text2)) {
                        array2[i] = directions[text2];
                        continue;
                    }
                    FieldInfo field = typeof(Input).GetField(text2, BindingFlags.Static | BindingFlags.Public);
                    if (field?.GetValue(null)?.GetType() == typeof(VirtualButton)) {
                        array2[i] = field.GetValue(null);
                        continue;
                    }
                    num++;
                    if (i == 0) {
                        num++;
                    }
                    if (text2.StartsWith("dialog:")) {
                        array2[i] = Dialog.Clean(text2.Substring("dialog:".Length));
                    } else {
                        array2[i] = text2;
                    }
                }
                if (string.IsNullOrEmpty(text)) {
                    gui = new BirdTutorialGui(this, new Vector2(0f, -16f), Dialog.Clean("tutorial_climb"), Dialog.Clean("tutorial_hold"), Input.Grab);
                } else {
                    gui = new BirdTutorialGui(this, new Vector2(0f, -16f), info, array2);
                }
                DynData<BirdTutorialGui> dynData = new DynData<BirdTutorialGui>(gui);
                if (string.IsNullOrEmpty(text)) {
                    dynData["infoHeight"] = 0f;
                }
                dynData["controlsWidth"] = dynData.Get<float>("controlsWidth") + (float) num;
            }
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
            moving = false;
            while (gui.Scale < 1f) {
                Idle();
                Sprite.Scale.X = -1;
                yield return null;
            }
        }
        public void Idle() {
            Random random = new Random();
            random.Range(0, 100);
            Console.WriteLine(random.NextFloat());
            if (random.NextFloat() > 0.03 && random.NextFloat() < 0.04)
                Sprite.Play("dance");
            else
                Sprite.Play("idle");
        }

        public IEnumerator HideTutorial() {
            if (gui != null) {
                gui.Open = false;
                while (gui.Scale > 0f) {
                    yield return null;
                }
                Scene.Remove(gui);
                moving = true;
                gui = null;
            }
        }
        private IEnumerator Movement() {
            yield return 0.5f;
            Player p = Scene.Tracker.GetEntity<Player>();
            while (p != null) {
                if (moving) {
                    Sprite.Play("walk");
                    Sprite.Scale.X = Math.Sign(node.Y - Position.Y);
                }
                while (Position != node) {
                    yield return null;
                    Position = Calc.Approach(Position, node, 20f * Engine.DeltaTime);
                    if (!moving || CollideCheck<Solid>(Position + new Vector2(0, Math.Sign(node.Y - Position.Y)))) {
                        break;
                    }
                }
                if (moving)
                    Idle();
                yield return delay;
                if (moving) {
                    Sprite.Play("walk");
                    Sprite.Scale.X = Math.Sign(StartPosition.Y - Position.Y);
                }
                while (Position != StartPosition) {
                    yield return null;
                    Position = Calc.Approach(Position, StartPosition, 20f * Engine.DeltaTime);
                    if (!moving || CollideCheck<Solid>(Position + new Vector2(0, Math.Sign(StartPosition.Y - Position.Y)))) {
                        break;
                    }
                }
                if (moving)
                    Idle();
                yield return delay;
            }
            yield break;
        }

        public void TriggerShowTutorial() {
            if (!triggered) {
                triggered = true;
                Add(new Coroutine(ShowTutorial(gui)));
            }
        }

        public void TriggerHideTutorial() {
            if (!flewAway) {
                flewAway = true;
                if (triggered) {
                    Add(new Coroutine(HideTutorial()));
                }
                triggered = true;
            }
        }

        public static Gecco FindById(Level level, string geckoId) {
            return (from gex in level.Tracker.GetEntities<Gecco>().OfType<Gecco>()
                    where gex.geckoId == geckoId
                    select gex).FirstOrDefault();
        }



    }
}