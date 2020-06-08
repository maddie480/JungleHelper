module JungleHelperRuinsSpikesDown

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Spikes (Down, Ruins) (JungleHelper)" => Ahorn.EntityPlacement(
        Maple.SpikesDown,
        "rectangle",
        Dict{String, Any}(
          "type" => "ruins"
        )
    )
)

end