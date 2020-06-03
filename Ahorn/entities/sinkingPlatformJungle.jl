module JungleHelperJungleSinkingPlatform

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Platform (Sinking, Jungle) (JungleHelper)" => Ahorn.EntityPlacement(
        Maple.sinkingPlatform,
        "rectangle",
        Dict{String, Any}(
          "texture" => "Jungle"
        )
    )
)

end
