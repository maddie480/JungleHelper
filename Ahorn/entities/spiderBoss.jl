module JungleHelperSpiderBoss

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/SpiderBoss" SpiderBoss(x::Integer, y::Integer, color::String="Blue", sprite::String="", webSprite::String="", flag::String="")

const bossColors = String["Blue", "Purple", "Red"]

const bossSprites = Dict{String, String}(
	"Blue" => "JungleHelper/SpiderBoss/spider_b_00",
	"Purple" => "JungleHelper/SpiderBoss/spider_p_00",
	"Red" => "JungleHelper/SpiderBoss/spider_r_00"
)

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
    return Ahorn.Rectangle(x - 9, y - 9, 17, 17)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SpiderBoss, room::Maple.Room)
	color = get(entity.data, "color", "Blue")
	Ahorn.drawSprite(ctx, bossSprites[color], 0, 0)
end

end
