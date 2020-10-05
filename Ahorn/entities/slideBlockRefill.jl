module JungleHelperRemoteKevinRefills

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/RemoteKevinRefill" RemoteKevinRefill(x::Integer, y::Integer, oneUse::Bool=false)

const placements = Ahorn.PlacementDict(
    "Slide Block Refill (Jungle Helper)" => Ahorn.EntityPlacement(
        RemoteKevinRefill,
        "rectangle"
    )
)

function Ahorn.selection(entity::RemoteKevinRefill)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle("JungleHelper/SlideBlockRefill/idle00", x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RemoteKevinRefill, room::Maple.Room)
    Ahorn.drawSprite(ctx, "JungleHelper/SlideBlockRefill/idle00", 0, 0)
end

end
