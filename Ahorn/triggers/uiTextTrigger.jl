module JungleHelperUITextTrigger

using ..Ahorn, Maple

@mapdef Trigger "JungleHelper/UITextTrigger" UITextTrigger(x::Integer, y::Integer, width::Integer=8, height::Integer=8, Dialog::String="",Flag::String="",TextX::Number=0.0,TextY::Number=0.0,FadeIn::Number=1.0,FadeOut::Number=1.0)

const placements = Ahorn.PlacementDict(
    "UI Text Display (Jungle Helper)" => Ahorn.EntityPlacement(
       UITextTrigger,
        "rectangle"
    )
)

end
