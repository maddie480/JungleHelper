module JungleHelperJungleSinkingPlatform

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Platform (Sinking, Jungle) (Jungle Helper)" => Ahorn.EntityPlacement(
        Maple.SinkingPlatform,
        "rectangle",
        Dict{String, Any}(
          "texture" => "JungleHelper/jungle"
        )
    )
)

end
