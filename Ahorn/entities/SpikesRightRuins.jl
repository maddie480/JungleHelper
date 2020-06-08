module JungleHelperRuinsSpikesRight

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Spikes (Right, Ruins) (JungleHelper)" => Ahorn.EntityPlacement(
        Maple.SpikesRight,
        "rectangle",
        Dict{String, Any}(
          "type" => "ruins"
        )
    )
)

end