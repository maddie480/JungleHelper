using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.JungleHelper.Entities {
    // mostly a refill copypaste, also looks like a refill for now, waiting for the sprites.
    [CustomEntity("JungleHelper/RemoteKevinRefill")]
    class RemoteKevinRefill : Entity {

        private Sprite sprite;
        private Sprite flash;
        private Image outline;

        private Wiggler wiggler;

        private BloomPoint bloom;
        private VertexLight light;

        private Level level;

        private SineWave sine;

        private bool oneUse;
        private bool usedBySlideBlock;

        private float respawnTimer;

        public RemoteKevinRefill(Vector2 position, bool oneUse, bool usedByPlayer, bool usedBySlideBlock)
            : base(position) {

            Collider = new Hitbox(16f, 16f, -8f, -8f);

            if (usedByPlayer) {
                Add(new PlayerCollider(OnPlayer));
            }

            this.oneUse = oneUse;
            this.usedBySlideBlock = usedBySlideBlock;

            Add(outline = new Image(GFX.Game["JungleHelper/SlideBlockRefill/outline"]));
            outline.CenterOrigin();
            outline.Visible = false;
            Add(sprite = new Sprite(GFX.Game, "JungleHelper/SlideBlockRefill/idle"));
            sprite.AddLoop("idle", "", 0.1f);
            sprite.Play("idle");
            sprite.CenterOrigin();
            Add(flash = new Sprite(GFX.Game, "JungleHelper/SlideBlockRefill/flash"));
            flash.Add("flash", "", 0.05f);
            flash.OnFinish = delegate {
                flash.Visible = false;
            };
            flash.CenterOrigin();
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
        }

        public RemoteKevinRefill(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Bool("oneUse"), data.Bool("usedByPlayer"), data.Bool("usedBySlideBlock")) {
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            level = SceneAs<Level>();
        }

        public override void Update() {
            base.Update();
            if (respawnTimer > 0f) {
                respawnTimer -= Engine.DeltaTime;
            }

            RemoteKevin hitSlideBlock;
            if (Collidable && usedBySlideBlock && (hitSlideBlock = CollideFirst<RemoteKevin>()) != null) {
                if (hitSlideBlock.Refill()) {
                    Audio.Play("event:/game/general/diamond_touch", Position);
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                    Collidable = false;
                    Add(new Coroutine(RefillRoutine(hitSlideBlock.Speed)));
                    respawnTimer = 2.5f;
                }
            }

            if (respawnTimer <= 0f && !Collidable && (!usedBySlideBlock || !CollideCheck<RemoteKevin>())) {
                Respawn();
            }

            if (Collidable && Scene.OnInterval(0.1f)) {
                level.ParticlesFG.Emit(Refill.P_Glow, 1, Position, Vector2.One * 5f);
            }

            UpdateY();

            light.Alpha = Calc.Approach(light.Alpha, sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
            bloom.Alpha = light.Alpha * 0.8f;

            if (Scene.OnInterval(2f) && sprite.Visible) {
                flash.Play("flash", restart: true);
                flash.Visible = true;
            }
        }

        private void Respawn() {
            if (!Collidable) {
                Collidable = true;
                sprite.Visible = true;
                outline.Visible = false;
                Depth = -100;
                wiggler.Start();
                Audio.Play("event:/game/general/diamond_return", Position);
                level.ParticlesFG.Emit(Refill.P_Regen, 16, Position, Vector2.One * 2f);
            }
        }

        private void UpdateY() {
            flash.Y = sprite.Y = bloom.Y = sine.Value * 2f;
        }

        public override void Render() {
            if (sprite.Visible) {
                sprite.DrawOutline();
            }
            base.Render();
        }

        private void OnPlayer(Player player) {
            bool refilledAtLeastOne = false;
            foreach (RemoteKevin kevin in Scene.Tracker.GetEntities<RemoteKevin>()) {
                refilledAtLeastOne = kevin.Refill() || refilledAtLeastOne;
            }
            if (refilledAtLeastOne) {
                Audio.Play("event:/game/general/diamond_touch", Position);
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
            level.ParticlesFG.Emit(Refill.P_Shatter, 5, Position, Vector2.One * 4f, angle - (float) Math.PI / 2f);
            level.ParticlesFG.Emit(Refill.P_Shatter, 5, Position, Vector2.One * 4f, angle + (float) Math.PI / 2f);
            SlashFx.Burst(Position, angle);
            if (oneUse) {
                RemoveSelf();
            }
        }
    }
}
