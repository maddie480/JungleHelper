module JungleHelperRemoteKevinRefills

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/RemoteKevinRefill" RemoteKevinRefill(x::Integer, y::Integer, oneUse::Bool=false, usedByPlayer::Bool=true, usedBySlideBlock::Bool=true)

const placements = Ahorn.PlacementDict(
    "Slide Block Refill (Used By Player) (Jungle Helper)" => Ahorn.EntityPlacement(
        RemoteKevinRefill,
        "rectangle",
        Dict{String, Any}(
            "usedByPlayer" => true,
            "usedBySlideBlock" => false
        )
    ),
    "Slide Block Refill (Used By Slide Block) (Jungle Helper)" => Ahorn.EntityPlacement(
        RemoteKevinRefill,
        "rectangle",
        Dict{String, Any}(
            "usedByPlayer" => false,
            "usedBySlideBlock" => true
        )
    )
)

function Ahorn.selection(entity::RemoteKevinRefill)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle("objects/refill/idle00", x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RemoteKevinRefill, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/refill/idle00", 0, 0)
end

end
