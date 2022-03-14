local drawableSpriteStruct = require("structs.drawable_sprite")
local drawing = require("utils.drawing")
local utils = require("utils")
local enums = require("consts.celeste_enums")

local function getTexture(entity)
    return entity.texture and entity.texture ~= "default" and entity.texture or "wood"
end

local wall = {}

wall.name = "JungleHelper/MossyWall"
wall.depth = -20000
wall.canResize = {false, true}
wall.placements = {}

wall.fieldInformation = {
    spriteDirectory = {
        options = { "JungleHelper/Moss", "JungleHelper/MossInvis" }
    }
}

wall.placements = {
    {
        name = "left",
        data = {
            height = 8,
            left = true,
            spriteDirectory = "JungleHelper/Moss"
        },
    },
    {
        name = "right",
        data = {
            height = 8,
            left = false,
            spriteDirectory = "JungleHelper/Moss"
        },
    }
}

function wall.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local height = entity.height or 8

    local startX, startY = math.floor(x / 8) + 1, math.floor(y / 8) + 1
    local stopY = startY + math.floor(height / 8) - 1
    local len = stopY - startY

    local sprites = {}

    for i = 0, len do
        local texture = entity.spriteDirectory .. "/moss_mid1"

        if i == 0 then
            texture = entity.spriteDirectory .. (entity.left and "/moss_top" or "/moss_bottom")

        elseif i == len then
            texture = entity.spriteDirectory .. (entity.left and "/moss_bottom" or "/moss_top")
        end

        local sprite = drawableSpriteStruct.fromTexture(texture, entity)

        sprite:setJustification(0, 0)
		sprite:setColor("33C111")
        sprite:addPosition(entity.left and 0 or 8, i * 8 + (entity.left and 0 or 8))
        sprite.rotation = entity.left and 0 or math.pi

        table.insert(sprites, sprite)
    end

    return sprites
end

function wall.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, 8, entity.height)
end

return wall
