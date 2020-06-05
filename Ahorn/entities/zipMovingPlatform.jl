module JungleHelperZipMovingPlatform

using ..Ahorn

@mapdef Entity "JungleHelper/MovingZipPlatform" MovingZipPlatform(x1::Integer, y1::Integer, x2::Integer=x1 + 16, y2::Integer=y1, width::Integer=defaultBlockWidth, texture::String="default") = Entity("movingPlatform", x=x1, y=y1, nodes=Tuple{Int, Int}[(x2, y2)], width=width, texture=texture)


const placements = Ahorn.PlacementDict(
    "Platform (Zip, Jungle) (JungleHelper)" => Ahorn.EntityPlacement(
        Maple.MovingPlatform,
        "rectangle",
        Dict{String, Any}(
          "texture" => "Jungle"
        )
    )
)

end
