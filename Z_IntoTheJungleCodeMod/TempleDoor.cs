﻿using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Linq;

namespace Celeste.Mod.IntoTheJungleCodeMod {
    // A very complex entity that consists in a sprite that triggers a cutscene.
    [CustomEntity("IntoTheJungleCodeMod/TempleDoor")]
    class TempleDoor : Entity {
        public readonly Sprite Sprite;
        private bool playerWasOnGround = false;

        public TempleDoor(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Add(Sprite = IntoTheJungleCodeModule.SpriteBank.Create("temple_door"));

            Depth = 8999; // juuuust in front of bg decals

            // this is the zone that will trigger the cutscene.
            Collider = new Hitbox(60f, 50f, -30f, -18f);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            // we want to find the decals for the totems near the door, and raise their depth so that they are in front of it.
            foreach (Decal decal in Scene.Entities.OfType<Decal>()) {
                if (decal.Name == "decals/Jungle4-temple/totem_top") {
                    decal.Depth = 8998;
                }
            }
        }

        public override void Update() {
            base.Update();

            Player player = CollideFirst<Player>();
            if (Collidable && player != null && player.OnGround()) {
                if (playerWasOnGround) {
                    // trigger the cutscene!
                    Scene.Add(new TempleDoorCutscene(player, this));
                    Collidable = false;
                } else {
                    // wait for 1 more frame.
                    playerWasOnGround = true;
                }
            } else if (player != null && !player.OnGround()) {
                // player isn't on ground anymore, so reset the flag.
                playerWasOnGround = false;
            }
        }
    }
}
