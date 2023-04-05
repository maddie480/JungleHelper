module JungleHelperFallingKillbox

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/FallingKillbox" FallingKillbox(x::Integer, y::Integer, width::Integer=Maple.defaultBlockWidth, fallSpeed::Number=100.0)

const placements = Ahorn.PlacementDict(
    "Falling Killbox (Jungle Helper)" => Ahorn.EntityPlacement(
        FallingKillbox,
        "rectangle"
    ),
)

Ahorn.minimumSize(entity::FallingKillbox) = 8, 0
Ahorn.resizable(entity::FallingKillbox) = true, false

function Ahorn.selection(entity::FallingKillbox)
    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 8))
    height = 32

    return Ahorn.Rectangle(x, y, width, height)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FallingKillbox, room::Maple.Room)
    width = Int(get(entity.data, "width", 32))
    height = 32

    Ahorn.drawRectangle(ctx, 0, 0, width, height, (0.8, 0.4, 0.4, 0.8), (0.0, 0.0, 0.0, 0.0))
end

end
