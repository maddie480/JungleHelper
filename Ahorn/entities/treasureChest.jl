module JungleHelperTreasureChest

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/TreasureChest" TreasureChest(x::Integer, y::Integer, sprite::String="")

const placements = Ahorn.PlacementDict(
    "Treasure Chest (Jungle Helper)" => Ahorn.EntityPlacement(
        TreasureChest
    )
)

function Ahorn.selection(entity::TreasureChest)
    x, y = Ahorn.position(entity)
    return Ahorn.Rectangle(x - 32, y - 24, 64, 48)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TreasureChest, room::Maple.Room)
    Ahorn.drawSprite(ctx, "JungleHelper/Treasure/TreasureIdle00", 0, 0)
end

end
