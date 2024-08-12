local utils = require("utils")

local lantern = {}
lantern.name = "JungleHelper/Lantern"
lantern.depth = 0
lantern.placements = {
    {
        name = "default",
        data = {
            sprite = "",
            lightSprite = "JungleHelper/Lantern/Overlay",
            onlyIfMaddyNotHolding = false
        }
    }
}

lantern.offset = { 10, 5 }
lantern.justification = { 0, 0 }
lantern.texture = "JungleHelper/Lantern/LanternEntity/lantern_00"

lantern.fieldOrder = {
    "x","y","sprite","lightSprite","onlyIfMaddyNotHolding"
}

function lantern.selection(room, entity)
    return utils.rectangle(entity.x - 5, entity.y - 2, 10, 10)
end

return lantern
