module JungleHelperWoodSpikesUp

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Spikes (Up, Wood) (JungleHelper)" => Ahorn.EntityPlacement(
        Maple.SpikesUp,
        "rectangle",
        Dict{String, Any}(
          "type" => "wood"
        )
    )
)

end