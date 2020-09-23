using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JungleHelper.Entities {
    [CustomEntity("JungleHelper/Lantern")]
    class Lantern : Entity {
        public const PlayerSpriteMode SpriteModeMadelineLantern = (PlayerSpriteMode) 444480;

        public static void Load() {
            On.Celeste.LevelLoader.ctor += onLevelLoaderConstructor;
            On.Celeste.PlayerSprite.ctor += onPlayerSpriteConstructor;
        }

        public static void Unload() {
            On.Celeste.LevelLoader.ctor -= onLevelLoaderConstructor;
            On.Celeste.PlayerSprite.ctor -= onPlayerSpriteConstructor;
        }

        private static void onLevelLoaderConstructor(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startPosition) {
            orig(self, session, startPosition);

            // we want Madeline's sprite to load its metadata (that is, her hair positions on her frames of animation).
            PlayerSprite.CreateFramesMetadata("junglehelper_madeline_lantern");
        }

        private static void onPlayerSpriteConstructor(On.Celeste.PlayerSprite.orig_ctor orig, PlayerSprite self, PlayerSpriteMode mode) {
            bool lantern = mode == SpriteModeMadelineLantern;
            if (lantern) {
                // build regular Madeline with backpack as a reference.
                mode = PlayerSpriteMode.Madeline;
            }

            orig(self, mode);

            if (lantern) {
                // throw lantern Madeline sprites in the mix.
                GFX.SpriteBank.CreateOn(self, "junglehelper_madeline_lantern");
            }
        }

        public Lantern(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Sprite sprite = JungleHelperModule.SpriteBank.Create("lantern");
            sprite.Y = 5;
            Add(sprite);
            Collider = new Hitbox(8, 8, -4, 0);

            Add(new PlayerCollider(onPlayer));
        }

        private void onPlayer(Player player) {
            // now that's the perfect entity, right?
            player.ResetSprite(SpriteModeMadelineLantern);
            RemoveSelf();
        }
    }
}
