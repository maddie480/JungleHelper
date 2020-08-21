module JungleHelperDarkWoodSpikesLeft

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Spikes (Left, Wood Outline) (Jungle Helper)" => Ahorn.EntityPlacement(
        Maple.SpikesLeft,
        "rectangle",
        Dict{String, Any}(
          "type" => "wooddark"
        )
    )
)

end