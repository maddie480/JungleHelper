module IntoTheJungleCodeModTempleDoor

using ..Ahorn, Maple

@mapdef Entity "IntoTheJungleCodeMod/TempleDoor" TempleDoor(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Temple Door (Into The Jungle code mod)" => Ahorn.EntityPlacement(
        TempleDoor
    )
)

function Ahorn.selection(entity::TempleDoor)
    x, y = Ahorn.position(entity)
    return Ahorn.Rectangle(x - 30, y - 3, 60, 35)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TempleDoor, room::Maple.Room)
    Ahorn.drawSprite(ctx, "JungleHelper/TempleMainDoor/templemaindoor_14", 0, 15)
end

end
