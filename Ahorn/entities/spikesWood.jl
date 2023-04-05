module JungleHelperWoodSpikesUp

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Spikes (Up, Wood) (Jungle Helper)" => Ahorn.EntityPlacement(
        Maple.SpikesUp,
        "rectangle",
        Dict{String, Any}(
          "type" => "JungleHelper/wood"
        )
    ),
    "Spikes (Down, Wood) (Jungle Helper)" => Ahorn.EntityPlacement(
        Maple.SpikesDown,
        "rectangle",
        Dict{String, Any}(
          "type" => "JungleHelper/wood"
        )
    ),
    "Spikes (Left, Wood) (Jungle Helper)" => Ahorn.EntityPlacement(
        Maple.SpikesLeft,
        "rectangle",
        Dict{String, Any}(
          "type" => "JungleHelper/wood"
        )
    ),
    "Spikes (Right, Wood) (Jungle Helper)" => Ahorn.EntityPlacement(
        Maple.SpikesRight,
        "rectangle",
        Dict{String, Any}(
          "type" => "JungleHelper/wood"
        )
    )
)

end