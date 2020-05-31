module JungleHelperInvisibleJumpthru

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Jump Through (Invisible) (JungleHelper)" => Ahorn.EntityPlacement(
        Maple.JumpThru,
        "rectangle",
        Dict{String, Any}(
          "texture" => "Invisible"
        )
    )
)

end