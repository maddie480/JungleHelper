module JungleHelperRemoteKevins

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/RemoteKevin" RemoteKevin(x::Integer, y::Integer, restrained::Bool=false, axes::String="both", spriteXmlName::String="", spriteDirectory::String="", infiniteCharges::Bool=false, ignoreJumpthrus::Bool=false)

const placements = Ahorn.PlacementDict(
    "Slide Block (Restraintless) (Jungle Helper)" => Ahorn.EntityPlacement(
        RemoteKevin,
        "rectangle",
        Dict{String, Any}(
            "restrained" => false
        )
    ),
    "Slide Block (Restrained) (Jungle Helper)" => Ahorn.EntityPlacement(
        RemoteKevin,
        "rectangle",
        Dict{String, Any}(
            "restrained" => true
        )
    )
)

const frameImage = Dict{String, String}(
    "none" => "block00",
    "horizontal" => "block01",
    "vertical" => "block02",
    "both" => "block03"
)

Ahorn.editingOptions(entity::RemoteKevin) = Dict{String, Any}(
    "axes" => Maple.kevin_axes
)

kevinColor = (138, 156, 96) ./ 255
Ahorn.minimumSize(entity::RemoteKevin) = 24, 24
Ahorn.resizable(entity::RemoteKevin) = true, true

Ahorn.selection(entity::RemoteKevin) = Ahorn.getEntityRectangle(entity)

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RemoteKevin, room::Maple.Room)
    restrainedTex = (get(entity.data, "restrained", false) ? "JungleHelper/SlideBlockGreen" : "JungleHelper/SlideBlockRed")
    frame = string(restrainedTex, "/", frameImage[lowercase(get(entity.data, "axes", "both"))])

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))
	
    if height >= 48 && width >= 48
        faceSprite = Ahorn.getSprite(restrainedTex * "/big_active_up00")
    else
        faceSprite = Ahorn.getSprite(restrainedTex * "/small_active_up00")
    end

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