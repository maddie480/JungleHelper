using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.JungleHelper.Entities {
    // functionally identical to an ice wall, except it looks like moss and can be dissolved with a lantern.
    [CustomEntity("JungleHelper/MossyWall")]
    public class MossyWall : Entity {
        public static void Load() {
            On.Celeste.Level.Update += onLevelUpdate;
        }

        public static void Unload() {
            On.Celeste.Level.Update -= onLevelUpdate;
        }

        // like PostUpdateHook except before the update
        [Tracked]
        private class PreUpdateHook : Component {
            public Action OnPreUpdate;

            public PreUpdateHook(Action onPreUpdate) : base(active: false, visible: false) {
                OnPreUpdate = onPreUpdate;
            }
        }

        private static void onLevelUpdate(On.Celeste.Level.orig_Update orig, Level self) {
            // call all pre-update hooks...
            foreach (PreUpdateHook component in self.Tracker.GetComponents<PreUpdateHook>()) {
                if (component.Entity.Active) {
                    component.OnPreUpdate();
                }
            }

            // ... *then* update the level
            orig(self);
        }



        private const int LANTERN_ACTIVATION_RADIUS = 75;

        // colors used when player gets close but moss isn't dissolved yet (from closest to furthest, 1 line per pixel).
        private static Color[] getDistanceBasedColors(string color1, string color2, string color3) {
            return new Color[] {
                Calc.HexToColor(color1) * 0.1f,
                Calc.HexToColor(color1) * 0.2f,
                Calc.HexToColor(color1) * 0.3f,
                Calc.HexToColor(color1) * 0.4f,
                Calc.HexToColor(color1) * 0.5f,
                Calc.HexToColor(color1) * 0.6f,
                Calc.HexToColor(color1) * 0.7f,
                Calc.HexToColor(color1) * 0.8f,
                Calc.HexToColor(color1) * 0.9f,
                Calc.HexToColor(color1),
                Color.Lerp(Calc.HexToColor(color1), Calc.HexToColor(color2), 0.1f),
                Color.Lerp(Calc.HexToColor(color1), Calc.HexToColor(color2), 0.2f),
                Color.Lerp(Calc.HexToColor(color1), Calc.HexToColor(color2), 0.3f),
                Color.Lerp(Calc.HexToColor(color1), Calc.HexToColor(color2), 0.4f),
                Color.Lerp(Calc.HexToColor(color1), Calc.HexToColor(color2), 0.5f),
                Color.Lerp(Calc.HexToColor(color1), Calc.HexToColor(color2), 0.6f),
                Color.Lerp(Calc.HexToColor(color1), Calc.HexToColor(color2), 0.7f),
                Color.Lerp(Calc.HexToColor(color1), Calc.HexToColor(color2), 0.8f),
                Color.Lerp(Calc.HexToColor(color1), Calc.HexToColor(color2), 0.9f),
                Calc.HexToColor(color2),
                Color.Lerp(Calc.HexToColor(color2), Calc.HexToColor(color3), 0.1f),
                Color.Lerp(Calc.HexToColor(color2), Calc.HexToColor(color3), 0.2f),
                Color.Lerp(Calc.HexToColor(color2), Calc.HexToColor(color3), 0.3f),
                Color.Lerp(Calc.HexToColor(color2), Calc.HexToColor(color3), 0.4f),
                Color.Lerp(Calc.HexToColor(color2), Calc.HexToColor(color3), 0.5f),
                Color.Lerp(Calc.HexToColor(color2), Calc.HexToColor(color3), 0.6f),
                Color.Lerp(Calc.HexToColor(color2), Calc.HexToColor(color3), 0.7f),
                Color.Lerp(Calc.HexToColor(color2), Calc.HexToColor(color3), 0.8f),
                Color.Lerp(Calc.HexToColor(color2), Calc.HexToColor(color3), 0.9f),
                Calc.HexToColor(color3)
            };
        }

        private readonly Image[] mossParts;
        private readonly Collider[] hitboxes;
        private readonly bool[] hitboxToggles;

        private Vector2 topCenter;
        private Vector2 shake;
        private Color[] distanceColors;

        public MossyWall(EntityData data, Vector2 offset) : base(data.Position + offset) {
            bool left = data.Bool("left");

            distanceColors = getDistanceBasedColors(
                data.Attr("color1", "7A612D"),
                data.Attr("color2", "AABF3D"),
                data.Attr("color3", "33C111")
            );

            Depth = -20000; // FG tiles have depth -10000

            Add(new StaticMover {
                SolidChecker = solid => checkAttachToSolid(solid, left),
                OnMove = move => {
                    Position += move;
                    topCenter += move;
                },
                OnShake = move => shake += move,
                OnDisable = () => Collidable = false,
                OnEnable = () => Collidable = true
            });
            Add(new ClimbBlocker(edge: false));

            List<Image> mossParts = new List<Image>();
            List<Collider> hitboxes = new List<Collider>();
            List<bool> hitboxToggles = new List<bool>();

            Calc.PushRandom();
            for (int i = 0; i < data.Height; i += 8) {
                string id = (i == 0) ? "moss_top" : (i + 16 <= data.Height ? "moss_mid" + Calc.Random.Next(1, 3) : "moss_bottom");
                Image sprite = new Image(GFX.Game[data.Attr("spriteDirectory", "JungleHelper/Moss") + "/" + id]);
                sprite.Position = new Vector2(0f, i);
                sprite.FlipX = !left;
                Add(sprite);

                mossParts.Add(sprite);

                Hitbox hitbox;
                if (left) {
                    hitbox = new Hitbox(2f, 8f, 8f, i);
                } else {
                    hitbox = new Hitbox(2f, 8f, -2f, i);
                }
                hitboxes.Add(hitbox);
                hitboxToggles.Add(true);
            }
            Calc.PopRandom();

            this.mossParts = mossParts.ToArray();
            this.hitboxes = hitboxes.ToArray();
            this.hitboxToggles = hitboxToggles.ToArray();

            Collider = new ColliderList(this.hitboxes);
            topCenter = TopCenter;
        }

        private bool checkAttachToSolid(Solid solid, bool left) {
            bool collides = CollideCheck(solid, Position + (left ? -2 : 2) * Vector2.UnitX);
            if (collides && solid is CassetteBlock cassetteBlock) {
                // check the color of the cassette block we're attached to, to apply it to the moss.
                Color mossColor;
                switch (cassetteBlock.Index) {
                    case 0:
                    default:
                        mossColor = Calc.HexToColor("377FB2");
                        break;
                    case 1:
                        mossColor = Calc.HexToColor("B2378F");
                        break;
                    case 2:
                        mossColor = Calc.HexToColor("B29B2A");
                        break;
                    case 3:
                        mossColor = Calc.HexToColor("269933");
                        break;
                }

                // the colors when the lantern gets close are a fade out from 1 to 0.1 alpha before the moss disappears (no color shifts like regular moss).
                distanceColors = new Color[10];
                for (int i = 0; i < 10; i++) {
                    distanceColors[i] = mossColor * ((i + 1) * 0.1f);
                }

                // make the moss update first so that it doesn't "blink" when Maddy grabs the lantern:
                // the cassette block will fix the depth when it gets updated anyway...
                Add(new PreUpdateHook(() => {
                    Depth = -20000;
                    Scene.Entities.UpdateLists();
                }));

                // ... then, when the cassette block is done setting the depth,
                // make sure the moss appears on top of it (because the cassette block moves it just behind).
                Add(new PostUpdateHook(() => {
                    Depth -= 2;
                    Scene.Entities.UpdateLists();
                }));
            }

            return collides;
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            updateLanternFade();
        }

        public override void Update() {
            base.Update();
            updateLanternFade();
        }

        private void updateLanternFade() {
            bool shouldUpdate = false;

            for (int i = 0; i < mossParts.Length; i++) {
                Image mossPart = mossParts[i];
                bool shouldBeCollidable = false;
                float distance = Lantern.GetClosestLanternDistanceTo(topCenter + mossPart.Position, Scene, out _);

                if (distance < LANTERN_ACTIVATION_RADIUS) {
                    // moss is dissolved, make it uncollidable.
                    mossPart.Visible = false;
                } else if (distance - LANTERN_ACTIVATION_RADIUS < distanceColors.Length) {
                    // moss has a particular fade of color.
                    mossPart.Visible = true;
                    mossPart.Color = distanceColors[(int) Math.Floor(distance - LANTERN_ACTIVATION_RADIUS)];

                    // if moss is more than 50% visible, it should be collidable.
                    if (Math.Floor(distance - LANTERN_ACTIVATION_RADIUS) >= 5) {
                        shouldBeCollidable = true;
                    }
                } else {
                    // moss is green (player is too far or not here), make it collidable.
                    mossPart.Visible = true;
                    mossPart.Color = distanceColors[distanceColors.Length - 1];
                    shouldBeCollidable = true;
                }

                if (!Collidable) {
                    // moss is deactivated (when attached to a cassette block), make it darker.
                    mossPart.Color.R /= 4;
                    mossPart.Color.G /= 4;
                    mossPart.Color.B /= 4;
                }

                if (shouldBeCollidable != hitboxToggles[i]) {
                    // that hitbox needs to be toggled!
                    hitboxToggles[i] = shouldBeCollidable;
                    shouldUpdate = true;
                }
            }

            // skip the collider rebuilding if nothing changed, this is just going to waste the GC's time
            if (!shouldUpdate) return;

            List<Collider> enabledHitboxes = new List<Collider>();
            for (int i = 0; i < mossParts.Length; i++) {
                if (hitboxToggles[i]) enabledHitboxes.Add(hitboxes[i]);
            }

            if (enabledHitboxes.Count == 0) {
                Collider = null;
            } else {
                Collider = new ColliderList(enabledHitboxes.ToArray());
            }
        }

        public override void Render() {
            Vector2 position = Position;
            Position += shake;
            base.Render();
            Position = position;
        }
    }
}
