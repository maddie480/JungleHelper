module JungleHelperCockatiel

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/Cockatiel" Cockatiel(x::Integer, y::Integer, facingLeft::Bool=true, sprite::String="")

const placements = Ahorn.PlacementDict(
    "Cockatiel (Jungle Helper)" => Ahorn.EntityPlacement(
        Cockatiel
    )
)

function Ahorn.selection(entity::Cockatiel)
    x, y = Ahorn.position(entity)
    return Ahorn.Rectangle(x - 6, y - 4, 12, 12)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Cockatiel, room::Maple.Room)
    scaleX = (get(entity.data, "facingLeft", false) ? 1 : -1)
    Ahorn.drawSprite(ctx, "JungleHelper/Cockatiel/idle00", 0, 2, sx = scaleX)
end

end
