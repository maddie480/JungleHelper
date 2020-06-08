module JungleHelperRuinsSpikesUp

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Spikes (Up, Ruins) (JungleHelper)" => Ahorn.EntityPlacement(
        Maple.SpikesUp,
        "rectangle",
        Dict{String, Any}(
          "type" => "ruins"
        )
    )
)

end