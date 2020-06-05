module JungleHelperTheoStatue

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/TheoStatue" TheoStatue(x::Integer, y::Integer, directory::String="JungleHelper/TheoStatue")

const placements = Ahorn.PlacementDict(
    "Theo Statue (Jungle Helper)" => Ahorn.EntityPlacement(
        TheoStatue
    )
)

function Ahorn.selection(entity::TheoStatue)
    x, y = Ahorn.position(entity)
	sprite = get(entity, "directory", "JungleHelper/TheoStatue") * "/idle00"

    return Ahorn.Rectangle(x - 11, y - 35, 21, 43)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TheoStatue, room::Maple.Room)
	sprite = get(entity, "directory", "JungleHelper/TheoStatue") * "/idle00"
	
    Ahorn.drawSprite(ctx, sprite, 0, -13)
end

end
