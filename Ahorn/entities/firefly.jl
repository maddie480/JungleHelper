module JungleHelperFirefly

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/Firefly" Firefly(x::Integer, y::Integer, number::Integer=1)

const placements = Ahorn.PlacementDict(
    "Firefly (Jungle Helper)" => Ahorn.EntityPlacement(
        Firefly
    )
)

sprite = "JungleHelper/Firefly/firefly00"

function Ahorn.selection(entity::Firefly)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle(sprite, x, y)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Firefly, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0)

end
