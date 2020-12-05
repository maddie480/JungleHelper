module JungleHelperTheoStatueGate

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/TheoStatueGate" TheoStatueGate(x::Integer, y::Integer, height::Integer=48, sprite::String="default")

const placements = Ahorn.PlacementDict(
    "Green Crystal Gate (Jungle Helper)" => Ahorn.EntityPlacement(
        TheoStatueGate,
        "point",
        Dict{String, Any}(
            "sprite" => "theo"
        )
    )
)

const textures = String["default", "mirror", "theo"]

Ahorn.editingOptions(entity::TheoStatueGate) = Dict{String, Any}(
    "sprite" => textures
)

function Ahorn.selection(entity::TheoStatueGate)
    x, y = Ahorn.position(entity)
    height = Int(get(entity.data, "height", 8))

    return Ahorn.Rectangle(x - 4, y, 15, height)
end

const sprites = Dict{String, String}(
    "default" => "objects/door/TempleDoor00",
    "mirror" => "objects/door/TempleDoorB00",
    "theo" => "objects/door/TempleDoorC00"
)

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TheoStatueGate, room::Maple.Room)
    sprite = get(entity.data, "sprite", "default")

    if haskey(sprites, sprite)
        Ahorn.drawImage(ctx, sprites[sprite], -4, 0)
    end
end

end
