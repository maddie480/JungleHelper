module JungleHelperMossyWall

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/MossyWall" MossyWall(x::Integer, y::Integer, height::Integer=8, left::Bool=false)

const placements = Ahorn.PlacementDict(
    "Mossy Wall (Right) (Jungle Helper)" => Ahorn.EntityPlacement(
        MossyWall,
        "rectangle",
        Dict{String, Any}(
            "left" => true
        )
    ),
    "Mossy Wall (Left) (Jungle Helper)" => Ahorn.EntityPlacement(
        MossyWall,
        "rectangle",
        Dict{String, Any}(
            "left" => false
        )
    )
)

Ahorn.minimumSize(entity::MossyWall) = 0, 8
Ahorn.resizable(entity::MossyWall) = false, true

function Ahorn.selection(entity::MossyWall)
    x, y = Ahorn.position(entity)
    height = Int(get(entity.data, "height", 8))

    return Ahorn.Rectangle(x, y, 8, height)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MossyWall, room::Maple.Room)
    left = get(entity.data, "left", false)

    # Values need to be system specific integer
    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    height = Int(get(entity.data, "height", 8))
    tileHeight = div(height, 8)

    if left
        for i in 2:tileHeight - 1
            Ahorn.drawImage(ctx, "JungleHelper/Moss/mossMid", 8, (i - 1) * 8)
        end

        Ahorn.drawImage(ctx, "JungleHelper/Moss/mossTop", 8, 0)
        Ahorn.drawImage(ctx, "JungleHelper/Moss/mossBottom", 8, (tileHeight - 1) * 8)

    else
        Ahorn.Cairo.save(ctx)
        Ahorn.scale(ctx, -1, 1)

        for i in 2:tileHeight - 1
            Ahorn.drawImage(ctx, "JungleHelper/Moss/mossMid", 0, (i - 1) * 8)
        end

        Ahorn.drawImage(ctx, "JungleHelper/Moss/mossTop", 0, 0)
        Ahorn.drawImage(ctx, "JungleHelper/Moss/mossBottom", 0, (tileHeight - 1) * 8)

        Ahorn.restore(ctx)
    end
end

end
