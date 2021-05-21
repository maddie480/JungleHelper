module JungleHelperLantern

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/Lantern" Lantern(x::Integer, y::Integer, sprite::String="", onlyIfMaddyNotHolding::Bool=false)

const placements = Ahorn.PlacementDict(
    "Lantern (Jungle Helper)" => Ahorn.EntityPlacement(
        Lantern
    )
)

sprite = "JungleHelper/Lantern/LanternEntity/lantern_00"

function Ahorn.selection(entity::Lantern)
    x, y = Ahorn.position(entity)
    return Ahorn.getSpriteRectangle(sprite, x, y + 5)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Lantern, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 5)

end
