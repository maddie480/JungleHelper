local fakeTilesHelper = require("helpers.fake_tiles")

local fallingBlock = {}

fallingBlock.name = "JungleHelper/AutoFallingBlockDelayed"
fallingBlock.placements = {
    name = "falling_block",
    data = {
        tiletype = "3",
        delay = 2.0,
        ShakeDelay = 0.5,
        silent = false,
        width = 8,
        height = 8
    }
}

fallingBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", false)
fallingBlock.fieldInformation = fakeTilesHelper.getFieldInformation("tiletype")

return fallingBlock
