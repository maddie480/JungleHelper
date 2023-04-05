module JungleHelperCassetteCustomPreviewMusic

using ..Ahorn, Maple

@pardef CassetteCustomPreviewMusic(x1::Integer, y1::Integer, x2::Integer=x1, y2::Integer=y1, musicEvent::String="event:/game/general/cassette_preview", musicParamName::String="remix", musicParamValue::Number=1.0) =
    Entity("JungleHelper/CassetteCustomPreviewMusic", x=x1, y=y1, nodes=Tuple{Int, Int}[(0, 0), (x2, y2)], musicEvent=musicEvent, musicParamName=musicParamName, musicParamValue=musicParamValue)

const placements = Ahorn.PlacementDict(
    "Cassette (Custom Preview Music) (Jungle Helper)" => Ahorn.EntityPlacement(
        CassetteCustomPreviewMusic,
        "point",
        Dict{String, Any}(),
        function(entity)
            entity.data["nodes"] = [
                (Int(entity.data["x"]) + 32, Int(entity.data["y"])),
                (Int(entity.data["x"]) + 64, Int(entity.data["y"]))
            ]
        end
    ),
)

Ahorn.nodeLimits(entity::CassetteCustomPreviewMusic) = 2, 2

sprite = "collectables/cassette/idle00.png"

function Ahorn.selection(entity::CassetteCustomPreviewMusic)
    x, y = Ahorn.position(entity)
    controllX, controllY = Int.(entity.data["nodes"][1])
    endX, endY = Int.(entity.data["nodes"][2])

    return [
        Ahorn.getSpriteRectangle(sprite, x, y),
        Ahorn.getSpriteRectangle(sprite, controllX, controllY),
        Ahorn.getSpriteRectangle(sprite, endX, endY)
    ]
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::CassetteCustomPreviewMusic)
    px, py = Ahorn.position(entity)
    nodes = entity.data["nodes"]

    for node in nodes
        nx, ny = Int.(node)

        Ahorn.drawArrow(ctx, px, py, nx, ny, Ahorn.colors.selection_selected_fc, headLength=6)
        Ahorn.drawSprite(ctx, sprite, nx, ny)
        px, py = nx, ny
    end
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CassetteCustomPreviewMusic, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0)

end
