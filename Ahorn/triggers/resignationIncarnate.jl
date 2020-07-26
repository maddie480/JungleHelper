module JungleHelperGeckoTutorialTrigger

using ..Ahorn, Maple

@mapdef Trigger "JungleHelper/GeckoTutorial" GeccoTutorialTrigger(x::Integer, y::Integer, width::Integer=8, height::Integer=8, geckoId::String="", showTutorial::Bool=true)

const placements = Ahorn.PlacementDict(
    "Gecko Tutorial Trigger (JungleHelper)" => Ahorn.EntityPlacement(
       GeccoTutorialTrigger,
        "rectangle"
    )
)

end