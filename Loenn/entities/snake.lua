local utils = require("utils")
local drawableSpriteStruct = require("structs.drawable_sprite")
local celesteEnums = require("consts.celeste_enums")

local snake = {}
snake.name = "JungleHelper/Snake"
snake.placements = {
    {
        name = "right",
        data = {
            left = false,
            sprite = ""
        }
    },
    {
        name = "left",
        data = {
            left = true,
            sprite = ""
        }
    }
}

snake.nodeLineRenderType = "line"
snake.nodeLimits = { 1, 1 }

snake.texture = "JungleHelper/Snake/IdleAggro/snake_idle00"

function snake.offset(room, entity)
    return entity.left and 64 or 0, 0
end

function snake.scale(room, entity)
    return entity.left and -1 or 1, 1
end

function snake.selection(room, entity)
    return utils.rectangle(entity.x, entity.y + 8, 64, 8), { utils.rectangle(entity.nodes[1].x, entity.nodes[1].y + 8, 64, 8) }
end

function snake.move(room, entity, nodeIndex, offsetX, offsetY)
    -- move the selected node on the X axis
    if nodeIndex == 0 then
        entity.x = entity.x + offsetX
    else
        entity.nodes[1].x = entity.nodes[1].x + offsetX
    end

    -- move BOTH nodes on the Y axis: they need to stay lined up
    entity.y = entity.y + offsetY
    entity.nodes[1].y = entity.nodes[1].y + offsetY
end

return snake
