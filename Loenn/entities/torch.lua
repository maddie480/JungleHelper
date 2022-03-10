local utils = require("utils")

local torch = {}
torch.name = "JungleHelper/Torch"
torch.placements = {
    {
        name = "default",
        data = {
            flag = "torch_flag",
            sprite = "",
        }
    }
}

torch.offset = { 16, 27 }
torch.texture = "JungleHelper/TorchNight/TorchNightOff"

function torch.selection(room, entity)
    return utils.rectangle(entity.x - 9, entity.y - 22, 17, 22)
end

return torch
