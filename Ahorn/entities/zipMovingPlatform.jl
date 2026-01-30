module JungleHelperZipMovingPlatform

using ..Ahorn, Maple

@pardef MovingZipPlatform(x1::Integer, y1::Integer, x2::Integer=x1 + 16, y2::Integer=y1, width::Integer=32, texture::String="default",
    waitTimer::Number=0, cooldownTimer::Number=0.5, movementMode::String="Normal", lineEdgeColor::String="2a1923", lineInnerColor::String="160b12")
    = Entity("JungleHelper/ZipMovingPlatform", x=x1, y=y1, nodes=Tuple{Int, Int}[(x2, y2)], width=width, texture=texture,
    waitTimer=waitTimer, cooldownTimer=cooldownTimer, movementMode=movementMode, lineEdgeColor=lineEdgeColor, lineInnerColor=lineInnerColor)

const placements = Ahorn.PlacementDict()

for texture in Maple.wood_platform_textures
    placements["Platform (Zip, $(uppercasefirst(texture))) (JungleHelper)"] = Ahorn.EntityPlacement(
        MovingZipPlatform,
        "rectangle",
        Dict{String, Any}(
          "texture" => texture,
          "waitTimer" => 0,
          "noReturn" => false
        ),
        function(entity)
            x, y = Int(entity.data["x"]), Int(entity.data["y"])
            width = Int(get(entity.data, "width", 8))
            entity.data["x"], entity.data["y"] = x + width, y
            entity.data["nodes"] = [(x, y)]
        end
    )
end

placements["Platform (Zip, Jungle) (JungleHelper)"] = Ahorn.EntityPlacement(
    MovingZipPlatform,
    "rectangle",
    Dict{String, Any}(
        "texture" => "JungleHelper/jungle",
        "waitTimer" => 0,
        "noReturn" => false
    ),
    function(entity)
        x, y = Int(entity.data["x"]), Int(entity.data["y"])
        width = Int(get(entity.data, "width", 8))
        entity.data["x"], entity.data["y"] = x + width, y
        entity.data["nodes"] = [(x, y)]
    end
)

placements["Platform (Zip, Night) (JungleHelper)"] = Ahorn.EntityPlacement(
    MovingZipPlatform,
    "rectangle",
    Dict{String, Any}(
        "texture" => "JungleHelper/night",
        "waitTimer" => 0,
        "noReturn" => false
    ),
    function(entity)
        x, y = Int(entity.data["x"]), Int(entity.data["y"])
        width = Int(get(entity.data, "width", 8))
        entity.data["x"], entity.data["y"] = x + width, y
        entity.data["nodes"] = [(x, y)]
    end
)

placements["Platform (Zip, Escape) (JungleHelper)"] = Ahorn.EntityPlacement(
    MovingZipPlatform,
    "rectangle",
    Dict{String, Any}(
        "texture" => "JungleHelper/escape",
        "waitTimer" => 0,
        "noReturn" => false
    ),
    function(entity)
        x, y = Int(entity.data["x"]), Int(entity.data["y"])
        width = Int(get(entity.data, "width", 8))
        entity.data["x"], entity.data["y"] = x + width, y
        entity.data["nodes"] = [(x, y)]
    end
)

Ahorn.nodeLimits(entity::MovingZipPlatform) = 1, 1
Ahorn.resizable(entity::MovingZipPlatform) = true, false
Ahorn.minimumSize(entity::MovingZipPlatform) = 8, 0

Ahorn.editingOptions(entity::MovingZipPlatform) = Dict{String, Any}(
    "texture" => Maple.wood_platform_textures,
    "movementMode" => String["Normal", "DisabledOnReachEnd", "StopOnReachEnd"]
)

function Ahorn.selection(entity::MovingZipPlatform)
    width = Int(get(entity.data, "width", 8))
    startX, startY = Int(entity.data["x"]), Int(entity.data["y"])
    stopX, stopY = Int.(entity.data["nodes"][1])

    return [Ahorn.Rectangle(startX, startY, width, 8), Ahorn.Rectangle(stopX, stopY, width, 8)]
end

outerColor = (30, 14, 25) ./ 255
innerColor = (10, 0, 6) ./ 255

function renderConnection(ctx::Ahorn.Cairo.CairoContext, x::Number, y::Number, nx::Number, ny::Number, width::Number)
    cx, cy = x + floor(Int, width / 2), y + 4
    cnx, cny = nx + floor(Int, width / 2), ny + 4

    length = sqrt((x - nx)^2 + (y - ny)^2)
    theta = atan(cny - cy, cnx - cx)

    Ahorn.Cairo.save(ctx)

    Ahorn.translate(ctx, cx, cy)
    Ahorn.rotate(ctx, theta)

    Ahorn.setSourceColor(ctx, outerColor)
    Ahorn.set_antialias(ctx, 1)
    Ahorn.set_line_width(ctx, 3);

    Ahorn.move_to(ctx, 0, 0)
    Ahorn.line_to(ctx, length, 0)

    Ahorn.stroke(ctx)

    Ahorn.setSourceColor(ctx, innerColor)
    Ahorn.set_antialias(ctx, 1)
    Ahorn.set_line_width(ctx, 1);

    Ahorn.move_to(ctx, 0, 0)
    Ahorn.line_to(ctx, length, 0)

    Ahorn.stroke(ctx)

    Ahorn.Cairo.restore(ctx)
end

function renderPlatform(ctx::Ahorn.Cairo.CairoContext, texture::String, x::Number, y::Number, width::Number)
    tilesWidth = div(width, 8)

    for i in 2:tilesWidth - 1
      Ahorn.drawImage(ctx, "objects/woodPlatform/$texture", x + 8 * (i - 1), y, 8, 0, 8, 8)
    end

    Ahorn.drawImage(ctx, "objects/woodPlatform/$texture", x, y, 0, 0, 8, 8)
    Ahorn.drawImage(ctx, "objects/woodPlatform/$texture", x + tilesWidth * 8 - 8, y, 24, 0, 8, 8)
    Ahorn.drawImage(ctx, "objects/woodPlatform/$texture", x + floor(Int, width / 2) - 4, y, 16, 0, 8, 8)
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::MovingZipPlatform, room::Maple.Room)
    width = Int(get(entity.data, "width", 8))

    x, y = Int(entity.data["x"]), Int(entity.data["y"])
    nx, ny = Int.(entity.data["nodes"][1])


    texture = get(entity.data, "texture", "default")

    renderConnection(ctx, x, y, nx, ny, width)
    renderPlatform(ctx, texture, x, y, width)
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::MovingZipPlatform, room::Maple.Room)
    width = Int(get(entity.data, "width", 8))

    startX, startY = Int(entity.data["x"]), Int(entity.data["y"])
    stopX, stopY = Int.(entity.data["nodes"][1])

    texture = get(entity.data, "texture", "default")

    renderPlatform(ctx, texture, startX, startY, width)
    renderPlatform(ctx, texture, stopX, stopY, width)

    Ahorn.drawArrow(ctx, startX + width / 2, startY, stopX + width / 2, stopY, Ahorn.colors.selection_selected_fc, headLength=6)
end

end