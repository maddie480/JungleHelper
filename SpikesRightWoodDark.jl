module JungleHelperDarkWoodSpikesRight

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Spikes (Right, Wood Outline) (JungleHelper)" => Ahorn.EntityPlacement(
        Maple.SpikesRight,
        "rectangle",
        Dict{String, Any}(
          "type" => "wooddark"
        )
    )
)

end