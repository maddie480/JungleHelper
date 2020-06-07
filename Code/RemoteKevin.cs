using System;
using System.Collections;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;

namespace Celeste.Mod.JungleHelper {
    // IT MAY BE A "SLIDE BLOCK" OFFICIALLY, BUT IT WILL ALWAYS BE A REMOTE KEVIN IN MY HEART
    [CustomEntity("JungleHelper/RemoteKevin")]
    public class RemoteKevin : Solid {
        public RemoteKevin(Vector2 position, float width, float height, bool restrained, Axes axes, bool chillOut = false, bool core = false) : base(position, width, height, false) {
            this.core = core;
            this.restrained = restrained;
            texture = (restrained ? "objects/slideBlock/green" : "objects/slideBlock/red");
            fill = Calc.HexToColor("8A9C60");
            idleImages = new List<Image>();
            activeTopImages = new List<Image>();
            activeRightImages = new List<Image>();
            activeLeftImages = new List<Image>();
            activeBottomImages = new List<Image>();
            returnStack = new List<MoveState>();
            this.chillOut = chillOut;
            Add(new DashListener {
                OnDash = new Action<Vector2>(OnDash)
            });
            giant = (Width >= 48f && Height >= 48f && chillOut);
            canActivate = true;
            attackCoroutine = new Coroutine(true);
            attackCoroutine.RemoveOnComplete = false;
            Add(attackCoroutine);
            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(texture + "/block");
            MTexture idle;
            switch (axes) {
                default:
                    idle = atlasSubtextures[3];
                    canMoveHorizontally = (canMoveVertically = true);
                    break;
                case Axes.Horizontal:
                    idle = atlasSubtextures[1];
                    canMoveHorizontally = true;
                    canMoveVertically = false;
                    break;
                case Axes.Vertical:
                    idle = atlasSubtextures[2];
                    canMoveHorizontally = false;
                    canMoveVertically = true;
                    break;
            }
            Add(face = JungleHelperModule.SpriteBank.Create("slideblock_face"));
            face.Position = new Vector2(Width, Height) / 2f;
            face.Play("idle", false, false);
            face.OnLastFrame = delegate (string f) {
                bool flag = f == "hit";
                if (flag) {
                    face.Play(nextFaceDirection, false, false);
                }
            };
            int num = (int) (Width / 8f) - 1;
            int num2 = (int) (Height / 8f) - 1;
            AddImage(idle, 0, 0, 0, 0, -1, -1);
            AddImage(idle, num, 0, 3, 0, 1, -1);
            AddImage(idle, 0, num2, 0, 3, -1, 1);
            AddImage(idle, num, num2, 3, 3, 1, 1);
            for (int i = 1; i < num; i++) {
                AddImage(idle, i, 0, Calc.Random.Choose(1, 2), 0, 0, -1);
                AddImage(idle, i, num2, Calc.Random.Choose(1, 2), 3, 0, 1);
            }
            for (int j = 1; j < num2; j++) {
                AddImage(idle, 0, j, 0, Calc.Random.Choose(1, 2), -1, 0);
                AddImage(idle, num, j, 3, Calc.Random.Choose(1, 2), 1, 0);
            }
            Add(new LightOcclude(0.2f));
            Add(returnLoopSfx = new SoundSource());
            Add(new WaterInteraction(() => crushDir != Vector2.Zero));
        }

        public RemoteKevin(EntityData data, Vector2 offset) : this(data.Position + offset, (float) data.Width, (float) data.Height, data.Bool("restrained", false), data.Enum("axes", Axes.Both), data.Bool("chillout", false), data.Bool("core", false)) {
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            level = SceneAs<Level>();
        }

        public override void Update() {
            base.Update();
            bool flag = crushDir == Vector2.Zero;
            if (flag) {
                face.Position = new Vector2(Width, Height) / 2f;
                bool flag2 = CollideCheck<Player>(Position + new Vector2(-1f, 0f));
                if (flag2) {
                    face.X -= 1f;
                } else {
                    bool flag3 = CollideCheck<Player>(Position + new Vector2(1f, 0f));
                    if (flag3) {
                        face.X += 1f;
                    } else {
                        bool flag4 = CollideCheck<Player>(Position + new Vector2(0f, -1f));
                        if (flag4) {
                            face.Y -= 1f;
                        }
                    }
                }
            }
            bool flag5 = currentMoveLoopSfx != null;
            if (flag5) {
                currentMoveLoopSfx.Param("submerged", (float) (Submerged ? 1 : 0));
            }
            bool flag6 = returnLoopSfx != null;
            if (flag6) {
                returnLoopSfx.Param("submerged", (float) (Submerged ? 1 : 0));
            }
        }

        public override void Render() {
            Vector2 position = Position;
            Position += Shake;
            Draw.Rect(X + 2f, Y + 2f, Width - 4f, Height - 4f, fill);
            base.Render();
            Position = position;
        }

        private bool Submerged {
            get {
                return Scene.CollideCheck<Water>(new Rectangle((int) (Center.X - 4f), (int) Center.Y, 8, 4));
            }
        }
        public void OnDash(Vector2 direction) {
            // if one of the directions is zero and the other isn't, this is a straight (non diagonal) dash, so we should trigger the Kevin.
            if ((direction.X == 0) != (direction.Y == 0)) {
                Attack(direction);
            }
        }
        private void AddImage(MTexture idle, int x, int y, int tx, int ty, int borderX = 0, int borderY = 0) {
            MTexture subtexture = idle.GetSubtexture(tx * 8, ty * 8, 8, 8, null);
            Vector2 vector = new Vector2((float) (x * 8), (float) (y * 8));
            bool flag = borderX != 0;
            if (flag) {
                Add(new Image(subtexture) {
                    Color = Color.Black,
                    Position = vector + new Vector2((float) borderX, 0f)
                });
            }
            bool flag2 = borderY != 0;
            if (flag2) {
                Add(new Image(subtexture) {
                    Color = Color.Black,
                    Position = vector + new Vector2(0f, (float) borderY)
                });
            }
            Image image = new Image(subtexture);
            image.Position = vector;
            Add(image);
            idleImages.Add(image);
            bool flag3 = borderX != 0 || borderY != 0;
            if (flag3) {
                bool flag4 = borderX < 0;
                if (flag4) {
                    Image image2 = new Image(GFX.Game[texture + "/lit_left"].GetSubtexture(0, ty * 8, 8, 8, null));
                    activeLeftImages.Add(image2);
                    image2.Position = vector;
                    image2.Visible = false;
                    Add(image2);
                } else {
                    bool flag5 = borderX > 0;
                    if (flag5) {
                        Image image3 = new Image(GFX.Game[texture + "/lit_right"].GetSubtexture(0, ty * 8, 8, 8, null));
                        activeRightImages.Add(image3);
                        image3.Position = vector;
                        image3.Visible = false;
                        Add(image3);
                    }
                }
                bool flag6 = borderY < 0;
                if (flag6) {
                    Image image4 = new Image(GFX.Game[texture + "/lit_top"].GetSubtexture(tx * 8, 0, 8, 8, null));
                    activeTopImages.Add(image4);
                    image4.Position = vector;
                    image4.Visible = false;
                    Add(image4);
                } else {
                    bool flag7 = borderY > 0;
                    if (flag7) {
                        Image image5 = new Image(GFX.Game[texture + "/lit_bottom"].GetSubtexture(tx * 8, 0, 8, 8, null));
                        activeBottomImages.Add(image5);
                        image5.Position = vector;
                        image5.Visible = false;
                        Add(image5);
                    }
                }
            }
        }

        private void TurnOffImages() {
            foreach (Image image in activeLeftImages) {
                image.Visible = false;
            }
            foreach (Image image2 in activeRightImages) {
                image2.Visible = false;
            }
            foreach (Image image3 in activeTopImages) {
                image3.Visible = false;
            }
            foreach (Image image4 in activeBottomImages) {
                image4.Visible = false;
            }
        }


        private bool CanActivate(Vector2 direction) {
            bool flag = giant && direction.X <= 0f;
            bool result;
            if (flag) {
                result = false;
            } else {
                bool flag2 = canActivate && crushDir != direction;
                if (flag2) {
                    bool flag3 = direction.X != 0f && !canMoveHorizontally;
                    if (flag3) {
                        result = false;
                    } else {
                        bool flag4 = direction.Y != 0f && !canMoveVertically;
                        result = !flag4;
                    }
                } else {
                    result = false;
                }
            }
            return result;
        }

        private void Attack(Vector2 direction) {
            if (!isHit) {
                Audio.Play("event:/game/05_mirror_temple/swapblock_move", Center);
                bool flag = currentMoveLoopSfx != null;
                if (flag) {
                    currentMoveLoopSfx.Param("end", 1f);
                    SoundSource sfx = currentMoveLoopSfx;
                    sfx.RemoveSelf();
                }
                Add(currentMoveLoopSfx = new SoundSource());
                currentMoveLoopSfx.Position = new Vector2(Width, Height) / 2f;
                currentMoveLoopSfx.Play("event:/junglehelper/sfx/Slide_block", null, 0f);
                crushDir = direction;
                canActivate = false;
                attackCoroutine.Replace(AttackSequence());
                face.Play("hit", false, false);
                ClearRemainder();
                TurnOffImages();
                bool flag2 = crushDir.X < 0f;
                if (flag2) {
                    foreach (Image image in activeLeftImages) {
                        image.Visible = true;
                    }
                    nextFaceDirection = "left";
                } else {
                    bool flag3 = crushDir.X > 0f;
                    if (flag3) {
                        foreach (Image image2 in activeRightImages) {
                            image2.Visible = true;
                        }
                        nextFaceDirection = "right";
                    } else {
                        bool flag4 = crushDir.Y < 0f;
                        if (flag4) {
                            foreach (Image image3 in activeTopImages) {
                                image3.Visible = true;
                            }
                            nextFaceDirection = "up";
                        } else {
                            bool flag5 = crushDir.Y > 0f;
                            if (flag5) {
                                foreach (Image image4 in activeBottomImages) {
                                    image4.Visible = true;
                                }
                                nextFaceDirection = "down";
                            }
                        }
                    }
                }
                bool flag6 = true;
                bool flag7 = returnStack.Count > 0;
                if (flag7) {
                    MoveState moveState = returnStack[returnStack.Count - 1];
                    bool flag8 = moveState.Direction == direction || moveState.Direction == -direction;
                    if (flag8) {
                        flag6 = false;
                    }
                }
                bool flag9 = flag6;
                if (flag9) {
                    returnStack.Add(new MoveState(Position, crushDir));
                }
            }
        }


        private IEnumerator AttackSequence() {
            isHit = true;
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            bool flag = !chillOut;
            if (flag) {
                canActivate = true;
            }
            StopPlayerRunIntoAnimation = false;
            bool slowing = false;
            float speed = 0f;
            float distance = (crushDir.X != 0 ? Width : Height);
            Action som = null; // = null wasn't there
            for (; ; )
            {
                speed = Calc.Approach(speed, CrushSpeed, CrushAccel * Engine.DeltaTime); // was speed, 240f, 500f
                bool flag5 = crushDir.X != 0f;
                bool hit;

                float moveAmount = speed * Engine.DeltaTime;
                if (restrained) {
                    moveAmount = Math.Min(moveAmount, distance);
                    distance -= moveAmount;
                }

                if (flag5) {
                    hit = MoveHCheck(moveAmount * crushDir.X);
                } else {
                    hit = MoveVCheck(moveAmount * crushDir.Y);
                }
                bool flag6 = hit;
                if (flag6 || (restrained && distance <= 0f)) {
                    if (flag6) {
                        Audio.Play("event:/game/06_reflection/crushblock_impact", Center);
                    }
                    break;
                }
                bool flag7 = Scene.OnInterval(0.02f);
                if (flag7) {
                    bool flag8 = crushDir == Vector2.UnitX;
                    Vector2 at;
                    float dir;
                    if (flag8) {
                        at = new Vector2(Left + 1f, Calc.Random.Range(Top + 3f, Bottom - 3f));
                        dir = 3.14159274f;
                    } else {
                        bool flag9 = crushDir == -Vector2.UnitX;
                        if (flag9) {
                            at = new Vector2(Right - 1f, Calc.Random.Range(Top + 3f, Bottom - 3f));
                            dir = 0f;
                        } else {
                            bool flag10 = crushDir == Vector2.UnitY;
                            if (flag10) {
                                at = new Vector2(Calc.Random.Range(Left + 3f, Right - 3f), Top + 1f);
                                dir = -1.57079637f;
                            } else {
                                at = new Vector2(Calc.Random.Range(Left + 3f, Right - 3f), Bottom - 1f);
                                dir = 1.57079637f;
                            }
                        }
                    }
                    at = default;
                }
                yield return null;
            }
            FallingBlock fallingBlock = CollideFirst<FallingBlock>(Position + crushDir);
            bool flag11 = fallingBlock != null;
            if (flag11) {
                fallingBlock.Triggered = true;
            }
            bool flag12 = crushDir == -Vector2.UnitX;
            if (flag12) {
                Vector2 add = new Vector2(0f, 2f);
                int i = 0;
                while ((float) i < Height / 8f) {
                    Vector2 at2 = new Vector2(Left - 1f, Top + 4f + (float) (i * 8));
                    bool flag13 = !Scene.CollideCheck<Water>(at2) && Scene.CollideCheck<Solid>(at2);
                    if (flag13) {
                        SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, at2 + add, 0f);
                        SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, at2 - add, 0f);
                    }
                    at2 = default;
                    int num = i;
                    i = num + 1;
                }
                add = default;
            } else {
                bool flag14 = crushDir == Vector2.UnitX;
                if (flag14) {
                    Vector2 add2 = new Vector2(0f, 2f);
                    int j = 0;
                    while ((float) j < Height / 8f) {
                        Vector2 at3 = new Vector2(Right + 1f, Top + 4f + (float) (j * 8));
                        bool flag15 = !Scene.CollideCheck<Water>(at3) && Scene.CollideCheck<Solid>(at3);
                        if (flag15) {
                            SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, at3 + add2, 3.14159274f);
                            SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, at3 - add2, 3.14159274f);
                        }
                        at3 = default;
                        int num = j;
                        j = num + 1;
                    }
                    add2 = default;
                } else {
                    bool flag16 = crushDir == -Vector2.UnitY;
                    if (flag16) {
                        Vector2 add3 = new Vector2(2f, 0f);
                        int k = 0;
                        while ((float) k < Width / 8f) {
                            Vector2 at4 = new Vector2(Left + 4f + (float) (k * 8), Top - 1f);
                            bool flag17 = !Scene.CollideCheck<Water>(at4) && Scene.CollideCheck<Solid>(at4);
                            if (flag17) {
                                SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, at4 + add3, 1.57079637f);
                                SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, at4 - add3, 1.57079637f);
                            }
                            at4 = default;
                            int num = k;
                            k = num + 1;
                        }
                        add3 = default;
                    } else {
                        bool flag18 = crushDir == Vector2.UnitY;
                        if (flag18) {
                            Vector2 add4 = new Vector2(2f, 0f);
                            int l = 0;
                            while ((float) l < Width / 8f) {
                                Vector2 at5 = new Vector2(Left + 4f + (float) (l * 8), Bottom + 1f);
                                bool flag19 = !Scene.CollideCheck<Water>(at5) && Scene.CollideCheck<Solid>(at5);
                                if (flag19) {
                                    SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, at5 + add4, -1.57079637f);
                                    SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, at5 - add4, -1.57079637f);
                                }
                                at5 = default;
                                int num = l;
                                l = num + 1;
                            }
                            add4 = default;
                        }
                    }
                }
            }
            level.DirectionalShake(crushDir, 0.3f);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            StartShaking(0.4f);
            StopPlayerRunIntoAnimation = true;
            isHit = false;
            SoundSource sfx = currentMoveLoopSfx;
            currentMoveLoopSfx.Param("end", 1f);
            currentMoveLoopSfx = null;
            Alarm.Set(this, 0.5f, delegate {
                sfx.RemoveSelf();
            }, Alarm.AlarmMode.Oneshot);
            crushDir = Vector2.Zero;
            TurnOffImages();
            face.Play("idle", false, false);
            returnLoopSfx.Stop(true);
            Audio.Play("event:/game/06_reflection/crushblock_rest", Center);
            StartShaking(0.2f);
            yield return 0.2f;
        }

        private bool MoveHCheck(float amount) {
            bool flag = MoveHCollideSolidsAndBounds(level, amount, true, null);
            bool result;
            if (flag) {
                bool flag2 = amount < 0f && Left <= (float) level.Bounds.Left;
                if (flag2) {
                    result = true;
                } else {
                    bool flag3 = amount > 0f && Right >= (float) level.Bounds.Right;
                    if (flag3) {
                        result = true;
                    } else {
                        for (int i = 1; i <= 4; i++) {
                            for (int j = 1; j >= -1; j -= 2) {
                                Vector2 value = new Vector2((float) Math.Sign(amount), (float) (i * j));
                                bool flag4 = !CollideCheck<Solid>(Position + value);
                                if (flag4) {
                                    MoveVExact(i * j);
                                    MoveHExact(Math.Sign(amount));
                                    return false;
                                }
                            }
                        }
                        result = true;
                    }
                }
            } else {
                result = false;
            }
            return result;
        }

        private bool MoveVCheck(float amount) {
            bool flag = MoveVCollideSolidsAndBounds(level, amount, true, null);
            bool result;
            if (flag) {
                bool flag2 = amount < 0f && Top <= (float) level.Bounds.Top;
                if (flag2) {
                    result = true;
                } else {
                    bool flag3 = amount > 0f && Bottom >= (float) (level.Bounds.Bottom + 32);
                    if (flag3) {
                        result = true;
                    } else {
                        for (int i = 1; i <= 4; i++) {
                            for (int j = 1; j >= -1; j -= 2) {
                                Vector2 value = new Vector2((float) (i * j), (float) Math.Sign(amount));
                                bool flag4 = !CollideCheck<Solid>(Position + value);
                                if (flag4) {
                                    MoveHExact(i * j);
                                    MoveVExact(Math.Sign(amount));
                                    return false;
                                }
                            }
                        }
                        result = true;
                    }
                }
            } else {
                result = false;
            }
            return result;
        }

        public bool core;

        public static ParticleType P_Impact;

        public static ParticleType P_Crushing;

        public static ParticleType P_Activate;

        private const float CrushSpeed = 512f;

        private const float CrushAccel = 512f;

        private bool restrained;

        private string texture;

        private Color fill;

        private Level level;

        private bool canActivate;

        private Vector2 crushDir;

        private List<MoveState> returnStack;

        private Coroutine attackCoroutine;

        private bool canMoveVertically;

        private bool canMoveHorizontally;

        private bool isHit = false;

        private bool chillOut;

        private bool giant;

        private Sprite face;

        private string nextFaceDirection;

        private List<Image> idleImages;

        private List<Image> activeTopImages;

        private List<Image> activeRightImages;

        // Token: 0x040007FF RID: 2047
        private List<Image> activeLeftImages;

        // Token: 0x04000800 RID: 2048
        private List<Image> activeBottomImages;

        // Token: 0x04000801 RID: 2049
        private SoundSource currentMoveLoopSfx;

        // Token: 0x04000802 RID: 2050
        private SoundSource returnLoopSfx;

        // Token: 0x0200019C RID: 412
        public enum Axes {
            Both,
            Horizontal,
            Vertical
        }

        private struct MoveState {
            // Token: 0x06000D5A RID: 3418 RVA: 0x00031326 File Offset: 0x0002F526
            public MoveState(Vector2 from, Vector2 direction) {
                From = from;
                Direction = direction;
            }

            // Token: 0x04000807 RID: 2055
            public Vector2 From;

            // Token: 0x04000808 RID: 2056
            public Vector2 Direction;
        }
    }

}