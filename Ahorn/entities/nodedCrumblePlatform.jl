module JungleHelperNodedCrumblePlatform

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/NodedCrumblePlatform" NodedCrumblePlatform(x::Integer, y::Integer, width::Integer=Maple.defaultBlockWidth, texture::String="default", betweenWaitTime::Number="1.0", betweenMoveTime::Number="1.0",
    nodes::Array{Tuple{Integer, Integer}, 1}=Tuple{Integer, Integer}[])

const placements = Ahorn.PlacementDict(
    "Crumble Block (Noded, $(uppercasefirst(texture))) (Jungle Helper)" => Ahorn.EntityPlacement(
        NodedCrumblePlatform,
        "rectangle",
        Dict{String, Any}(
            "texture" => texture,
            "betweenWaitTime" => 1.0,
            "betweenMoveTime" => 1.0
        )
    ) for texture in Maple.crumble_block_textures
)

Ahorn.editingOptions(entity::NodedCrumblePlatform) = Dict{String, Any}(
    "texture" => Maple.crumble_block_textures
)

Ahorn.nodeLimits(entity::NodedCrumblePlatform) = 0, -1

Ahorn.minimumSize(entity::NodedCrumblePlatform) = 8, 0
Ahorn.resizable(entity::NodedCrumblePlatform) = true, false

function Ahorn.selection(entity::NodedCrumblePlatform)
    nodes = get(entity.data, "nodes", ())
    x, y = Ahorn.position(entity)
    width = Int(get(entity.data, "width", 8))

    res = Ahorn.Rectangle[Ahorn.Rectangle(x, y, width, 8)]

    for node in nodes
        nx, ny = Int.(node)

        push!(res, Ahorn.Rectangle(nx, ny, width, 8))
    end

    return res
end

function renderCrumbleBlock(ctx::Ahorn.Cairo.CairoContext, x::Int, y::Int, width::Int, texture::String, alpha::Number=1.0)
    tilesWidth = div(width, 8)

    for i in 0:floor(Int, tilesWidth / 4)
        Ahorn.drawImage(ctx, texture, x + 32 * i, y, 0, 0, min(32, width - 32 * i), 8; alpha=alpha)
    end
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::NodedCrumblePlatform)
    texture = get(entity.data, "texture", "default")
    texture = "objects/crumbleBlock/$texture"

    x, y = Ahorn.position(entity)
    px, py = Ahorn.position(entity)
    width = Int(get(entity.data, "width", 8))

    for node in get(entity.data, "nodes", ())
        nx, ny = Int.(node)
        Ahorn.drawArrow(ctx, px + width / 2, py + 4, nx + width / 2, ny + 4, Ahorn.colors.selection_selected_fc, headLength=6)
        px, py = nx, ny
    end

    Ahorn.drawArrow(ctx, px + width / 2, py + 4, x + width / 2, y + 4, Ahorn.colors.selection_selected_fc, headLength=6)
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::NodedCrumblePlatform, room::Maple.Room)
    texture = get(entity.data, "texture", "default")
    texture = "objects/crumbleBlock/$texture"

    # Values need to be system specific integer
    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    width = Int(get(entity.data, "width", 8))

    renderCrumbleBlock(ctx, x, y, width, texture)

    for node in get(entity.data, "nodes", ())
        nx, ny = Int.(node)
        renderCrumbleBlock(ctx, nx, ny, width, texture, 0.5)
    end
end

end
