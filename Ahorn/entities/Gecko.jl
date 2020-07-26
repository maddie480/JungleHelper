module JungleHelperGecko

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/Gecko" Gecco(x::Integer, y::Integer,birdId::String="", onlyOnce::Bool=false, info::String="TUTORIAL_CLIMB", controls::String="Climb", hostile::Bool=false,left::Bool=false, showTutorial::Bool=false, range::Number=20.0, delay::Number=0.5)
const placements = Ahorn.PlacementDict(
    "Gecko (Jungle Helper)" => Ahorn.EntityPlacement(
        Gecco,
        "rectangle"

    ),
    "Gecko (Jungle Helper) (Left)" => Ahorn.EntityPlacement(
        Gecco,
        "rectangle",
        Dict{String, Any}(
            "left" => true,
        )

    )
)
sprite = "objects/gecko/hostile/idle00"
function Ahorn.selection(entity::Gecco)
    
    sprite = (get(entity.data, "hostile", false) ? "objects/gecko/hostile/idle00" : "objects/gecko/normal/idle00")
    scaleX = (get(entity.data, "left", false) ? 1 : -1)
    x, y = Ahorn.position(entity)
    x -= 8
    if get(entity.data, "left", false)
        res = Ahorn.Rectangle(x,y,12,24)
    else
        res = Ahorn.Rectangle(x+4,y,12,24)
    end
    return res
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::Gecco)
    px, py = Ahorn.position(entity)
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::Gecco, room::Maple.Room)
    sprite = (get(entity.data, "hostile", false) ? "objects/gecko/hostile/idle00" : "objects/gecko/normal/idle00")
    scaleX = (get(entity.data, "left", false) ? 1 : -1)
    x, y = Ahorn.position(entity)
    x -= 8
    if get(entity.data, "left", false)
        Ahorn.drawSprite(ctx, sprite, x+34, y+8, sx=scaleX, rot = pi *0.5)
    else
        Ahorn.drawSprite(ctx, sprite, x-18, y+8, sx=scaleX, rot = pi *0.5)
    end
end

end