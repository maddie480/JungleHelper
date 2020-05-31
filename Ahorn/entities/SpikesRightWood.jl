module JungleHelperWoodSpikesRight

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Spikes (Right, Wood) (JungleHelper)" => Ahorn.EntityPlacement(
        Maple.SpikesRight,
        "rectangle",
        Dict{String, Any}(
          "type" => "wood"
        )
    )
)

end