module JungleHelperGecko

using ..Ahorn, Maple
@pardef Gecko(x1::Integer, y1::Integer, x2::Integer=x1, y2::Integer=y1+ 16, hostile::Bool=false, left::Bool=false, delay::Number=0.5, geckoId::String="geckoId", info::String="", controls::String="", sprite::String="") =
    Entity("JungleHelper/Gecko", x=x1, y=y1, nodes=Tuple{Int, Int}[(x2, y2)], hostile=hostile, left=left, delay=delay, geckoId=geckoId, info=info, controls=controls, sprite=sprite)
    
const placements = Ahorn.PlacementDict(
    "Gecko (Jungle Helper)" => Ahorn.EntityPlacement(
        Gecko,
        "rectangle",
        Dict{String, Any}(),
        function(entity)
            x, y = Int(entity.data["x"]), Int(entity.data["y"])
            entity.data["x"], entity.data["y"] = x, y + 24
            entity.data["nodes"] = [(x, y)]
        end
    ),
    "Gecko (Left) (Jungle Helper)" => Ahorn.EntityPlacement(
        Gecko,
        "rectangle",
        Dict{String, Any}(
            "left" => true,
        ),
        function(entity)
            x, y = Int(entity.data["x"]), Int(entity.data["y"])
            entity.data["x"], entity.data["y"] = x, y + 24
            entity.data["nodes"] = [(x, y)]
        end

    )
)

Ahorn.nodeLimits(entity::Gecko) = 1, 1

Ahorn.editingOptions(entity::Gecko) = return Dict{String, Any}(
    "info" => Maple.everest_bird_tutorial_tutorials
)

function Ahorn.selection(entity::Gecko)
    sprite = (get(entity.data, "hostile", false) ? "JungleHelper/gecko/hostile/idle00" : "JungleHelper/gecko/normal/idle00")
    x, y = Ahorn.position(entity)
    nx, ny = Int.(entity.nodes[1])
    
    if get(entity.data, "left", false)
        res = Ahorn.Rectangle[Ahorn.Rectangle(x - 8, y - 16, 12, 24), Ahorn.Rectangle(nx - 8, ny - 16, 12, 24)]
    else
        res = Ahorn.Rectangle[Ahorn.Rectangle(x - 4, y - 16, 12, 24), Ahorn.Rectangle(nx - 4, ny - 16, 12, 24)]
    end
    return res
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::Gecko)
    sprite = (get(entity.data, "hostile", false) ? "JungleHelper/gecko/hostile/idle00" : "JungleHelper/gecko/normal/idle00")
    scaleX = (get(entity.data, "left", false) ? 1 : -1)
    px, py = Ahorn.position(entity)
    nx, ny = Int.(entity.nodes[1])

    # align node vertically with entity
    entity.nodes[1] = (px, ny)
    nx = px

    if get(entity.data, "left", false)
        Ahorn.drawSprite(ctx, sprite, nx + 26, ny - 8, sx = scaleX, rot = pi * 0.5)
    else
        Ahorn.drawSprite(ctx, sprite, nx - 26, ny - 8, sx = scaleX, rot = pi * 0.5)
    end
    
    if get(entity.data, "left", false)
        Ahorn.drawArrow(ctx, px - 1, py + 4, nx - 1, ny - 4, Ahorn.colors.selection_selected_fc, headLength=6)
    else
        Ahorn.drawArrow(ctx, px + 3, py + 4, nx + 3, ny - 4, Ahorn.colors.selection_selected_fc, headLength=6)
    end
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::Gecko, room::Maple.Room)
    sprite = (get(entity.data, "hostile", false) ? "JungleHelper/gecko/hostile/idle00" : "JungleHelper/gecko/normal/idle00")
    scaleX = (get(entity.data, "left", false) ? 1 : -1)
    x, y = Ahorn.position(entity)
    
    if get(entity.data, "left", false)
        Ahorn.drawSprite(ctx, sprite, x + 26, y - 8, sx = scaleX, rot = pi * 0.5)
    else
        Ahorn.drawSprite(ctx, sprite, x - 26, y - 8, sx = scaleX, rot = pi * 0.5)
    end
end

end