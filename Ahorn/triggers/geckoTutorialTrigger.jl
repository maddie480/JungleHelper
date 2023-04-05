module JungleHelperGeckoTutorialTrigger

using ..Ahorn, Maple

@mapdef Trigger "JungleHelper/GeckoTutorialTrigger" GeckoTutorialTrigger(x::Integer, y::Integer, width::Integer=8, height::Integer=8, geckoId::String="geckoId", showTutorial::Bool=true)

const placements = Ahorn.PlacementDict(
    "Gecko Tutorial (Jungle Helper)" => Ahorn.EntityPlacement(
       GeckoTutorialTrigger,
        "rectangle"
    )
)

end
