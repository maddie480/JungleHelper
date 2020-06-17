module JungleHelperPredatorPlant

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/PredatorPlant" PredatorPlant(x::Integer, y::Integer, color::String="Pink")

const colors = String["Pink", "Blue", "Yellow"]

const placements = Ahorn.PlacementDict(
    "Predator Plant ($(color)) (Jungle Helper)" => Ahorn.EntityPlacement(
        PredatorPlant,
        "point",
        Dict{String, Any}(
          "color" => color
        )
    ) for color in colors
)

const spritePaths = Dict{String,String}(
    "Pink" => "JungleHelper/PredatorPlant/Bitey00",
    "Blue" => "JungleHelper/PredatorPlant/Blue/BiteyB00",
    "Yellow" => "JungleHelper/PredatorPlant/Yellow/BiteyC00"
)

Ahorn.editingOptions(entity::PredatorPlant) = Dict{String, Any}(
    "color" => colors
)

function Ahorn.selection(entity::PredatorPlant)
    x, y = Ahorn.position(entity)
    return Ahorn.Rectangle(x - 6, y - 16, 24, 24)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::PredatorPlant, room::Maple.Room)
    color = get(entity.data, "color", "Pink")
    Ahorn.drawSprite(ctx, spritePaths[color], 0, -4)
end

end
