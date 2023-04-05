module JungleHelperSpinyPlant

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/SpinyPlant" SpinyPlant(x::Integer, y::Integer, height::Integer=16, color::String="Blue", sprite::String="")

colors = ["Blue", "Pink", "Yellow", "Orange"]

const placements = Ahorn.PlacementDict(
    "Spiny Plant ($(color)) (Jungle Helper)" => Ahorn.EntityPlacement(
        SpinyPlant,
        "rectangle",
        Dict{String, Any}(
            "color" => color
        )
    ) for color in colors
)

Ahorn.editingOptions(entity::SpinyPlant) = Dict{String, Any}(
    "color" => colors
)

Ahorn.minimumSize(entity::SpinyPlant) = 0, 16
Ahorn.resizable(entity::SpinyPlant) = false, true

function Ahorn.selection(entity::SpinyPlant)
    x, y = Ahorn.position(entity)
    height = Int(get(entity.data, "height", 16))

    return Ahorn.Rectangle(x, y, 24, height)
end

sections = ["Top", "Mid", "Bottom"]

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SpinyPlant, room::Maple.Room)
    color = get(entity.data, "color", "Blue")

    # Values need to be system specific integer
    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    height = Int(get(entity.data, "height", 16))
    left = get(entity.data, "left", true)

    startX = div(x, 8) + 1
    startY = div(y, 8) + 1
    stopY = startY + div(height, 8) - 1

    len = stopY - startY

    if len == 1
        topOpen = get(room.fgTiles.data, (startY - 1, startX + 1), false) == '0'
        bottomOpen = get(room.fgTiles.data, (stopY + 1, startX + 1), false) == '0'

        if topOpen
            if bottomOpen
                section = "Solo"
            else
                section = "Top"
            end
        else
            if bottomOpen
                section = "Bottom"
            else
                section = "Mid"
            end
        end

        Ahorn.drawImage(ctx, "JungleHelper/SpinyPlant/Spiny$(color)$(section)", section == "Solo" ? 4 : 0, 0)
    else
        for i in 0:2:len
            if i == len
                i = len - 1
            end

            section = "Mid"
            if i == 0
                if get(room.fgTiles.data, (startY - 1, startX + 1), false) == '0'
                    section = "Top"
                end
            elseif i == len - 1
                if get(room.fgTiles.data, (stopY + 1, startX + 1), false) == '0'
                    section = "Bottom"
                end
            end

            Ahorn.drawImage(ctx, "JungleHelper/SpinyPlant/Spiny$(color)$(section)", 0, 8 * i)
        end
    end
end

end
