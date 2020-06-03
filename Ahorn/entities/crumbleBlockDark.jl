module JungleHelperDarkCrumbleBlock

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Crumble Blocks (Dark) (JungleHelper)" => Ahorn.EntityPlacement(
        Maple.crumbleBlock,
        "rectangle",
        Dict{String, Any}(
          "texture" => "dark"
        )
    )
)

end
