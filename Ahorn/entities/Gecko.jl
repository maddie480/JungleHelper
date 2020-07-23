module JungleHelperGecko

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/Gecko" Gecco(x::Integer, y::Integer, hostile::Bool=false,left::Bool=false, showTutorial::Bool=false, range::Number=20.0)
const placements = Ahorn.PlacementDict(
    "Gecko (Jungle Helper)" => Ahorn.EntityPlacement(
        Gecco,
        "rectangle",
        Dict{String, Any}(
            "hostile" => false,
            "showTutorial" => false,
            "left" => false,
            "range" => 20
        )

    ),
    "Gecko (Jungle Helper) (Left)" => Ahorn.EntityPlacement(
        Gecco,
        "rectangle",
        Dict{String, Any}(
            "hostile" => false,
            "showTutorial" => false,
            "left" => true,
            "range" => 20
        )

    )
)


sprite = "objects/hawk/hold03"

function Ahorn.selection(entity::Gecco)
    x, y = Ahorn.position(entity)

    res = Ahorn.Rectangle[Ahorn.getSpriteRectangle(sprite, x, y)]
    return res
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::Gecco)
    px, py = Ahorn.position(entity)
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::Gecco, room::Maple.Room)
    x, y = Ahorn.position(entity)
    Ahorn.drawSprite(ctx, sprite, x, y)
end

end