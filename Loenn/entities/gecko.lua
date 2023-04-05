local drawableSpriteStruct = require("structs.drawable_sprite")
local celesteEnums = require("consts.celeste_enums")

local gecko = {}
gecko.name = "JungleHelper/Gecko"
gecko.placements = {
    {
        name = "right",
        data = {
            hostile = false,
            left = false,
            delay = 0.5,
            geckoId = "geckoId",
            info = "",
            controls = "",
            sprite = ""
        }
    },
    {
        name = "left",
        data = {
            hostile = false,
            left = true,
            delay = 0.5,
            geckoId = "geckoId",
            info = "",
            controls = "",
            sprite = ""
        }
    }
}

gecko.fieldInformation = {
    info = {
         options = celesteEnums.everest_bird_tutorial_tutorials
    }
}

gecko.nodeLineRenderType = "line"
gecko.nodeLimits = { 1, 1 }

function gecko.sprite(room, entity)
    local texture = entity.hostile and "JungleHelper/gecko/hostile/idle00" or "JungleHelper/gecko/normal/idle00"
    local sprite = drawableSpriteStruct.fromTexture(texture, entity)

    sprite.rotation = math.pi * 0.5
    sprite:setScale(1, entity.left and 1 or -1)
    sprite:addPosition(0, 2)

    return sprite
end

function gecko.move(room, entity, nodeIndex, offsetX, offsetY)
    -- move the selected node on the Y axis
    if nodeIndex == 0 then
        entity.y = entity.y + offsetY
    else
        entity.nodes[1].y = entity.nodes[1].y + offsetY
    end

    -- move BOTH nodes on the X axis: they need to stay lined up
    entity.x = entity.x + offsetX
    entity.nodes[1].x = entity.nodes[1].x + offsetX
end

return gecko
