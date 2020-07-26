module JungleHelperPredatorPlant

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/PredatorPlant" PredatorPlant(x::Integer, y::Integer, color::String="Pink", facingRight::Bool=false)

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
    facingRight = get(entity.data, "facingRight", false)
    return Ahorn.Rectangle(x - (facingRight ? 18 : 6), y - 16, 24, 24)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::PredatorPlant, room::Maple.Room)
    color = get(entity.data, "color", "Pink")
    scaleX = get(entity.data, "facingRight", false) ? -1 : 1
    Ahorn.drawSprite(ctx, spritePaths[color], 0, -4, sx=scaleX)
end

end
