using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Linq;

namespace Celeste.Mod.JungleHelper.Entities {
    // mostly a refill copypaste, also looks like a refill for now, waiting for the sprites.
    [CustomEntity("JungleHelper/RemoteKevinRefill")]
    public class RemoteKevinRefill : Entity {

        private static ParticleType P_SlideRefillGlow;
        private static ParticleType P_SlideRefillRegen;
        private static ParticleType P_SlideRefillShatter;

        private Sprite sprite;
        private Sprite flash;
        private Image outline;

        private Wiggler wiggler;

        private BloomPoint bloom;
        private VertexLight light;

        private Level level;

        private SineWave sine;

        private bool oneUse;

        private float respawnTimer;

        private float fade = 1f;

        public RemoteKevinRefill(Vector2 position, bool oneUse, string spriteName, string flashSpriteName) : base(position) {
            Collider = new Hitbox(16f, 16f, -8f, -8f);

            Add(new PlayerCollider(OnPlayer));

            this.oneUse = oneUse;

            Add(outline = new Image(GFX.Game["JungleHelper/SlideBlockRefill/outline"]));
            outline.CenterOrigin();
            outline.Visible = false;
            Add(sprite = JungleHelperModule.CreateReskinnableSprite(spriteName, "slide_block_refill"));
            Add(flash = JungleHelperModule.CreateReskinnableSprite(flashSpriteName, "slide_block_refill_flash"));
            flash.OnFinish = delegate {
                flash.Visible = false;
            };

            Add(wiggler = Wiggler.Create(1f, 4f, delegate (float v) {
                sprite.Scale = (flash.Scale = Vector2.One * (1f + v * 0.2f));
            }));

            Add(new MirrorReflection());
            Add(bloom = new BloomPoint(0.8f, 16f));
            Add(light = new VertexLight(Color.White, 1f, 16, 48));
            Add(sine = new SineWave(0.6f, 0f));
            sine.Randomize();

            UpdateY();

            Depth = -100;

            if (P_SlideRefillGlow == null) {
                P_SlideRefillGlow = new ParticleType(Refill.P_Glow) {
                    Color = Calc.HexToColor("C1734F"),
                    Color2 = Calc.HexToColor("960463"),
                };
                P_SlideRefillRegen = new ParticleType(Refill.P_Regen) {
                    Color = Calc.HexToColor("C1734F"),
                    Color2 = Calc.HexToColor("960463"),
                };
                P_SlideRefillShatter = new ParticleType(Refill.P_Shatter) {
                    Color = Calc.HexToColor("C1734F"),
                    Color2 = Calc.HexToColor("960463"),
                };
            }
        }

        public RemoteKevinRefill(EntityData data, Vector2 offset) : this(data.Position + offset, data.Bool("oneUse"), data.Attr("sprite"), data.Attr("flashSprite")) { }

        public override void Added(Scene scene) {
            base.Added(scene);
            level = SceneAs<Level>();
        }

        public override void Update() {
            base.Update();
            if (respawnTimer > 0f) {
                respawnTimer -= Engine.DeltaTime;
            }

            if (respawnTimer <= 0f && !Collidable) {
                Respawn();
            }

            if (Collidable && Scene.OnInterval(0.1f) && fade == 1f) {
                level.ParticlesFG.Emit(P_SlideRefillGlow, 1, Position, Vector2.One * 5f);
            }

            UpdateY();

            light.Alpha = Calc.Approach(light.Alpha, sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
            bloom.Alpha = light.Alpha * 0.8f;

            if (Scene.OnInterval(2f) && sprite.Visible) {
                flash.Play("flash", restart: true);
                flash.Visible = true;
            }

            // usable = there is a Kevin that needs refilling.
            bool usable = Scene.Tracker.GetEntities<RemoteKevin>().OfType<RemoteKevin>().Any(kevin => !kevin.Refilled);

            // update the fade to match whether the refill is usable or not.
            fade = Calc.Approach(fade, usable ? 1f : 0.5f, 2f * Engine.DeltaTime);
            sprite.Color = Color.White * fade;
            flash.Color = Color.White * fade;
        }

        private void Respawn() {
            if (!Collidable) {
                Collidable = true;
                sprite.Visible = true;
                outline.Visible = false;
                Depth = -100;
                wiggler.Start();
                Audio.Play("event:/game/general/diamond_return", Position);
                level.ParticlesFG.Emit(P_SlideRefillRegen, 16, Position, Vector2.One * 2f);
            }
        }

        private void UpdateY() {
            flash.Y = sprite.Y = bloom.Y = sine.Value * 2f;
        }

        public override void Render() {
            if (sprite.Visible) {
                sprite.DrawOutline(Color.Black * Calc.ClampedMap(fade, 0.5f, 1f, 0.25f, 1f));
            }
            base.Render();
        }

        private void OnPlayer(Player player) {
            bool refilledAtLeastOne = false;
            foreach (RemoteKevin kevin in Scene.Tracker.GetEntities<RemoteKevin>()) {
                refilledAtLeastOne = kevin.Refill() || refilledAtLeastOne;
            }
            if (refilledAtLeastOne) {
                Audio.Play("event:/junglehelper/sfx/SlideRefill_touch", Position);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Collidable = false;
                Add(new Coroutine(RefillRoutine(player.Speed)));
                respawnTimer = 2.5f;
            }
        }

        private IEnumerator RefillRoutine(Vector2 entitySpeed) {
            Celeste.Freeze(0.05f);

            yield return null;

            level.Shake();
            sprite.Visible = (flash.Visible = false);
            if (!oneUse) {
                outline.Visible = true;
            }

            Depth = 8999;
            yield return 0.05f;

            float angle = entitySpeed.Angle();
            level.ParticlesFG.Emit(P_SlideRefillShatter, 5, Position, Vector2.One * 4f, angle - (float) Math.PI / 2f);
            level.ParticlesFG.Emit(P_SlideRefillShatter, 5, Position, Vector2.One * 4f, angle + (float) Math.PI / 2f);
            SlashFx.Burst(Position, angle);
            if (oneUse) {
                RemoveSelf();
            }
        }
    }
}
