module JungleHelperWoodSpikesLeft

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Spikes (Left, Wood) (JungleHelper)" => Ahorn.EntityPlacement(
        Maple.SpikesLeft,
        "rectangle",
        Dict{String, Any}(
          "type" => "wood"
        )
    )
)

end