local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local templeGate = {}

local textures = {
    default = "objects/door/TempleDoor00",
    mirror = "objects/door/TempleDoorB00",
    theo = "objects/door/TempleDoorC00"
}

local textureOptions = {}
for texture, _ in pairs(textures) do
    textureOptions[utils.titleCase(texture)] = texture
end

templeGate.name = "JungleHelper/TheoStatueGate"
templeGate.depth = -9000
templeGate.canResize = {false, false}
templeGate.fieldInformation = {
    sprite = {
        options = textureOptions,
        editable = false
    }
}
templeGate.placements = {
    name = "default",
    data = {
        height = 48,
        sprite = "default"
    }
}

function templeGate.sprite(room, entity)
    local variant = entity.sprite or "default"
    local texture = textures[variant] or textures["default"]
    local sprite = drawableSprite.fromTexture(texture, entity)

    -- Weird offset from the code, justifications are from sprites.xml
    sprite:setJustification(0.5, 0.0)
    sprite:addPosition(4, 0)

    return sprite
end

return templeGate
