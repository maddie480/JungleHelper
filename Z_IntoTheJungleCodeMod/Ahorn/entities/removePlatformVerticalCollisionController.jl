module IntoTheJungleCodeModRemovePlatformVerticalCollisionController

using ..Ahorn, Maple

@mapdef Entity "IntoTheJungleCodeMod/RemovePlatformVerticalCollisionController" RemovePlatformVerticalCollisionController(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Remove Platform Vertical Collision Controller\n(Into The Jungle code mod)" => Ahorn.EntityPlacement(
        RemovePlatformVerticalCollisionController
    )
)

function Ahorn.selection(entity::RemovePlatformVerticalCollisionController)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RemovePlatformVerticalCollisionController, room::Maple.Room) = Ahorn.drawImage(ctx, Ahorn.Assets.northernLights, -12, -12)

end
