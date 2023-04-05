local entities = require("entities")

-- add custom variations of moving platforms
for index, name in ipairs({ "ruins", "wood", "wooddark" }) do
    table.insert(entities.registeredEntities.spikesUp.placements, {
        name = "junglehelper_" .. name,
        data = {
            width = 8,
            ["type"] = "JungleHelper/" .. name
        }
    })
    
    table.insert(entities.registeredEntities.spikesDown.placements, {
        name = "junglehelper_" .. name,
        data = {
            width = 8,
            ["type"] = "JungleHelper/" .. name
        }
    })
    
    table.insert(entities.registeredEntities.spikesLeft.placements, {
        name = "junglehelper_" .. name,
        data = {
            height = 8,
            ["type"] = "JungleHelper/" .. name
        }
    })
    
    table.insert(entities.registeredEntities.spikesRight.placements, {
        name = "junglehelper_" .. name,
        data = {
            height = 8,
            ["type"] = "JungleHelper/" .. name
        }
    })
end

return {}
