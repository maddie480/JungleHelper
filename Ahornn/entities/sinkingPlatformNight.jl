module JungleHelperNightSinkingPlatform

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Platform (Sinking, Night) (JungleHelper)" => Ahorn.EntityPlacement(
        Maple.SinkingPlatform,
        "rectangle",
        Dict{String, Any}(
          "texture" => "night"
        )
		
    )
)

end
