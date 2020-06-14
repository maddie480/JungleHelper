module JungleHelperRollingRock

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/RollingRock" RollingRock(x::Integer, y::Integer, sprite::String="boulder0")

const placements = Ahorn.PlacementDict(
    "Rolling Rock (Jungle Helper)" => Ahorn.EntityPlacement(
        RollingRock
    )
)

Ahorn.editingOptions(entity::RollingRock) = Dict{String, Any}(
    "sprite" => String["boulder0", "boulder1"]
)

function Ahorn.selection(entity::RollingRock)
    x, y = Ahorn.position(entity)
    spriteName = get(entity.data, "sprite", "boulder0")
    sprite = "JungleHelper/RollingRock/$(spriteName)"
    return Ahorn.getSpriteRectangle(sprite, x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RollingRock, room::Maple.Room)
    spriteName = get(entity.data, "sprite", "boulder0")
    sprite = "JungleHelper/RollingRock/$(spriteName)"
    Ahorn.drawSprite(ctx, sprite, 0, 0)
end

end
