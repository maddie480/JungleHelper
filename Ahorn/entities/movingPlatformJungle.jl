module JungleHelperJungleMovingPlatform

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Platform (Moving, Jungle) (JungleHelper)" => Ahorn.EntityPlacement(
        Maple.MovingPlatform,
        "rectangle",
        Dict{String, Any}(
          "texture" => "Jungle"
        )
    )
)

end
