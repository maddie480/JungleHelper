local drawableNinePatch = require("structs.drawable_nine_patch")
local utils = require("utils")
local entities = require("entities")

local crumbleBlock = {}

local textures = {
    "default", "cliffside"
}

crumbleBlock.name = "JungleHelper/UnrandomizedCrumblePlatform"
crumbleBlock.depth = 0
crumbleBlock.fieldInformation = {
    texture = {
        options = textures,
    }
}

crumbleBlock.placements = {
    name = "mosaic",
    data = {
        width = 8,
        texture = "JungleHelper/mosaic"
    }
}

local ninePatchOptions = {
    mode = "fill",
    fillMode = "repeat",
    border = 0
}

function crumbleBlock.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width = math.max(entity.width or 0, 8)

    local variant = entity.texture or "default"
    local texture = "objects/crumbleBlock/" .. variant
    local ninePatch = drawableNinePatch.fromTexture(texture, ninePatchOptions, x, y, width, 8)

    return ninePatch
end

function crumbleBlock.selection(room, entity)
    return utils.rectangle(entity.x or 0, entity.y or 0, math.max(entity.width or 0, 8), 8)
end

-- add custom variations of crumble blocks
for index, name in ipairs({ "mossy", "thin", "dark" }) do
    table.insert(entities.registeredEntities.crumbleBlock.placements, {
        name = "junglehelper_" .. name,
        data = {
            width = 8,
            texture = "JungleHelper/" .. name
        }
    })
end

return crumbleBlock
