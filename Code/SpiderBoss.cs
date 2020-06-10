using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.JungleHelper {
    [CustomEntity("JungleHelper/SpiderBoss")]
    class SpiderBoss : Entity {
        private enum SpiderColor {
            Blue, Purple, Red
        };

        // configuration constants: timings in seconds
        private const float SLIDE_DURATION = 1f;
        private const float DELAY_BEFORE_FALL = 2f;
        private readonly float[] RESPAWN_DELAY = {
            1f, 0f, 0f
        };

        // configuration constants: dimensions in px
        private const float SLIDE_DISTANCE = 24f;

        // configuration constants: speeds in px/s
        private const float FALLING_SPEED = 200f;
        private readonly float[] TRACK_PLAYER_SPEED = {
            50f, 100f, float.MaxValue
        };

        // configuration constants: accelerations in px/s ... /s
        private const float FALLING_ACCELERATION = 400f;

        // settings
        private readonly SpiderColor color;

        private SoundSource sfx;

        // state information
        private Sprite web;
        private float stateDelay = 0f;
        private float speed = 0f;
        private bool falling = false;

        // paired spider info (only used for red spiders)
        private SpiderBoss pairedSpider = null;
        private bool ignorePairedSpider = false;

        public SpiderBoss(EntityData data, Vector2 offset) : base(data.Position + offset) {
            color = data.Enum("color", SpiderColor.Blue);

            // set up the web above the spider
            web = JungleHelperModule.SpriteBank.Create("spiderboss_web");
            web.Position = new Vector2(-1f, -22f);
            Add(web);

            // set up the spider sprite (static image for now, might become a Sprite later)
            Image spider = new Image(GFX.Game["JungleHelper/SpiderBoss/Spider" + color.ToString()]);
            spider.CenterOrigin();
            Add(spider);

            // set up the spider hitbox
            Collider = new Hitbox(13f, 13f, -7f, -7f);
            Add(new PlayerCollider(onPlayer));

            Depth = -12500; // as deep as the Oshiro boss

            // set up the state machine.
            StateMachine stateMachine = new StateMachine();
            stateMachine.SetCallbacks(0, waitingUpdate, null, waitingBegin);
            stateMachine.SetCallbacks(1, poppingInUpdate, null, poppingInBegin);
            stateMachine.SetCallbacks(2, trackingUpdate, null, trackingBegin);
            stateMachine.SetCallbacks(3, fallingUpdate, null, fallingBegin, fallingEnd);
            Add(stateMachine);

            Add(sfx = new SoundSource());
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            // one red spider appears as soon as the previous one snaps web: to do that, we are using 2 paired spiders that alternate.
            if (color == SpiderColor.Red && pairedSpider == null) {
                // create our paired spider.
                EntityData data = new EntityData();
                data.Values = new Dictionary<string, object> {
                    { "color", "Red" }
                };
                pairedSpider = new SpiderBoss(data, Vector2.Zero);
                scene.Add(pairedSpider);

                // make the link between spiders bidirectional...
                pairedSpider.pairedSpider = this;

                // ... and ignore it at first (because one of the spiders in the pair has to start off if we don't want them to wait for each other forever!)
                ignorePairedSpider = true;
            }
        }

        private void onPlayer(Player player) {
            // kill the player.
            if (player.Position != Position) {
                player.Die(Vector2.Normalize(player.Position - Position));
            } else {
                player.Die(Vector2.Zero);
            }
        }

        private void waitingBegin() {
            // spider is invisible while it's waiting.
            Visible = false;
            Collidable = false;
        }

        private int waitingUpdate() {
            // if delay is over, player already moved and paired spider is falling if any, switch to the Popping In state.
            if (stateDelay <= 0f && !(Scene.Tracker.GetEntity<Player>()?.JustRespawned ?? true) && (pairedSpider == null || ignorePairedSpider || pairedSpider.falling)) {
                ignorePairedSpider = false; // from now, both spiders should be synced up.
                return 1;
            }
            return 0;
        }

        private void poppingInBegin() {
            // the spider is now visible.
            Visible = true;
            Collidable = true;
            web.Play("idle");

            // set up the spider pop position according to camera and player position.
            Position.X = Scene.Tracker.GetEntity<Player>()?.X ?? SceneAs<Level>().Camera.Left + 115f;
            Position.Y = SceneAs<Level>().Camera.Top - 8f;

            // duration of the state: the slide in duration.
            stateDelay = SLIDE_DURATION;

            // play the appear sound.
            sfx.Play("event:/junglehelper/sfx/SpiderBoss_Appear");
        }

        private int poppingInUpdate() {
            // ease the spider in.
            float progress = Calc.ClampedMap(stateDelay, SLIDE_DURATION, 0);
            float cameraTop = SceneAs<Level>().Camera.Top;
            Position.Y = MathHelper.Lerp(cameraTop - 8f, cameraTop + SLIDE_DISTANCE, Ease.SineOut(progress));

            // also track the player at the same time if relevant.
            trackingUpdate();

            // if delay is over, switch to the Tracking state.
            if (stateDelay <= 0f) {
                return 2;
            }
            return 1;
        }

        private void trackingBegin() {
            // duration of the state: the delay before the spider falls.
            stateDelay = DELAY_BEFORE_FALL;
        }

        private int trackingUpdate() {
            // track the X position of the player, if the option is enabled.
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null) {
                Position.X = Calc.Approach(Position.X, player.X, TRACK_PLAYER_SPEED[(int) color] * Engine.DeltaTime);
            }

            // if delay is over, switch to the Falling state.
            if (stateDelay <= 0f) {
                return 3;
            }
            return 2;
        }

        private void fallingBegin() {
            falling = true;

            // web breaks!
            web.Play("break");

            speed = 0f;

            // play the snapping sound.
            sfx.Play("event:/junglehelper/sfx/SpiderBoss_Snap");
        }

        private int fallingUpdate() {
            // spider falls down
            speed = Calc.Approach(speed, FALLING_SPEED, FALLING_ACCELERATION * Engine.DeltaTime);
            Position.Y += speed * Engine.DeltaTime;

            // web has to look static.
            float initialSpiderPosition = SceneAs<Level>().Camera.Top + SLIDE_DISTANCE;
            web.Position.Y = initialSpiderPosition - Position.Y - 22f;

            // if spider is off-screen, switch to the Waiting state.
            if (Position.Y > SceneAs<Level>().Camera.Bottom + 10f) {
                stateDelay = RESPAWN_DELAY[(int) color];
                return 0;
            }
            return 3;
        }

        private void fallingEnd() {
            falling = false;

            // restore web position
            web.Position.Y = -22f;
        }

        public override void Update() {
            stateDelay -= Engine.DeltaTime;
            base.Update();
        }
    }
}
