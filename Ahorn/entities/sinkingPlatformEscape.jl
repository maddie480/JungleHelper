module JungleHelperEscapeSinkingPlatform

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Platform (Sinking, Escape) (JungleHelper)" => Ahorn.EntityPlacement(
        Maple.SinkingPlatform,
        "rectangle",
        Dict{String, Any}(
          "texture" => "escape"
        )
		
    )
)

end
