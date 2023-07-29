module JungleHelperMossyWall

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/MossyWall" MossyWall(x::Integer, y::Integer, height::Integer=8, left::Bool=false,
    spriteDirectory::String="JungleHelper/Moss", color1::String="7A612D", color2::String="AABF3D", color3::String="33C111")

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

Ahorn.editingOptions(entity::MossyWall) = Dict{String, Any}(
    "spriteDirectory" => String["JungleHelper/Moss", "JungleHelper/MossInvis"]
)

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

    spriteDirectory = get(entity.data, "spriteDirectory", "JungleHelper/Moss")
    mossColor = Ahorn.argb32ToRGBATuple(parse(Int, get(entity.data, "color3", "33C111"), base=16)) ./ 255

    if left
        for i in 2:tileHeight - 1
            Ahorn.drawImage(ctx, "$(spriteDirectory)/moss_mid1", 8, (i - 1) * 8, tint=mossColor)
        end

        Ahorn.drawImage(ctx, "$(spriteDirectory)/moss_top", 8, 0, tint=mossColor)
        Ahorn.drawImage(ctx, "$(spriteDirectory)/moss_bottom", 8, (tileHeight - 1) * 8, tint=mossColor)

    else
        Ahorn.Cairo.save(ctx)
        Ahorn.scale(ctx, -1, 1)

        for i in 2:tileHeight - 1
            Ahorn.drawImage(ctx, "$(spriteDirectory)/moss_mid1", 0, (i - 1) * 8, tint=mossColor)
        end

        Ahorn.drawImage(ctx, "$(spriteDirectory)/moss_top", 0, 0, tint=mossColor)
        Ahorn.drawImage(ctx, "$(spriteDirectory)/moss_bottom", 0, (tileHeight - 1) * 8, tint=mossColor)

        Ahorn.restore(ctx)
    end
end

end
