local spiderBoss = {}
spiderBoss.name = "JungleHelper/SpiderBoss"
spiderBoss.depth = -20001

local colors = { "Blue", "Purple", "Red" }

local sprites = {
    Blue = "JungleHelper/SpiderBoss/spider_b_00",
    Purple = "JungleHelper/SpiderBoss/spider_p_00",
    Red = "JungleHelper/SpiderBoss/spider_r_00"
}

spiderBoss.placements = {}

for _, color in ipairs(colors) do
    table.insert(spiderBoss.placements, {
        name = color,
        data = {
            color = color,
            sprite = "",
            webSprite = "",
            flag = "",
        }
    })
end

spiderBoss.fieldInformation = {
    color = {
        options = colors,
        editable = false
    }
}

function spiderBoss.texture(room, entity)
   return sprites[entity.color] 
end

return spiderBoss
