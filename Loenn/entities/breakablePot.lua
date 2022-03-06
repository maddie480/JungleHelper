local utils = require("utils")

local breakablePot = {}
breakablePot.name = "JungleHelper/BreakablePot"
breakablePot.depth = 1
breakablePot.placements = {
    {
        name = "default",
        data = {
            sprite = "",
            rupeeImage = "JungleHelper/Breakable Pot/rupee",
            containsKey = false,
        }
    }
}

breakablePot.offset = { 12, 21 }
breakablePot.texture = "JungleHelper/Breakable Pot/breakpotidle"

function breakablePot.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 20, 17, 20)
end

return breakablePot
