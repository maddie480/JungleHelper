module JungleHelperRemoteKevins

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/RemoteKevin" RemoteKevin(x::Integer, y::Integer, restrained::Bool=false)

const placements = Ahorn.PlacementDict(
    "Slide Block (Restraintless, Jungle Helper)" => Ahorn.EntityPlacement(
        RemoteKevin,
        "rectangle",
        Dict{String, Any}(
            "restrained" => false
        )
    ),
    "Slide Block (Restrained, Jungle Helper)" => Ahorn.EntityPlacement(
        RemoteKevin,
        "rectangle",
        Dict{String, Any}(
            "restrained" => true
        )
    )
)

kevinColor = (138, 156, 96) ./ 255
Ahorn.minimumSize(entity::RemoteKevin) = 24, 24
Ahorn.resizable(entity::RemoteKevin) = true, true

Ahorn.selection(entity::RemoteKevin) = Ahorn.getEntityRectangle(entity)

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RemoteKevin, room::Maple.Room)
    restrainedTex = (get(entity.data, "restrained", false) ? "objects/slideBlock/green" : "objects/slideBlock/red")
    frame = string(restrainedTex, "/block03")
    faceSprite = Ahorn.getSprite("objects/slideBlock/green/idle_face")

    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    tilesWidth = div(width, 8)
    tilesHeight = div(height, 8)

    Ahorn.drawRectangle(ctx, 2, 2, width - 4, height - 4, kevinColor)
    Ahorn.drawImage(ctx, faceSprite, div(width - faceSprite.width, 2), div(height - faceSprite.height, 2))

    for i in 2:tilesWidth - 1
        Ahorn.drawImage(ctx, frame, (i - 1) * 8, 0, 8, 0, 8, 8)
        Ahorn.drawImage(ctx, frame, (i - 1) * 8, height - 8, 8, 24, 8, 8)
    end

    for i in 2:tilesHeight - 1
        Ahorn.drawImage(ctx, frame, 0, (i - 1) * 8, 0, 8, 8, 8)
        Ahorn.drawImage(ctx, frame, width - 8, (i - 1) * 8, 24, 8, 8, 8)
    end

    Ahorn.drawImage(ctx, frame, 0, 0, 0, 0, 8, 8)
    Ahorn.drawImage(ctx, frame, width - 8, 0, 24, 0, 8, 8)
    Ahorn.drawImage(ctx, frame, 0, height - 8, 0, 24, 8, 8)
    Ahorn.drawImage(ctx, frame, width - 8, height - 8, 24, 24, 8, 8)
end

end