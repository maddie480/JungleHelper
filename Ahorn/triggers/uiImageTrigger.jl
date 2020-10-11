module JungleHelperUIImageTrigger

using ..Ahorn, Maple

@mapdef Trigger "JungleHelper/UIImageTrigger" UIImageTrigger(x::Integer, y::Integer, width::Integer=8, height::Integer=8, ImagePath::String="logo",Flag::String="",ImageX::Number=0.0,ImageY::Number=0.0,FadeIn::Number=1.0,FadeOut::Number=1.0)

const placements = Ahorn.PlacementDict(
    "UI Image Display (Jungle Helper)" => Ahorn.EntityPlacement(
       UIImageTrigger,
        "rectangle"
    )
)

end
