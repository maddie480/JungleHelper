module JungleHelperDarkCrumbleBlock

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Crumble Blocks (Dark) (Jungle Helper)" => Ahorn.EntityPlacement(
        Maple.CrumbleBlock,
        "rectangle",
        Dict{String, Any}(
          "texture" => "JungleHelper/dark"
        )
    )
)

end
