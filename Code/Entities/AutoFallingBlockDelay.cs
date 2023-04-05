
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
namespace Celeste.Mod.JungleHelper.Entities {

    [CustomEntity("JungleHelper/AutoFallingBlockDelayed")]
    public class AutoFallingBlockDelayed : Solid {
        private TileGrid tiles;

        private char TileType;

        private int originalY;
        private float delay = 2;
        private float shakeTimer;
        private bool silent = false;

        private bool manuallyTriggered = false;

        public AutoFallingBlockDelayed(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Char("tiletype", '3'), data.Width, data.Height, data.Float("delay", 2), data.Float("ShakeDelay", 0.5f), data.Bool("silent", false)) {
        }


        public AutoFallingBlockDelayed(Vector2 position, char tile, int width, int height, float delay, float shakeTimer, bool silent)
            : base(position, width, height, safe: false) {
            originalY = (int) position.Y;
            this.delay = delay;
            this.shakeTimer = shakeTimer;
            this.silent = silent;
            int newSeed = Calc.Random.Next();
            Calc.PushRandom(newSeed);
            Add(tiles = GFX.FGAutotiler.GenerateBox(tile, width / 8, height / 8).TileGrid);
            Calc.PopRandom();
            Add(new Coroutine(Sequence()));
            Add(new LightOcclude());
            Add(new TileInterceptor(tiles, highPriority: false));
            TileType = tile;
            SurfaceSoundIndex = SurfaceIndex.TileToIndex[tile];
        }

        public override void OnShake(Vector2 amount) {
            base.OnShake(amount);
            TileGrid tileGrid = tiles;
            tileGrid.Position += amount;
        }
        private void ShakeSfx() {
            if (silent) {
                return;
            }

            if (TileType == '3') {
                Audio.Play("event:/game/01_forsaken_city/fallblock_ice_shake", base.Center);
            } else if (TileType == '9') {
                Audio.Play("event:/game/03_resort/fallblock_wood_shake", base.Center);
            } else if (TileType == 'g') {
                Audio.Play("event:/game/06_reflection/fallblock_boss_shake", base.Center);
            } else {
                Audio.Play("event:/game/general/fallblock_shake", base.Center);
            }
        }
        private IEnumerator Sequence() {
            if (delay > 0) {
                if (delay > shakeTimer) {
                    float timer = delay - shakeTimer;
                    while (timer > 0f && !manuallyTriggered) {
                        yield return null;
                        timer -= Engine.DeltaTime;
                    }
                    StartShaking();
                    ShakeSfx();
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                    yield return shakeTimer;
                    StopShaking();
                } else {
                    StartShaking();
                    ShakeSfx();
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                    yield return delay;
                    StopShaking();
                }

            }
            while (true) {
                for (int i = 2; (float) i < base.Width; i += 4) {
                    if (base.Scene.CollideCheck<Solid>(base.TopLeft + new Vector2((float) i, -2f))) {
                        SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustA, 2, new Vector2(base.X + (float) i, base.Y), Vector2.One * 4f, (float) Math.PI / 2f);
                    }
                    SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustB, 2, new Vector2(base.X + (float) i, base.Y), Vector2.One * 4f);
                }
                float speed = 0f;
                float maxSpeed = 160f;
                while (true) {
                    Level level = SceneAs<Level>();
                    speed = Calc.Approach(speed, maxSpeed, 500f * Engine.DeltaTime);
                    if (MoveVCollideSolids(speed * Engine.DeltaTime, thruDashBlocks: true)) {
                        break;
                    }
                    Safe = false;
                    float top = base.Top;
                    Rectangle bounds = level.Bounds;
                    int num;
                    if (!(top > bounds.Bottom + 16)) {
                        float top2 = base.Top;
                        bounds = level.Bounds;
                        num = ((top2 > bounds.Bottom - 1) && CollideCheck<Solid>(Position + new Vector2(0f, 1f))) ? 1 : 0;
                    } else {
                        num = 1;
                    }
                    if (num != 0) {
                        Collidable = (Visible = false);
                        yield return 0.2f;
                        if (level.Session.MapData.CanTransitionTo(level, new Vector2(base.Center.X, base.Bottom + 12f))) {
                            yield return 0.2f;
                            SceneAs<Level>().Shake();
                            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                        }
                        RemoveSelf();
                        DestroyStaticMovers();
                        yield break;
                    }
                    yield return null;
                }
                Safe = true;
                if (base.Y != (float) originalY) {
                    ImpactSfx();
                    Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                    SceneAs<Level>().DirectionalShake(Vector2.UnitY);
                    StartShaking();
                    LandParticles();
                    yield return 0.2f;
                    StopShaking();
                }
                if (CollideCheck<SolidTiles>(Position + new Vector2(0f, 1f))) {
                    break;
                }
                while (CollideCheck<Platform>(Position + new Vector2(0f, 1f))) {
                    yield return 0.1f;
                }
            }
            Safe = true;
        }

        private void LandParticles() {
            for (int i = 2; (float) i <= base.Width; i += 4) {
                if (base.Scene.CollideCheck<Solid>(base.BottomLeft + new Vector2((float) i, 3f))) {
                    SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_FallDustA, 1, new Vector2(base.X + (float) i, base.Bottom), Vector2.One * 4f, -(float) Math.PI / 2f);
                    float direction = (!((float) i < base.Width / 2f)) ? 0f : ((float) Math.PI);
                    SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_LandDust, 1, new Vector2(base.X + (float) i, base.Bottom), Vector2.One * 4f, direction);
                }
            }
        }

        private void ImpactSfx() {
            if (silent) {
                return;
            }

            if (TileType == '3') {
                Audio.Play("event:/game/01_forsaken_city/fallblock_ice_impact", base.BottomCenter);
            } else if (TileType == '9') {
                Audio.Play("event:/game/03_resort/fallblock_wood_impact", base.BottomCenter);
            } else if (TileType == 'g') {
                Audio.Play("event:/game/06_reflection/fallblock_boss_impact", base.BottomCenter);
            } else {
                Audio.Play("event:/game/general/fallblock_impact", base.BottomCenter);
            }
        }

        public void ForceFall() {
            manuallyTriggered = true;
        }
    }

}