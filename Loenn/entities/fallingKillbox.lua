local utils = require("utils")

local killbox = {}

killbox.name = "JungleHelper/FallingKillbox"
killbox.color = {0.8, 0.4, 0.4, 0.8}
killbox.canResize = {true, false}
killbox.placements = {
    name = "killbox",
    data = {
        width = 8,
        fallSpeed = 100.0
    }
}

function killbox.rectangle(room, entity)
    return utils.rectangle(entity.x, entity.y, entity.width or 8, 32)
end

return killbox
