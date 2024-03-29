module JungleHelperWoodSwitchGate

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Switch Gate (Wood) (Jungle Helper)" => Ahorn.EntityPlacement(
        Maple.SwitchGate,
        "rectangle",
        Dict{String, Any}(
          "sprite" => "JungleHelper/wood"
        ),

        function(entity)
            x, y = Int(entity.data["x"]), Int(entity.data["y"])
            width = Int(get(entity.data, "width", 16))
            entity.data["x"], entity.data["y"] = x + width, y
            entity.data["nodes"] = [(x, y)]
        end
    )
)

end
