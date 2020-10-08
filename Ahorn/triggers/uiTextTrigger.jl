module JungleHelperUITextTrigger

using ..Ahorn, Maple

@mapdef Trigger "JungleHelper/UITextTrigger" UITextTrigger(x::Integer, y::Integer, width::Integer=8, height::Integer=8, Dialog::String="",TextX::Number=0,TextY::Number=0)

const placements = Ahorn.PlacementDict(
    "UI Text Display (Jungle Helper)" => Ahorn.EntityPlacement(
       UITextTrigger,
        "rectangle"
    )
)

end
