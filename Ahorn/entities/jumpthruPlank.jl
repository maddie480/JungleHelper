module JungleHelperPlankJumpthru

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Jump Through (Plank) (Jungle Helper)" => Ahorn.EntityPlacement(
        Maple.JumpThru,
        "rectangle",
        Dict{String, Any}(
          "texture" => "JungleHelper/Plank"
        )
    )
)

end
