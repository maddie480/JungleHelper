module JungleHelperTorch

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/Torch" Torch(x::Integer, y::Integer, flag::String="torch_flag", sprite::String="")

const placements = Ahorn.PlacementDict(
    "Torch (Jungle Helper)" => Ahorn.EntityPlacement(
        Torch
    )
)

function Ahorn.selection(entity::Torch)
    x, y = Ahorn.position(entity)
    return Ahorn.Rectangle(x - 9, y - 22, 17, 22)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Torch, room::Maple.Room)
    Ahorn.drawSprite(ctx, "JungleHelper/TorchNight/TorchNightOff", 0, -11)
end

end
