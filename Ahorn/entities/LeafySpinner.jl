module JungleHelperLeafySpinner

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Spinner(Leafy) (JungleHelper)" => Ahorn.EntityPlacement(
        Maple.Spinner,
		"point",
        Dict{String, Any}(
          "color" => "jungle"
        )
    )
)

end