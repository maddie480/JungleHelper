module JungleHelperSnake

using ..Ahorn, Maple
@pardef Snake(x1::Integer, y1::Integer, x2::Integer=x1, y2::Integer=y1+ 16, left::Bool=false, sprite::String="") = Entity("JungleHelper/Snake", x=x1, y=y1, nodes=Tuple{Int, Int}[(x2, y2)], left=left, sprite=sprite)
    
const placements = Ahorn.PlacementDict(
    "Snake (Jungle Helper)" => Ahorn.EntityPlacement(
        Snake,
        "rectangle",
        Dict{String, Any}(),
        function(entity)
            x, y = Int(entity.data["x"]), Int(entity.data["y"])
            entity.data["x"], entity.data["y"] = x + 64, y
            entity.data["nodes"] = [(x, y)]
        end
    ),
    "Snake (Facing Left) (Jungle Helper)" => Ahorn.EntityPlacement(
        Snake,
        "rectangle",
        Dict{String, Any}(
            "left" => true,
        ),
        function(entity)
            x, y = Int(entity.data["x"]), Int(entity.data["y"])
            entity.data["x"], entity.data["y"] = x + 64, y
            entity.data["nodes"] = [(x, y)]
        end
    )
)

Ahorn.nodeLimits(entity::Snake) = 1, 1

const sprite = "JungleHelper/Snake/IdleAggro/snake_idle00"

function Ahorn.selection(entity::Snake)
    x, y = Ahorn.position(entity)
    nx, ny = Int.(entity.nodes[1])
    
    return Ahorn.Rectangle[Ahorn.Rectangle(x, y + 8, 64, 8), Ahorn.Rectangle(nx, ny + 8, 64, 8)]
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::Snake)
    px, py = Ahorn.position(entity)
    nx, ny = Int.(entity.nodes[1])

    if get(entity.data, "left", false)
        Ahorn.drawSprite(ctx, sprite, nx + 64, ny, sx=-1, jx=0, jy=0)
    else
        Ahorn.drawSprite(ctx, sprite, nx, ny, jx=0, jy=0)
    end
    
	Ahorn.drawArrow(ctx, px + 32, py + 12, nx + 32, ny + 12, Ahorn.colors.selection_selected_fc, headLength=6)
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::Snake, room::Maple.Room)
    x, y = Ahorn.position(entity)
    
    if get(entity.data, "left", false)
        Ahorn.drawSprite(ctx, sprite, x + 64, y, sx=-1, jx=0, jy=0)
    else
        Ahorn.drawSprite(ctx, sprite, x, y, jx=0, jy=0)
    end
end

end
