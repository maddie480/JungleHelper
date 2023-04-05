local utils = require("utils")

local cockatiel = {}
cockatiel.name = "JungleHelper/Cockatiel"
cockatiel.depth = -9999
cockatiel.placements = {
    {
        name = "default",
        data = {
            facingLeft = true,
            sprite = "",
        }
    }
}

function cockatiel.scale(room, entity)
    local scaleX = entity.facingLeft and 1 or -1
    return scaleX, 1
end

function cockatiel.selection(room, entity)
    return utils.rectangle(entity.x - 6, entity.y - 4, 12, 12)
end

cockatiel.offset = { 6, 4 }
cockatiel.texture = "JungleHelper/Cockatiel/idle00"

return cockatiel
