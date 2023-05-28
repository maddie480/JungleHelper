local entities = require("entities")

-- add custom variations of moving platforms
for index, name in ipairs({ "escape", "jungle", "night" }) do
    table.insert(entities.registeredEntities.sinkingPlatform.placements, {
        name = "junglehelper_" .. name,
        associatedMods = {"JungleHelper"},
        data = {
            width = 16,
            texture = "JungleHelper/" .. name
        }
    })
end

return {}
