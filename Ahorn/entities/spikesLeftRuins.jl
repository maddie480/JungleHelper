module JungleHelperRuinsSpikesLeft

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Spikes (Left, Ruins) (Jungle Helper)" => Ahorn.EntityPlacement(
        Maple.SpikesLeft,
        "rectangle",
        Dict{String, Any}(
          "type" => "ruins"
        )
    )
)

end