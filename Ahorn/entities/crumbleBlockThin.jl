module JungleHelperThinCrumbleBlock

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Crumble Blocks (Thin) (JungleHelper)" => Ahorn.EntityPlacement(
        Maple.CrumbleBlock,
        "rectangle",
        Dict{String, Any}(
          "texture" => "thin"
        )
    )
)

end
