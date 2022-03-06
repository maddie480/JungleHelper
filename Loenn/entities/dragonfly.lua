local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local dragonfly = {}
dragonfly.name = "JungleHelper/Dragonfly"
dragonfly.depth = 5000
dragonfly.placements = {
    {
        name = "default",
        data = {
            wingsColor = "FFFFFF",
            sprite = "",
        }
    }
}

dragonfly.fieldInformation = {
    wingsColor = {
        fieldType = "color",
        allowXNAColors = true,
    }
}

function dragonfly.sprite(room, entity, viewport)
    local body = drawableSprite.fromTexture("JungleHelper/Dragonfly/dragonfly_body00", entity)
    local wings = drawableSprite.fromTexture("JungleHelper/Dragonfly/dragonfly_wings00", entity)

    body:setJustification(0, 0)
    wings:setJustification(0, 0)

    local success, r, g, b = utils.parseHexColor(entity.wingsColor)
    local color = success and { r, g, b } or { 1, 1, 1 }
    wings:setColor(color)

    return { body, wings }
end

return dragonfly
