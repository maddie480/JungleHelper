local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableSprite = require("structs.drawable_sprite")

local kevin = {}

local axesOptions = {
    Both = "both",
    Vertical = "vertical",
    Horizontal = "horizontal"
}

kevin.name = "JungleHelper/RemoteKevin"
kevin.depth = 0
kevin.minimumSize = {24, 24}

kevin.fieldInformation = {
    axes = {
        options = axesOptions,
        editable = false
    }
}

kevin.placements = {
    {
        name = "restrained",
        data = {
            width = 24,
            height = 24,
            restrained = true,
            axes = "both",
            spriteXmlName = "",
            spriteDirectory = "",
            infiniteCharges = false,
            ignoreJumpthrus = false
        }
    },
    {
        name = "restraintless",
        data = {
            width = 24,
            height = 24,
            restrained = false,
            axes = "both",
            spriteXmlName = "",
            spriteDirectory = "",
            infiniteCharges = false,
            ignoreJumpthrus = false
        }
    },
}


local ninePatchOptions = {
    mode = "border",
    borderMode = "repeat"
}


function kevin.sprite(room, entity)
    local kevinColor = {138 / 255, 156 / 255, 96 / 255}
    local baseDir = entity.restrained and "JungleHelper/SlideBlockGreen" or "JungleHelper/SlideBlockRed"
    local smallFaceTexture = baseDir .. "/small_active_up00"
    local giantFaceTexture = baseDir .. "/big_active_up00"

    local frameTextures = {
        none = baseDir .. "/block00",
        horizontal = baseDir .. "/block01",
        vertical = baseDir .. "/block02",
        both = baseDir .. "/block03"
    }

    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24

    local axes = entity.axes or "both"
    local chillout = entity.chillout

    local giant = height >= 48 and width >= 48 and chillout
    local faceTexture = giant and giantFaceTexture or smallFaceTexture

    local frameTexture = frameTextures[axes] or frameTextures["both"]
    local ninePatch = drawableNinePatch.fromTexture(frameTexture, ninePatchOptions, x, y, width, height)

    local rectangle = drawableRectangle.fromRectangle("fill", x + 2, y + 2, width - 4, height - 4, kevinColor)
    local faceSprite = drawableSprite.fromTexture(faceTexture, entity)

    faceSprite:addPosition(math.floor(width / 2), math.floor(height / 2))

    local sprites = ninePatch:getDrawableSprite()

    table.insert(sprites, 1, rectangle:getDrawableSprite())
    table.insert(sprites, 2, faceSprite)

    return sprites
end

return kevin
