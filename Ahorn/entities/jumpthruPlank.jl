module JungleHelperPlankJumpthru

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Jump Through (Plank) (JungleHelper)" => Ahorn.EntityPlacement(
        Maple.JumpThru,
        "rectangle",
        Dict{String, Any}(
          "texture" => "Plank"
        )
    )
)

end
