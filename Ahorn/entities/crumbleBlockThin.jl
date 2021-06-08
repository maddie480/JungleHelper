module JungleHelperThinCrumbleBlock

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Crumble Blocks (Thin) (Jungle Helper)" => Ahorn.EntityPlacement(
        Maple.CrumbleBlock,
        "rectangle",
        Dict{String, Any}(
          "texture" => "JungleHelper/thin"
        )
    )
)

end
