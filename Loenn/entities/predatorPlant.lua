local utils = require("utils")

local predatorPlant = {}
predatorPlant.name = "JungleHelper/PredatorPlant"

local colors = { "Pink", "Blue", "Yellow" }

local spritePaths = {
    Pink = "JungleHelper/PredatorPlant/Bitey00",
    Blue = "JungleHelper/PredatorPlant/Blue/BiteyB00",
    Yellow = "JungleHelper/PredatorPlant/Yellow/BiteyC00"
}

predatorPlant.placements = {}

for _, color in ipairs(colors) do
    table.insert(predatorPlant.placements, {
        name = color,
        data = {
            color = color,
            facingRight = false,
            sprite = "",
        }
    })
end

predatorPlant.fieldInformation = {
    color = {
        options = colors,
        editable = false
    }
}

predatorPlant.offset = { 16, 16 }

function predatorPlant.texture(room, entity)
    return spritePaths[entity.color]
end

function predatorPlant.scale(room, entity)
    return entity.facingRight and -1 or 1, 1
end

function predatorPlant.selection(room, entity)
    return utils.rectangle(entity.x - (entity.facingRight and 18 or 6), entity.y - 16, 24, 24)
end

return predatorPlant
