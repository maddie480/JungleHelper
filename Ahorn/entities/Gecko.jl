module JungleHelperGecko

using ..Ahorn, Maple
@pardef Gecco(x1::Integer, y1::Integer, x2::Integer=x1, y2::Integer=y1+ 16,geckoId::String="", onlyOnce::Bool=false, info::String="TUTORIAL_CLIMB", controls::String="Grab", hostile::Bool=false,left::Bool=false, showTutorial::Bool=false, delay::Number=0.5) = Entity("JungleHelper/Gecko", x=x1, y=y1, nodes=Tuple{Int, Int}[(x2, y2)], geckoId=geckoId, onlyOnce=onlyOnce, info=info, controls=controls, hostile=hostile,left=left, showTutorial=showTutorial)
const placements = Ahorn.PlacementDict(
    "Gecko (Jungle Helper)" => Ahorn.EntityPlacement(
        Gecco,
        "rectangle",
        function(entity)
            x, y = Int(entity.data["x"]), Int(entity.data["y"])
            entity.data["x"], entity.data["y"] = x + width, y+25
            entity.data["nodes"] = [(x, y)]
        end

    ),
    "Gecko (Jungle Helper) (Left)" => Ahorn.EntityPlacement(
        Gecco,
        "rectangle",
        Dict{String, Any}(
            "left" => true,
        ),
        function(entity)
            x, y = Int(entity.data["x"]), Int(entity.data["y"])
            entity.data["x"], entity.data["y"] = x, y+25
            entity.data["nodes"] = [(x, y)]
        end

    )
)

Ahorn.nodeLimits(entity::Gecco) = 1, 1
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