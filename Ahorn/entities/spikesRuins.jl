module JungleHelperRuinsSpikesUp

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Spikes (Up, Ruins) (Jungle Helper)" => Ahorn.EntityPlacement(
        Maple.SpikesUp,
        "rectangle",
        Dict{String, Any}(
          "type" => "JungleHelper/ruins"
        )
    ),
    "Spikes (Down, Ruins) (Jungle Helper)" => Ahorn.EntityPlacement(
        Maple.SpikesDown,
        "rectangle",
        Dict{String, Any}(
          "type" => "JungleHelper/ruins"
        )
    ),
	"Spikes (Left, Ruins) (Jungle Helper)" => Ahorn.EntityPlacement(
        Maple.SpikesLeft,
        "rectangle",
        Dict{String, Any}(
          "type" => "JungleHelper/ruins"
        )
    ),
	"Spikes (Right, Ruins) (Jungle Helper)" => Ahorn.EntityPlacement(
        Maple.SpikesRight,
        "rectangle",
        Dict{String, Any}(
          "type" => "JungleHelper/ruins"
        )
    )
)

end