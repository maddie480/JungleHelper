local drawableNinePatch = require("structs.drawable_nine_patch")
local utils = require("utils")

local crumbleBlock = {}

local textures = {
    "default", "cliffside"
}

crumbleBlock.name = "JungleHelper/NodedCrumblePlatform"
crumbleBlock.nodeLimits = { 0, -1 }
crumbleBlock.nodeLineRenderType = "line"
crumbleBlock.nodeVisibility = "always"
crumbleBlock.depth = 0
crumbleBlock.fieldInformation = {
    texture = {
        options = textures,
    }
}
crumbleBlock.placements = {}

for _, texture in ipairs(textures) do
    table.insert(crumbleBlock.placements, {
        name = texture,
        data = {
            width = 8,
            texture = texture
        }
    })
end

local ninePatchOptions = {
    mode = "fill",
    fillMode = "repeat",
    border = 0
}

local function generateSprite(entity, position)
    local x, y = position.x, position.y
    local width = math.max(entity.width or 0, 8)

    local variant = entity.texture or "default"
    local texture = "objects/crumbleBlock/" .. variant
    local ninePatch = drawableNinePatch.fromTexture(texture, ninePatchOptions, x, y, width, 8)

    return ninePatch
end

function crumbleBlock.sprite(room, entity)
    return generateSprite(entity, entity)
end

function crumbleBlock.nodeSprite(room, entity, node)
    return generateSprite(entity, node)
end

function crumbleBlock.selection(room, entity)
    local mainRectangle = utils.rectangle(entity.x, entity.y, entity.width, 8)

    local nodeRectangles = {}

    for i, node in ipairs(entity.nodes or {}) do
        nodeRectangles[i] = utils.rectangle(node.x, node.y, entity.width, 8)
    end

    return mainRectangle, nodeRectangles
end

return crumbleBlock
