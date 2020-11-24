module JungleHelperDarkWoodSpikesUp

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Spikes (Up, Wood Outline) (Jungle Helper)" => Ahorn.EntityPlacement(
        Maple.SpikesUp,
        "rectangle",
        Dict{String, Any}(
          "type" => "wooddark"
        )
    ),
	"Spikes (Left, Wood Outline) (Jungle Helper)" => Ahorn.EntityPlacement(
        Maple.SpikesLeft,
        "rectangle",
        Dict{String, Any}(
          "type" => "wooddark"
        )
    ),
	"Spikes (Right, Wood Outline) (Jungle Helper)" => Ahorn.EntityPlacement(
        Maple.SpikesRight,
        "rectangle",
        Dict{String, Any}(
          "type" => "wooddark"
        )
    ),
	"Spikes (Down, Wood Outline) (Jungle Helper)" => Ahorn.EntityPlacement(
        Maple.SpikesDown,
        "rectangle",
        Dict{String, Any}(
          "type" => "wooddark"
        )
    )
)

end