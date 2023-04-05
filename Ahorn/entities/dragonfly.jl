module JungleHelperDragonfly

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/Dragonfly" Dragonfly(x::Integer, y::Integer, wingsColor::String="FFFFFF", sprite::String="")

const placements = Ahorn.PlacementDict(
    "Dragonfly (Jungle Helper)" => Ahorn.EntityPlacement(
        Dragonfly
    )
)

function Ahorn.selection(entity::Dragonfly)
    x, y = Ahorn.position(entity)
    return Ahorn.Rectangle(x, y, 32, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Dragonfly, room::Maple.Room)
    color = Ahorn.argb32ToRGBATuple(parse(Int, entity.wingsColor, base=16))[1:3] ./ 255
    color = (color..., 1.0)
    Ahorn.drawSprite(ctx, "JungleHelper/Dragonfly/dragonfly_body00", 0, 0, jx=0, jy=0)
    Ahorn.drawSprite(ctx, "JungleHelper/Dragonfly/dragonfly_wings00", 0, 0, jx=0, jy=0, tint=color)
end

end
