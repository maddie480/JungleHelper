module IntoTheJungleCodeModSetColorGradeIfFlagOnSpawnController

using ..Ahorn, Maple

@mapdef Entity "IntoTheJungleCodeMod/SetColorGradeIfFlagOnSpawnController" SetColorGradeIfFlagOnSpawnController(x::Integer, y::Integer, flag::String="", colorGrade::String="")

const placements = Ahorn.PlacementDict(
    "Set Color Grade If Flag On Spawn Controller\n(Into The Jungle code mod)" => Ahorn.EntityPlacement(
        SetColorGradeIfFlagOnSpawnController
    )
)

function Ahorn.selection(entity::SetColorGradeIfFlagOnSpawnController)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SetColorGradeIfFlagOnSpawnController, room::Maple.Room) = Ahorn.drawImage(ctx, Ahorn.Assets.northernLights, -12, -12)

end
