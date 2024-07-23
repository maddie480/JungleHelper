local utils = require("utils")

local bouncyShroomUp = {}
bouncyShroomUp.name = "JungleHelper/BouncyShroomUp"
bouncyShroomUp.depth = -1
bouncyShroomUp.placements = {
    {
        name = "default",
        data = {
            yeetx = 200,
            yeety = -290,
            spriteDirectory = "JungleHelper/BouncyShroom",
            dashCount = "",
            triggerBlocks = false
        }
    }
}
bouncyShroomUp.offset = { 20, 20 }
bouncyShroomUp.justification = { 0, 0 }
function bouncyShroomUp.texture(room, entity)
    return (entity.spriteDirectory or "JungleHelper/BouncyShroom") .. "/mushroom00"
end
function bouncyShroomUp.selection(room, entity)
    return utils.rectangle(entity.x - 16, entity.y - 16, 24, 24)
end
bouncyShroomUp.fieldInformation = {
    spriteDirectory = {
        options = { "JungleHelper/BouncyShroom", "JungleHelper/BouncyShroomOutline" }
    }
}

local bouncyShroomLeft = {}
bouncyShroomLeft.name = "JungleHelper/BouncyShroomLeft"
bouncyShroomLeft.depth = -1
bouncyShroomLeft.placements = {
    {
        name = "default",
        data = {
            yeetx = 200,
            yeety = -290,
            spriteDirectory = "JungleHelper/BouncyShroom",
            dashCount = "",
            triggerBlocks = false
        }
    }
}
bouncyShroomLeft.offset = { 20, 20 }
bouncyShroomLeft.justification = { 0, 0 }
function bouncyShroomLeft.texture(room, entity)
    return (entity.spriteDirectory or "JungleHelper/BouncyShroom") .. "/mushroom_ld_00"
end
function bouncyShroomLeft.selection(room, entity)
    return utils.rectangle(entity.x - 16, entity.y - 16, 24, 24)
end
bouncyShroomLeft.fieldInformation = {
    spriteDirectory = {
        options = { "JungleHelper/BouncyShroom", "JungleHelper/BouncyShroomOutline" }
    }
}

local bouncyShroomRight = {}
bouncyShroomRight.name = "JungleHelper/BouncyShroomRight"
bouncyShroomRight.depth = -1
bouncyShroomRight.placements = {
    {
        name = "default",
        data = {
            yeetx = 200,
            yeety = -290,
            spriteDirectory = "JungleHelper/BouncyShroom",
            dashCount = "",
            triggerBlocks = false
        }
    }
}
bouncyShroomRight.offset = { 20, 20 }
bouncyShroomRight.justification = { 0, 0 }
function bouncyShroomRight.texture(room, entity)
    return (entity.spriteDirectory or "JungleHelper/BouncyShroom") .. "/mushroom_rd_00"
end
function bouncyShroomRight.selection(room, entity)
    return utils.rectangle(entity.x - 16, entity.y - 16, 24, 24)
end
bouncyShroomRight.fieldInformation = {
    spriteDirectory = {
        options = { "JungleHelper/BouncyShroom", "JungleHelper/BouncyShroomOutline" }
    }
}

return { bouncyShroomUp, bouncyShroomLeft, bouncyShroomRight }
