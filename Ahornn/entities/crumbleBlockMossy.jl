module JungleHelperMossyCrumbleBlock

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Crumble Blocks (Mossy) (Jungle Helper)" => Ahorn.EntityPlacement(
        Maple.CrumbleBlock,
        "rectangle",
        Dict{String, Any}(
          "texture" => "mossy"
        )
    )
)

end
