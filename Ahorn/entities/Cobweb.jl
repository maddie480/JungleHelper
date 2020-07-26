module JungleHelperCobweb

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/Cobweb" Cobweb(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Cobweb (Jungle Helper)" => Ahorn.EntityPlacement(
        Cobweb
    )
)

sprite = "JungleHelper/Cobweb/idle00"

function Ahorn.selection(entity::Cobweb)
    x, y = Ahorn.position(entity)
    return Ahorn.getSpriteRectangle(sprite, x, y)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Cobweb, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0)

end
