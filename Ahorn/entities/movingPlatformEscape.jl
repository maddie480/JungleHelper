module JungleHelperEscapeMovingPlatform

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Platform (Moving, Escape) (Jungle Helper)" => Ahorn.EntityPlacement(
        Maple.MovingPlatform,
        "rectangle",
        Dict{String, Any}(
          "texture" => "escape"
        ),
        
        function(entity)
            x, y = Int(entity.data["x"]), Int(entity.data["y"])
            width = Int(get(entity.data, "width", 8))
            entity.data["x"], entity.data["y"] = x + width, y
            entity.data["nodes"] = [(x, y)]
        end
    )
)

end
