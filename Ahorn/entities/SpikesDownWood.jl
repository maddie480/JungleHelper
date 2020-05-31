module JungleHelperWoodSpikesDown

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Spikes (Down, Wood) (JungleHelper)" => Ahorn.EntityPlacement(
        Maple.SpikesDown,
        "rectangle",
        Dict{String, Any}(
          "type" => "wood"
        )
    )
)

end