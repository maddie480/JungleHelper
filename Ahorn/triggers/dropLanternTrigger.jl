module JungleHelperDropLanternTrigger

using ..Ahorn, Maple

@mapdef Trigger "JungleHelper/DropLanternTrigger" DropLanternTrigger(x::Integer, y::Integer, width::Integer=8, height::Integer=8, oneUse::Bool=false, destroyLantern::Bool=true)

const placements = Ahorn.PlacementDict(
    "Drop Lantern (Jungle Helper)" => Ahorn.EntityPlacement(
       DropLanternTrigger,
        "rectangle"
    )
)

end
