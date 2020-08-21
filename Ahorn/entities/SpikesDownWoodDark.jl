module JungleHelperDarkWoodSpikesDown

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Spikes (Down, Wood Outline) (Jungle Helper)" => Ahorn.EntityPlacement(
        Maple.SpikesDown,
        "rectangle",
        Dict{String, Any}(
          "type" => "wooddark"
        )
    )
)

end