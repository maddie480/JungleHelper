module JungleHelperRollingRock

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/RollingRock" RollingRock(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Rolling Rock (Jungle Helper)" => Ahorn.EntityPlacement(
        RollingRock
    )
)

sprite = "JungleHelper/RollingRock/circle_of_doom_please_replace00"

function Ahorn.selection(entity::RollingRock)
    x, y = Ahorn.position(entity)
    return Ahorn.getSpriteRectangle(sprite, x, y)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RollingRock, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0)

end
