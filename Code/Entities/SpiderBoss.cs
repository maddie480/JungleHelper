using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.JungleHelper.Entities {
    [CustomEntity("JungleHelper/SpiderBoss")]
    public class SpiderBoss : Entity {
        private static bool usingWatchtower;

        public static void Load() {
            On.Celeste.LevelLoader.ctor += onLevelStart;
            On.Celeste.Lookout.LookRoutine += onWatchtowerUse;
        }

        public static void Unload() {
            On.Celeste.LevelLoader.ctor -= onLevelStart;
            On.Celeste.Lookout.LookRoutine -= onWatchtowerUse;
        }

        private static void onLevelStart(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startPosition) {
            orig(self, session, startPosition);
            usingWatchtower = false;
        }

        private static IEnumerator onWatchtowerUse(On.Celeste.Lookout.orig_LookRoutine orig, Lookout self, Player player) {
            usingWatchtower = true;
            yield return new SwapImmediately(orig(self, player));
            usingWatchtower = false;
        }


        private enum SpiderColor {
            Blue, Purple, Red
        };

        private float cameraRelativeY;

        // configuration constants: timings in seconds
        private const float SLIDE_DURATION = 1f;
        private const float DELAY_BEFORE_FALL = 2f;
        private readonly float[] RESPAWN_DELAY = {
            1f, 0f, 0f
        };
        private const float RESPAWN_DELAY_AFTER_WATCHTOWER = 1f;

        // configuration constants: dimensions in px
        private const float SLIDE_DISTANCE = 24f;

        // configuration constants: speeds in px/s
        private const float FALLING_SPEED = 200f;
        private readonly float[] TRACK_PLAYER_SPEED = {
            50f, 100f, float.MaxValue
        };
        private const float WATCHTOWER_RETRACT_SPEED = 100f;

        // configuration constants: accelerations in px/s ... /s
        private const float FALLING_ACCELERATION = 400f;
        private readonly float[] TRACK_PLAYER_ACCELERATION = {
            400f, 800f, float.MaxValue
        };

        // light colors
        private readonly Color[] LIGHT_COLORS = {
            Calc.HexToColor("b3cffc"), Calc.HexToColor("eab3fc"), Calc.HexToColor("ffb6a6")
        };

        // settings
        private readonly SpiderColor color;
        private readonly string flag;

        private SoundSource sfx;
        private VertexLight light;

        // state information
        private Sprite spider;
        private Sprite web;
        private float stateDelay = 0.5f;
        private float speed = 0f;
        private bool falling = false;
        private bool justRespawned = true;

        // paired spider info (only used for red spiders)
        private SpiderBoss pairedSpider = null;
        private bool ignorePairedSpider = false;

        public SpiderBoss(EntityData data, Vector2 offset) : base(data.Position + offset) {
            color = data.Enum("color", SpiderColor.Blue);
            flag = data.Attr("flag");

            // set up the web above the spider
            web = JungleHelperModule.CreateReskinnableSprite(data.Attr("webSprite"), "spiderboss_web");
            web.Position = new Vector2(-1f, -22f);
            Add(web);

            // set up the spider sprite
            Add(spider = JungleHelperModule.CreateReskinnableSprite(data, "spider_boss_" + color.ToString().ToLowerInvariant()));

            // set up the spider hitbox
            Collider = new Hitbox(13f, 13f, -7f, -7f);
            Add(new PlayerCollider(onPlayer));

            Depth = -20001; // in front of exit blocks, FG tiles and mossy walls

            // set up the state machine.
            StateMachine stateMachine = new StateMachine();
            stateMachine.SetCallbacks(0, waitingUpdate, null, waitingBegin);
            stateMachine.SetCallbacks(1, poppingInUpdate, null, poppingInBegin);
            stateMachine.SetCallbacks(2, trackingUpdate, null, trackingBegin);
            stateMachine.SetCallbacks(3, fallingUpdate, null, fallingBegin, fallingEnd);
            stateMachine.SetCallbacks(4, watchtowerRetractUpdate);
            Add(stateMachine);

            Add(sfx = new SoundSource());
            Add(light = new VertexLight(LIGHT_COLORS[(int) color], 1f, 24, 48));

            // make the spider pop out of existence on the beginning of transitions because it looks better than it freezing into place.
            Add(new TransitionListener {
                OnOutBegin = () => Visible = false
            });
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            // one red spider appears as soon as the previous one snaps web: to do that, we are using 2 paired spiders that alternate.
            if (color == SpiderColor.Red && pairedSpider == null) {
                // create our paired spider.
                EntityData data = new EntityData();
                data.Values = new Dictionary<string, object> {
                    { "color", "Red" },
                    { "flag", flag }
                };
                pairedSpider = new SpiderBoss(data, Vector2.Zero);
                scene.Add(pairedSpider);

                // make the link between spiders bidirectional...
                pairedSpider.pairedSpider = this;

                // ... and ignore it at first (because one of the spiders in the pair has to start off if we don't want them to wait for each other forever!)
                ignorePairedSpider = true;
            }
        }

        private bool shouldPause() {
            // the spider should pause when the player is using a watchtower, or if the spider is flag-activated and the flag is not set.
            return usingWatchtower || (!string.IsNullOrEmpty(flag) && !SceneAs<Level>().Session.GetFlag(flag));
        }

        private void onPlayer(Player player) {
            if (!SaveData.Instance.Assists.Invincible) {
                // kill the player.
                if (player.Position != Position) {
                    player.Die(Vector2.Normalize(player.Position - Position));
                } else {
                    player.Die(Vector2.Zero);
                }

                // play the impact sprite
                spider.Play("impact");
            }
        }

        private void waitingBegin() {
            // spider is invisible while it's waiting.
            Visible = false;
            Collidable = false;
            light.Visible = false;
        }

        private int waitingUpdate() {
            // if delay is over, player already moved and paired spider is falling if any, switch to the Popping In state.
            if (stateDelay <= 0f && !didPlayerJustRespawn() && (pairedSpider == null || ignorePairedSpider || pairedSpider.falling)) {
                justRespawned = false;
                ignorePairedSpider = false; // from now, both spiders should be synced up.
                return 1;
            }

            // if player is using a watchtower, give them some delay before the spider shows up again (camera has to come back to them).
            if (shouldPause()) {
                stateDelay = RESPAWN_DELAY_AFTER_WATCHTOWER;
            }

            return 0;
        }

        private bool didPlayerJustRespawn() {
            if (!justRespawned) {
                // spider already moved before.
                return false;
            }
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null) {
                // check if the player is moving, which is how Player.JustRespawned works.
                return player.Speed == Vector2.Zero;
            }
            // no player - pretend they didn't move.
            return true;
        }

        private void poppingInBegin() {
            // the spider is now visible.
            Visible = true;
            light.Visible = true;
            spider.Play("idle_0");
            web.Play("idle");

            // ... but it isn't collidable yet, it will become once the animation will have made some progress.
            Collidable = false;

            // set up the spider pop position according to camera and player position.
            Position.X = Scene.Tracker.GetEntity<Player>()?.X ?? SceneAs<Level>().Camera.Left + 115f;
            cameraRelativeY = -8f;

            // duration of the state: the slide in duration.
            stateDelay = SLIDE_DURATION;

            // play the appear sound.
            sfx.Play("event:/junglehelper/sfx/SpiderBoss_Appear");
        }

        private int poppingInUpdate() {
            // if player is using a watchtower, give up and switch to the "Watchtower Retract" state.
            if (shouldPause()) {
                return 4;
            }

            // ease the spider in.
            float progress = Calc.ClampedMap(stateDelay, SLIDE_DURATION, 0);
            cameraRelativeY = MathHelper.Lerp(-8f, SLIDE_DISTANCE, Ease.SineOut(progress));

            // the spider will only be harmful after a bit.
            Collidable = (progress > 0.6f);

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
            speed = 0f;
        }

        private int trackingUpdate() {
            // if player is using a watchtower, give up and switch to the "Watchtower Retract" state.
            if (shouldPause()) {
                return 4;
            }

            float originalX = Position.X;

            // track the X position of the player.
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null) {
                if (TRACK_PLAYER_ACCELERATION[(int) color] == float.MaxValue) {
                    Position.X = player.X;
                } else {
                    speed = Calc.Approach(speed, Math.Sign(player.X - Position.X) * TRACK_PLAYER_SPEED[(int) color], TRACK_PLAYER_ACCELERATION[(int) color] * Engine.DeltaTime);
                    Position.X = approachKeepingMoveDirection(Position.X, player.X, speed * Engine.DeltaTime);
                }
            }

            // update sprite depending on speed (we can't use speed directly, because red spiders with "infinite speed" are a thing...)
            float effectiveSpeed;
            if (Engine.DeltaTime != 0f) {
                effectiveSpeed = (Position.X - originalX) / Engine.DeltaTime;
            } else {
                // time was probably frozen and divisions by 0 are bad. let's just assume effective speed is 0
                effectiveSpeed = 0f;
            }
            int sprite = Calc.Clamp((int) ((effectiveSpeed / 45f) + Math.Sign(effectiveSpeed)), -2, 2);
            spider.Play($"idle_{sprite}");

            // if delay is over, switch to the Falling state.
            if (stateDelay <= 0f) {
                return 3;
            }
            return 2;
        }

        // slight variation on Calc.Approach, but the move always happens in the direction specified by maxMove.
        private static float approachKeepingMoveDirection(float val, float target, float maxMove) {
            if (val != target) {
                return (val > target) ? Math.Max(val + maxMove, target) : Math.Min(val + maxMove, target);
            }
            return target;
        }

        private void fallingBegin() {
            falling = true;

            // web breaks!
            spider.Play("start_falling");
            web.Play("break");

            speed = 0f;

            // play the snapping sound.
            sfx.Play("event:/junglehelper/sfx/SpiderBoss_Snap");
        }

        private int fallingUpdate() {
            // spider falls down
            speed = Calc.Approach(speed, FALLING_SPEED, FALLING_ACCELERATION * Engine.DeltaTime);
            cameraRelativeY += speed * Engine.DeltaTime;

            // web has to look static.
            float initialSpiderPosition = SLIDE_DISTANCE;
            web.Position.Y = initialSpiderPosition - cameraRelativeY - 22f;

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

        private int watchtowerRetractUpdate() {
            // retract the spider until it is off-screen.
            cameraRelativeY -= WATCHTOWER_RETRACT_SPEED * Engine.DeltaTime;

            // if we're finished retracting, switch to the Waiting state.
            if (cameraRelativeY <= -8f) {
                if (pairedSpider != null) {
                    // since the current spider was getting ready to fall, it should start falling again without waiting for the paired spider...
                    // because said paired spider is waiting for the current spider to fall.
                    ignorePairedSpider = true;
                }

                return 0;
            }
            return 4;
        }

        public override void Update() {
            stateDelay -= Engine.DeltaTime;

            if (spider.CurrentAnimationID == "impact") {
                // we hit the player: freeze, only animate the sprite.
                spider.Update();
            } else {
                // update all components.
                base.Update();
            }

            Position.Y = SceneAs<Level>().Camera.Top + cameraRelativeY;
        }
    }
}
