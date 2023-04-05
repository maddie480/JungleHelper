module JungleHelperBouncyShroom

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/BouncyShroomUp" BouncyShroomUp(x::Integer, y::Integer, yeetx::Integer = 200, yeety::Integer = -290, spriteDirectory::String="JungleHelper/BouncyShroom")
@mapdef Entity "JungleHelper/BouncyShroomLeft" BouncyShroomLeft(x::Integer, y::Integer, yeetx::Integer = 200, yeety::Integer = -290, spriteDirectory::String="JungleHelper/BouncyShroom")
@mapdef Entity "JungleHelper/BouncyShroomRight" BouncyShroomRight(x::Integer, y::Integer, yeetx::Integer = 200, yeety::Integer = -290, spriteDirectory::String="JungleHelper/BouncyShroom")

const placements = Ahorn.PlacementDict()

BouncyShroom = Dict{String, Type}(
    "up" => BouncyShroomUp,
    "left" => BouncyShroomLeft,
    "right" => BouncyShroomRight
)

BouncyShroomUnion = Union{BouncyShroomUp, BouncyShroomLeft, BouncyShroomRight}

for (dir, entity) in BouncyShroom
    key = "Bouncy Shroom ($(uppercasefirst(dir))) (Jungle Helper)"
    placements[key] = Ahorn.EntityPlacement(
        entity
    )
end

directions = Dict{String, String}(
    "JungleHelper/BouncyShroomUp" => "up",
    "JungleHelper/BouncyShroomLeft" => "left",
    "JungleHelper/BouncyShroomRight" => "right",
)

Ahorn.editingOptions(entity::BouncyShroomUnion) = Dict{String, Any}(
    "spriteDirectory" => String["JungleHelper/BouncyShroom", "JungleHelper/BouncyShroomOutline"]
)

function Ahorn.selection(entity::BouncyShroomUnion)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 16, y - 16, 24, 24)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BouncyShroomUp, room::Maple.Room) = Ahorn.drawSprite(ctx, get(entity, "spriteDirectory", "JungleHelper/BouncyShroom") * "/mushroom00", -4, -4)
Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BouncyShroomLeft, room::Maple.Room) = Ahorn.drawSprite(ctx, get(entity, "spriteDirectory", "JungleHelper/BouncyShroom") * "/mushroom_ld_00", -4, -4)
Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BouncyShroomRight, room::Maple.Room) = Ahorn.drawSprite(ctx, get(entity, "spriteDirectory", "JungleHelper/BouncyShroom") * "/mushroom_rd_00", -4, -4)

end