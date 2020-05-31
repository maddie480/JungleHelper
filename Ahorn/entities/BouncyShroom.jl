module JungleHelperBouncyShroom

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/BouncyShroomUp" BouncyShroomUp(x::Integer, y::Integer, yeetx::Integer = 200, yeety::Integer = -290)
@mapdef Entity "JungleHelper/BouncyShroomLeft" BouncyShroomLeft(x::Integer, y::Integer, yeetx::Integer = 200, yeety::Integer = -290)
@mapdef Entity "JungleHelper/BouncyShroomRight" BouncyShroomRight(x::Integer, y::Integer, yeetx::Integer = 200, yeety::Integer = -290)

const placements = Ahorn.PlacementDict()

BouncyShroom = Dict{String, Type}(
    "up" => BouncyShroomUp,
    "left" => BouncyShroomLeft,
    "right" => BouncyShroomRight
)

BouncyShroomUnion = Union{BouncyShroomUp, BouncyShroomLeft, BouncyShroomRight}

for (dir, entity) in BouncyShroom
	key = "Bouncy Shroom ($(uppercasefirst(dir))) (JungleHelper)"
	placements[key] = Ahorn.EntityPlacement(
		entity
	)
end

directions = Dict{String, String}(
    "JungleHelper/BouncyShroomUp" => "up",
    "JungleHelper/BouncyShroomLeft" => "left",
    "JungleHelper/BouncyShroomRight" => "right",
)

function Ahorn.selection(entity::BouncyShroomUnion)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 16, y - 16, 24, 24)
end

spriteup = "JungleHelper/mushroomTemplate_up00.png"
spriteleft = "JungleHelper/mushroomTemplate_left00.png"
spriteright = "JungleHelper/mushroomTemplate_right00.png"

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BouncyShroomUp, room::Maple.Room) = Ahorn.drawSprite(ctx, spriteup, -4, -4)
Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BouncyShroomLeft, room::Maple.Room) = Ahorn.drawSprite(ctx, spriteleft, -4, -4)
Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BouncyShroomRight, room::Maple.Room) = Ahorn.drawSprite(ctx, spriteright, -4, -4)

end