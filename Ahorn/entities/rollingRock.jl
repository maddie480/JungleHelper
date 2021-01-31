module JungleHelperRollingRock

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/RollingRock" RollingRock(x::Integer, y::Integer, cracked::Bool=false, sprite::String="", flag::String="")

const placements = Ahorn.PlacementDict(
    "Rolling Rock (Jungle Helper)" => Ahorn.EntityPlacement(
        RollingRock
    )
)

function Ahorn.selection(entity::RollingRock)
    x, y = Ahorn.position(entity)
    spriteName = get(entity.data, "cracked", false) ? "boulder_cracked" : "boulder"
    sprite = "JungleHelper/RollingRock/$(spriteName)"
    return Ahorn.getSpriteRectangle(sprite, x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RollingRock, room::Maple.Room)
    spriteName = get(entity.data, "cracked", false) ? "boulder_cracked" : "boulder"
    sprite = "JungleHelper/RollingRock/$(spriteName)"
    Ahorn.drawSprite(ctx, sprite, 0, 0)
end

end
