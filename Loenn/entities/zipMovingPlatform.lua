local resortPlatformHelper = require("helpers.resort_platforms")
local utils = require("utils")

local textures = {
    "default", "cliffside", "JungleHelper/jungle", "JungleHelper/night", "JungleHelper/escape"
}
local textureOptions = {}

for _, texture in ipairs(textures) do
    textureOptions[utils.titleCase(texture)] = texture
end

local movingPlatform = {}

movingPlatform.name = "JungleHelper/ZipMovingPlatform"
movingPlatform.nodeLimits = {1, 1}
movingPlatform.fieldInformation = {
    texture = {
        options = textureOptions
    }

}
movingPlatform.placements = {}

for i, texture in ipairs(textures) do
    movingPlatform.placements[i] = {
        name = texture,
        data = {
            width = 8,
            texture = texture,
            waitTimer = 0,
            noReturn = false
        }
    }
end

function movingPlatform.sprite(room, entity)
    local sprites = {}

    local x, y = entity.x or 0, entity.y or 0
    local nodes = entity.nodes or {{x = 0, y = 0}}
    local nodeX, nodeY = nodes[1].x, nodes[1].y

    resortPlatformHelper.addConnectorSprites(sprites, entity, x, y, nodeX, nodeY)
    resortPlatformHelper.addPlatformSprites(sprites, entity, entity)

    return sprites
end

function movingPlatform.nodeSprite(room, entity, node)
    return resortPlatformHelper.addPlatformSprites({}, entity, node)
end

movingPlatform.selection = resortPlatformHelper.getSelection


return movingPlatform
