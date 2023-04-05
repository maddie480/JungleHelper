module IntoTheJungleCodeModLanternHeartSpawner

using ..Ahorn, Maple

@mapdef Entity "IntoTheJungleCodeMod/LanternHeartSpawner" LanternHeartSpawner(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Lantern Heart Spawner (Into The Jungle code mod)" => Ahorn.EntityPlacement(
        LanternHeartSpawner
    )
)

const sprite = "collectables/heartGem/0/00.png"

function Ahorn.selection(entity::LanternHeartSpawner)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle(sprite, x, y)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::LanternHeartSpawner, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0)

end
