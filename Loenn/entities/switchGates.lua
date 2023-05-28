local entities = require("entities")


for index, name in ipairs({ "junglestone", "wood" }) do
    table.insert(entities.registeredEntities.switchGate.placements, {
        name = "junglehelper_" .. name,
        associatedMods = {"JungleHelper"},
        data = {
            width = 16,
            height = 16,
            sprite = "JungleHelper/" .. name
        }
    })
end

return {}