module JungleHelperGrablessGoldenBerry

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/TreeDepthController" GrablessGoldenBerry(x::Integer, y::Integer, nodes::Array{Tuple{Integer, Integer}, 1}=Tuple{Integer, Integer}[])

const placements = Ahorn.PlacementDict(
    "Golden Strawberry (Grabless) (Jungle Helper)" => Ahorn.EntityPlacement(
        GrablessGoldenBerry,
        "point"
    )
)

const sprite = "collectables/goldberry/wings01"
const seedSprite = "collectables/goldberry/seed00"

Ahorn.nodeLimits(entity::GrablessGoldenBerry) = 0, -1

function Ahorn.selection(entity::GrablessGoldenBerry)
    x, y = Ahorn.position(entity)

    nodes = get(entity.data, "nodes", ())

    res = Ahorn.Rectangle[Ahorn.getSpriteRectangle(sprite, x, y)]
    
    for node in nodes
        nx, ny = node

        push!(res, Ahorn.getSpriteRectangle(seedSprite, nx, ny))
    end

    return res
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::GrablessGoldenBerry)
    x, y = Ahorn.position(entity)

    for node in get(entity.data, "nodes", ())
        nx, ny = node

        Ahorn.drawLines(ctx, Tuple{Number, Number}[(x, y), (nx, ny)], Ahorn.colors.selection_selected_fc)
    end
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::GrablessGoldenBerry, room::Maple.Room)
    x, y = Ahorn.position(entity)

    nodes = get(entity.data, "nodes", ())

    for node in nodes
        nx, ny = node

        Ahorn.drawSprite(ctx, seedSprite, nx, ny)
    end

    Ahorn.drawSprite(ctx, sprite, x, y)
end

end
