module JungleHelperCrabSmall

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/CrabSmall" CrabSmall(x::Integer, y::Integer, facingLeft::Bool=true, sprite::String="")

const placements = Ahorn.PlacementDict(
    "CrabSmall (Jungle Helper)" => Ahorn.EntityPlacement(
        CrabSmall
    )
)

function Ahorn.selection(entity::CrabSmall)
    x, y = Ahorn.position(entity)
    return Ahorn.Rectangle(x + 2, y + 2, 16, 8)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CrabSmall, room::Maple.Room)
    scaleX = (get(entity.data, "facingLeft", false) ? 1 : -1)
    Ahorn.drawSprite(ctx, "JungleHelper/CrabSmall/IdleA00", 0, 2, sx = scaleX)
end

end
