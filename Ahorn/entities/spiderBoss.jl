module JungleHelperSpiderBoss

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/SpiderBoss" SpiderBoss(x::Integer, y::Integer, color::String="Blue")

const bossColors = String["Blue", "Purple", "Red"]

const placements = Ahorn.PlacementDict(
	"Spider Boss ($(color)) (Jungle Helper)" => Ahorn.EntityPlacement(
		SpiderBoss,
		"point",
		Dict{String, Any}(
			"color" => color
		)
	) for color in bossColors
)

Ahorn.editingOptions(entity::SpiderBoss) = Dict{String, Any}(
	"color" => bossColors
)

function Ahorn.selection(entity::SpiderBoss)
    x, y = Ahorn.position(entity)
    return Ahorn.Rectangle(x - 8, y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SpiderBoss, room::Maple.Room)
	color = get(entity.data, "color", "Blue")
	Ahorn.drawSprite(ctx, "JungleHelper/SpiderBoss/Spider$(color)", 0, 0)
end

end
