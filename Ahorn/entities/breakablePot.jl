module JungleHelperBreakablePot

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/BreakablePot" BreakablePot(x::Integer, y::Integer, sprite::String="", rupeeImage::String="JungleHelper/Breakable Pot/rupee")

const placements = Ahorn.PlacementDict(
    "Breakable Pot (Jungle Helper)" => Ahorn.EntityPlacement(
        BreakablePot
    )
)

function Ahorn.selection(entity::BreakablePot)
    x, y = Ahorn.position(entity)
    return Ahorn.Rectangle(x - 8, y - 20, 17, 20)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BreakablePot, room::Maple.Room) = Ahorn.drawSprite(ctx, "JungleHelper/Breakable Pot/breakpotidle", 0, -9)

end
