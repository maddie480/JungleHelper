module JungleHelperRuinsSpikesLeft

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Spikes (Left, Ruins) (JungleHelper)" => Ahorn.EntityPlacement(
        Maple.SpikesLeft,
        "rectangle",
        Dict{String, Any}(
          "type" => "ruins"
        )
    )
)

end