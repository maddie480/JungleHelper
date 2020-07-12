using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;

namespace Celeste.Mod.JungleHelper {
    // IT MAY BE A "SLIDE BLOCK" OFFICIALLY, BUT IT WILL ALWAYS BE A REMOTE KEVIN IN MY HEART
    [CustomEntity("JungleHelper/RemoteKevin")]
    public class RemoteKevin : Solid {
        public RemoteKevin(Vector2 position, float width, float height, bool restrained) : base(position, width, height, false) {
            this.restrained = restrained;
            texture = (restrained ? "objects/slideBlock/green" : "objects/slideBlock/red");
            fill = Calc.HexToColor("8A9C60");

            idleImages = new List<Image>();
            activeTopImages = new List<Image>();
            activeRightImages = new List<Image>();
            activeLeftImages = new List<Image>();
            activeBottomImages = new List<Image>();

            Add(new DashListener {
                OnDash = new Action<Vector2>(OnDash)
            });

            attackCoroutine = new Coroutine(true);
            attackCoroutine.RemoveOnComplete = false;
            Add(attackCoroutine);

            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(texture + "/block");
            MTexture idle = atlasSubtextures[3];

            Add(face = JungleHelperModule.SpriteBank.Create("slideblock_face"));
            face.Position = new Vector2(Width, Height) / 2f;
            face.Play("idle", false, false);
            face.OnLastFrame = animation => {
                if (animation == "hit") {
                    face.Play(nextFaceDirection, false, false);
                }
            };

            int right = (int) (Width / 8f) - 1;
            int bottom = (int) (Height / 8f) - 1;
            addImage(idle, 0, 0, 0, 0, -1, -1);
            addImage(idle, right, 0, 3, 0, 1, -1);
            addImage(idle, 0, bottom, 0, 3, -1, 1);
            addImage(idle, right, bottom, 3, 3, 1, 1);
            for (int i = 1; i < right; i++) {
                addImage(idle, i, 0, Calc.Random.Choose(1, 2), 0, 0, -1);
                addImage(idle, i, bottom, Calc.Random.Choose(1, 2), 3, 0, 1);
            }
            for (int j = 1; j < bottom; j++) {
                addImage(idle, 0, j, 0, Calc.Random.Choose(1, 2), -1, 0);
                addImage(idle, right, j, 3, Calc.Random.Choose(1, 2), 1, 0);
            }

            Add(new LightOcclude(0.2f));
            Add(new WaterInteraction(() => crushDir != Vector2.Zero));
        }

        public RemoteKevin(EntityData data, Vector2 offset) : this(data.Position + offset, data.Width, data.Height, data.Bool("restrained", false)) { }

        public override void Added(Scene scene) {
            base.Added(scene);
            level = SceneAs<Level>();
        }

        public override void Update() {
            base.Update();

            if (crushDir == Vector2.Zero) {
                face.Position = new Vector2(Width, Height) / 2f;
                if (CollideCheck<Player>(Position + new Vector2(-1f, 0f))) {
                    face.X -= 1f;
                } else {
                    if (CollideCheck<Player>(Position + new Vector2(1f, 0f))) {
                        face.X += 1f;
                    } else {
                        if (CollideCheck<Player>(Position + new Vector2(0f, -1f))) {
                            face.Y -= 1f;
                        }
                    }
                }
            }

            if (currentMoveLoopSfx != null) {
                currentMoveLoopSfx.Param("submerged", (Submerged ? 1 : 0));
            }
        }

        public override void Render() {
            Vector2 bakPosition = Position;
            Position += Shake;
            Draw.Rect(X + 2f, Y + 2f, Width - 4f, Height - 4f, fill);
            base.Render();
            Position = bakPosition;
        }

        private bool Submerged {
            get {
                return Scene.CollideCheck<Water>(new Rectangle((int) (Center.X - 4f), (int) Center.Y, 8, 4));
            }
        }

        public void OnDash(Vector2 direction) {
            // if one of the directions is zero and the other isn't, this is a straight (non diagonal) dash, so we should trigger the Kevin.
            if ((direction.X == 0) != (direction.Y == 0)) {
                attack(direction);
            }
        }

        private void addImage(MTexture idle, int x, int y, int tx, int ty, int borderX = 0, int borderY = 0) {
            MTexture idleTexturePiece = idle.GetSubtexture(tx * 8, ty * 8, 8, 8, null);
            Vector2 vector = new Vector2(x * 8, y * 8);

            if (borderX != 0) {
                Add(new Image(idleTexturePiece) {
                    Color = Color.Black,
                    Position = vector + new Vector2(borderX, 0f)
                });
            }

            if (borderY != 0) {
                Add(new Image(idleTexturePiece) {
                    Color = Color.Black,
                    Position = vector + new Vector2(0f, borderY)
                });
            }

            Image idleTexturePieceImage = new Image(idleTexturePiece);
            idleTexturePieceImage.Position = vector;
            Add(idleTexturePieceImage);
            idleImages.Add(idleTexturePieceImage);

            if (borderX != 0 || borderY != 0) {
                if (borderX < 0) {
                    Image litLeftTexturePiece = new Image(GFX.Game[texture + "/lit_left"].GetSubtexture(0, ty * 8, 8, 8, null));
                    activeLeftImages.Add(litLeftTexturePiece);
                    litLeftTexturePiece.Position = vector;
                    litLeftTexturePiece.Visible = false;
                    Add(litLeftTexturePiece);
                } else if (borderX > 0) {
                    Image litRightTexturePiece = new Image(GFX.Game[texture + "/lit_right"].GetSubtexture(0, ty * 8, 8, 8, null));
                    activeRightImages.Add(litRightTexturePiece);
                    litRightTexturePiece.Position = vector;
                    litRightTexturePiece.Visible = false;
                    Add(litRightTexturePiece);
                }

                if (borderY < 0) {
                    Image litTopTexturePiece = new Image(GFX.Game[texture + "/lit_top"].GetSubtexture(tx * 8, 0, 8, 8, null));
                    activeTopImages.Add(litTopTexturePiece);
                    litTopTexturePiece.Position = vector;
                    litTopTexturePiece.Visible = false;
                    Add(litTopTexturePiece);
                } else if (borderY > 0) {
                    Image litBottomTexturePiece = new Image(GFX.Game[texture + "/lit_bottom"].GetSubtexture(tx * 8, 0, 8, 8, null));
                    activeBottomImages.Add(litBottomTexturePiece);
                    litBottomTexturePiece.Position = vector;
                    litBottomTexturePiece.Visible = false;
                    Add(litBottomTexturePiece);
                }
            }
        }

        private void turnOffImages() {
            foreach (Image image in activeLeftImages) {
                image.Visible = false;
            }
            foreach (Image image in activeRightImages) {
                image.Visible = false;
            }
            foreach (Image image in activeTopImages) {
                image.Visible = false;
            }
            foreach (Image image in activeBottomImages) {
                image.Visible = false;
            }
        }

        private void attack(Vector2 direction) {
            if (!isHit) {
                Audio.Play("event:/game/05_mirror_temple/swapblock_move", Center);
                if (currentMoveLoopSfx != null) {
                    currentMoveLoopSfx.Param("end", 1f);
                    SoundSource sfx = currentMoveLoopSfx;
                    sfx.RemoveSelf();
                }
                Add(currentMoveLoopSfx = new SoundSource());
                currentMoveLoopSfx.Position = new Vector2(Width, Height) / 2f;
                currentMoveLoopSfx.Play("event:/junglehelper/sfx/Slide_block", null, 0f);

                crushDir = direction;

                attackCoroutine.Replace(attackSequence());

                face.Play("hit", false, false);

                ClearRemainder();
                turnOffImages();

                if (crushDir.X < 0f) {
                    foreach (Image image in activeLeftImages) {
                        image.Visible = true;
                    }
                    nextFaceDirection = "left";
                } else if (crushDir.X > 0f) {
                    foreach (Image image2 in activeRightImages) {
                        image2.Visible = true;
                    }
                    nextFaceDirection = "right";
                } else if (crushDir.Y < 0f) {
                    foreach (Image image3 in activeTopImages) {
                        image3.Visible = true;
                    }
                    nextFaceDirection = "up";
                } else if (crushDir.Y > 0f) {
                    foreach (Image image4 in activeBottomImages) {
                        image4.Visible = true;
                    }
                    nextFaceDirection = "down";
                }
            }
        }

        private IEnumerator attackSequence() {
            isHit = true;
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            StopPlayerRunIntoAnimation = false;
            float speed = 0f;
            float distance = (crushDir.X != 0 ? Width : Height);
            Vector2 startPoint = Position;
            while (true) {
                speed = Calc.Approach(speed, CrushSpeed, CrushAccel * Engine.DeltaTime); // was speed, 240f, 500f

                float moveAmount = speed * Engine.DeltaTime;
                if (restrained) {
                    moveAmount = Math.Min(moveAmount, distance);
                    distance -= moveAmount;
                }

                // move and break dash blocks if the Kevin moved from its start position.
                bool hit;
                if (crushDir.X != 0f) {
                    hit = moveHCheck(moveAmount * crushDir.X, Position != startPoint);
                } else {
                    hit = MoveVCheck(moveAmount * crushDir.Y, Position != startPoint);
                }

                if (hit || (restrained && distance <= 0f)) {
                    if (!hit) {
                        Vector2 initialPos = Position;

                        // this is a restrained block. pretend we are moving 1 more pixel because we want to break dash blocks that are just ahead
                        // and make the wall hit sound...
                        if (crushDir.X != 0f) {
                            hit = moveHCheck(crushDir.X, Position != startPoint);
                        } else {
                            hit = MoveVCheck(crushDir.Y, Position != startPoint);
                        }

                        // ... but don't actually move.
                        MoveTo(initialPos);
                    }

                    if (hit) {
                        if (Position != startPoint) {
                            Audio.Play("event:/game/06_reflection/crushblock_impact", Center);
                        }
                        level.DirectionalShake(crushDir, 0.3f);
                    }
                    break;
                }

                if (Scene.OnInterval(0.02f)) {
                    Vector2 at;
                    float dir;
                    if (crushDir == Vector2.UnitX) {
                        at = new Vector2(Left + 1f, Calc.Random.Range(Top + 3f, Bottom - 3f));
                        dir = 3.14159274f;
                    } else if (crushDir == -Vector2.UnitX) {
                        at = new Vector2(Right - 1f, Calc.Random.Range(Top + 3f, Bottom - 3f));
                        dir = 0f;
                    } else if (crushDir == Vector2.UnitY) {
                        at = new Vector2(Calc.Random.Range(Left + 3f, Right - 3f), Top + 1f);
                        dir = -1.57079637f;
                    } else {
                        at = new Vector2(Calc.Random.Range(Left + 3f, Right - 3f), Bottom - 1f);
                        dir = 1.57079637f;
                    }
                    ParticleType particlesCrushing = (restrained ? P_Green : P_Red);
                    level.Particles.Emit(particlesCrushing, at, dir);
                }
                yield return null;
            }

            if (Position != startPoint) {
                // trigger falling blocks.
                FallingBlock fallingBlock = CollideFirst<FallingBlock>(Position + crushDir);
                if (fallingBlock != null) {
                    fallingBlock.Triggered = true;
                }
            }

            if (crushDir == -Vector2.UnitX) {
                Vector2 add = new Vector2(0f, 2f);
                for (int i = 0; i < Height / 8f; i++) {
                    Vector2 at = new Vector2(Left - 1f, Top + 4f + (i * 8));
                    if (!Scene.CollideCheck<Water>(at) && Scene.CollideCheck<Solid>(at)) {
                        SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, at + add, 0f);
                        SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, at - add, 0f);
                    }
                }
            } else if (crushDir == Vector2.UnitX) {
                Vector2 add = new Vector2(0f, 2f);
                for (int i = 0; i < Height / 8f; i++) {
                    Vector2 at = new Vector2(Right + 1f, Top + 4f + (i * 8));
                    if (!Scene.CollideCheck<Water>(at) && Scene.CollideCheck<Solid>(at)) {
                        SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, at + add, 3.14159274f);
                        SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, at - add, 3.14159274f);
                    }
                }
            } else if (crushDir == -Vector2.UnitY) {
                Vector2 add = new Vector2(2f, 0f);
                for (int i = 0; i < Width / 8f; i++) {
                    Vector2 at = new Vector2(Left + 4f + (i * 8), Top - 1f);
                    if (!Scene.CollideCheck<Water>(at) && Scene.CollideCheck<Solid>(at)) {
                        SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, at + add, 1.57079637f);
                        SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, at - add, 1.57079637f);
                    }
                }
            } else if (crushDir == Vector2.UnitY) {
                Vector2 add = new Vector2(2f, 0f);
                for (int i = 0; i < Width / 8f; i++) {
                    Vector2 at = new Vector2(Left + 4f + (i * 8), Bottom + 1f);
                    if (!Scene.CollideCheck<Water>(at) && Scene.CollideCheck<Solid>(at)) {
                        SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, at + add, -1.57079637f);
                        SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, at - add, -1.57079637f);
                    }
                }
            }

            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            StartShaking(0.4f);
            StopPlayerRunIntoAnimation = true;
            isHit = false;

            SoundSource sfx = currentMoveLoopSfx;
            if (Position == startPoint) {
                currentMoveLoopSfx.Param("end_2", 1f);
            } else {
                currentMoveLoopSfx.Param("end", 1f);
            }
            currentMoveLoopSfx = null;
            Alarm.Set(this, 0.5f, delegate {
                sfx.RemoveSelf();
            }, Alarm.AlarmMode.Oneshot);

            crushDir = Vector2.Zero;
            turnOffImages();
            face.Play("idle", false, false);
            Audio.Play("event:/game/06_reflection/crushblock_rest", Center);
            StartShaking(0.2f);
            yield return 0.2f;
        }

        private bool moveHCheck(float amount, bool breakDashBlocks) {
            if (MoveHCollideSolidsAndBounds(level, amount, breakDashBlocks, null)) {
                if (amount < 0f && Left <= level.Bounds.Left) {
                    return true;
                } else if (amount > 0f && Right >= level.Bounds.Right) {
                    return true;
                } else {
                    for (int i = 1; i <= 4; i++) {
                        for (int j = 1; j >= -1; j -= 2) {
                            Vector2 offset = new Vector2(Math.Sign(amount), (i * j));
                            if (!CollideCheck<Solid>(Position + offset)) {
                                MoveVExact(i * j);
                                MoveHExact(Math.Sign(amount));
                                return false;
                            }
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        private bool MoveVCheck(float amount, bool breakDashBlocks) {
            if (MoveVCollideSolidsAndBounds(level, amount, breakDashBlocks, null)) {
                if (amount < 0f && Top <= level.Bounds.Top) {
                    return true;
                } else if (amount > 0f && Bottom >= (level.Bounds.Bottom + 32)) {
                    return true;
                } else {
                    for (int i = 1; i <= 4; i++) {
                        for (int j = 1; j >= -1; j -= 2) {
                            Vector2 offset = new Vector2((i * j), Math.Sign(amount));
                            if (!CollideCheck<Solid>(Position + offset)) {
                                MoveHExact(i * j);
                                MoveVExact(Math.Sign(amount));
                                return false;
                            }
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        private const float CrushSpeed = 512f;
        private const float CrushAccel = 512f;

        public static ParticleType P_Red;
        public static ParticleType P_Green;

        private bool restrained;
        private string texture;
        private Color fill;

        private Vector2 crushDir;

        private Level level;

        private Coroutine attackCoroutine;

        private bool isHit = false;

        private Sprite face;

        private string nextFaceDirection;

        private List<Image> idleImages;
        private List<Image> activeTopImages;
        private List<Image> activeRightImages;
        private List<Image> activeLeftImages;
        private List<Image> activeBottomImages;

        private SoundSource currentMoveLoopSfx;
    }

}