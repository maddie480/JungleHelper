module JungleHelperEnforceSkinController

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/EnforceSkinController" EnforceSkinController(x::Integer, y::Integer, showPostcard::Bool=true)

const placements = Ahorn.PlacementDict(
    "Enforce Skin Controller (Jungle Helper)" => Ahorn.EntityPlacement(
        EnforceSkinController
    )
)

function Ahorn.selection(entity::EnforceSkinController)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::EnforceSkinController, room::Maple.Room) = Ahorn.drawImage(ctx, "ahorn/JungleHelper/enforce_skin_controller", -12, -12)

end
