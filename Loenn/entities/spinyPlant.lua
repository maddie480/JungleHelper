local drawableSpriteStruct = require("structs.drawable_sprite")
local drawing = require("utils.drawing")
local utils = require("utils")
local enums = require("consts.celeste_enums")

local function getTexture(entity)
    return entity.texture and entity.texture ~= "default" and entity.texture or "wood"
end

local plant = {}

plant.name = "JungleHelper/SpinyPlant"
plant.depth = -9500
plant.canResize = {false, true}
plant.minimumSize = { 0, 16 }

local colors = { "Blue", "Pink", "Yellow", "Orange" }

plant.placements = {}

for _, color in ipairs(colors) do
    table.insert(plant.placements, {
        name = color,
        data = {
            height = 16,
            color = color,
            sprite = ""
        },
    })
end

plant.fieldInformation = {
    color = {
        options = colors,
        editable = false
    }
}

function plant.sprite(room, entity)
    local textureRaw = getTexture(entity)
    local texture = "objects/plant/" .. textureRaw

    local x, y = entity.x or 0, entity.y or 0
    local height = entity.height or 8

    local startX, startY = math.floor(x / 8) + 1, math.floor(y / 8) + 1
    local stopY = startY + math.floor(height / 8) - 1
    local len = stopY - startY

    local sprites = {}

    for i = 0, len, 2 do
        local topOpen = room.tilesFg.matrix:get(startX + 1, startY - 1, "0") == "0"
        local bottomOpen = room.tilesFg.matrix:get(startX + 1, stopY + 1, "0") == "0"

        local section = ""
        if len == 1 then
            if topOpen then
                if bottomOpen then
                    section = "Solo"
                else
                    section = "Top"
                end
            else
                if bottomOpen then
                    section = "Bottom"
                else
                    section = "Mid"
                end
            end
        else
            if i == len then
                i = len - 1
            end

            section = "Mid"
            if i == 0 and topOpen then
                section = "Top"
            elseif i == len - 1 and bottomOpen then
                section = "Bottom"
            end
        end

        -- We cannot pass the entity to drawableSprite.fromTexture, because otherwise its "color" option is going to be interpreted as tinting. :a:
        local sprite = drawableSpriteStruct.fromTexture("JungleHelper/SpinyPlant/Spiny" .. entity.color .. section, {x = entity.x, y = entity.y})

        sprite:setJustification(0, 0)
        sprite:addPosition(section == "Solo" and 4 or 0, i * 8)
        table.insert(sprites, sprite)
    end

    return sprites
end

function plant.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, 24, entity.height)
end

return plant
