module JungleHelperJungleMovingPlatform

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Platform (Moving, Jungle) (JungleHelper)" => Ahorn.EntityPlacement(
        Maple.movingPlatform,
        "rectangle",
        Dict{String, Any}(
          "texture" => "Jungle"
        )
    )
)

end
