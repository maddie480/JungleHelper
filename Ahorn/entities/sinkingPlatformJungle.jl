module JungleHelperJungleSinkingPlatform

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Platform (Sinking, Jungle) (JungleHelper)" => Ahorn.EntityPlacement(
        Maple.SinkingPlatform,
        "rectangle",
        Dict{String, Any}(
          "texture" => "jungle"
        )
		
    )
)

end
