local utils = require("utils")

local theoStatue = {}
theoStatue.name = "JungleHelper/TheoStatue"
theoStatue.depth = 100
theoStatue.placements = {
    {
        name = "default",
        data = {
            sprite = "",
        }
    }
}

theoStatue.texture = "JungleHelper/TheoStatue/idle00"
theoStatue.offset = { 32, 45 }

function theoStatue.selection(room, entity)
    return utils.rectangle(entity.x - 11, entity.y - 35, 21, 43)
end

return theoStatue
