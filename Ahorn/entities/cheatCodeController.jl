module JungleHelperCheatCodeController

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/CheatCodeController" CheatCodeController(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Cheat Code Controller (Jungle Helper)" => Ahorn.EntityPlacement(
        CheatCodeController
    )
)

function Ahorn.selection(entity::CheatCodeController)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CheatCodeController, room::Maple.Room) = Ahorn.drawImage(ctx, "ahorn/JungleHelper/cheat_code", -12, -12)

end
