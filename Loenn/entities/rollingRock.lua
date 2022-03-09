local utils = require("utils")

local rollingRock = {}
rollingRock.name = "JungleHelper/RollingRock"

rollingRock.placements = {
    {
        name = "default",
        data = {
            cracked = false,
            spriteXmlName = "",
            debrisSpriteDirectory = "JungleHelper/RollingRock",
            flag = "",
            rollingSpeed = 100.0,
            fallingSpeed = 200.0,
            instantFalling = false
        }
    }
}

function rollingRock.texture(room, entity)
    return "JungleHelper/RollingRock/" .. (entity.cracked and "boulder_cracked" or "boulder")
end

return rollingRock
