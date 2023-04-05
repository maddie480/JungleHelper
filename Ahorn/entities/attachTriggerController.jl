module JungleHelperAttachTriggerController

using ..Ahorn, Maple

@mapdef Entity "JungleHelper/AttachTriggerController" AttachTriggerController(x::Integer, y::Integer, entityFilter::String="", triggerFilter::String="")

const placements = Ahorn.PlacementDict(
    "Attach Trigger Controller (Jungle Helper)" => Ahorn.EntityPlacement(
        AttachTriggerController,
        "point",
        Dict{String, Any}(),
        function(trigger)
            trigger.data["nodes"] = [
                (Int(trigger.data["x"]) + 16, Int(trigger.data["y"]))
            ]
        end
    )
)

triggerColor = (0, 127, 14, 1) ./ (255, 255, 255, 1)
triggerEdgeColor = (0, 158, 15, 1) ./ (255, 255, 255, 1)

Ahorn.nodeLimits(entity::AttachTriggerController) = 1, 1

function Ahorn.selection(entity::AttachTriggerController)
    triggerX, triggerY = Int(entity.data["x"]), Int(entity.data["y"])
    entityX, entityY = Int.(entity.data["nodes"][1])

    return [Ahorn.Rectangle(triggerX, triggerY, 8, 8), Ahorn.Rectangle(entityX, entityY, 8, 8)]
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::AttachTriggerController, room::Maple.Room)
    Ahorn.Cairo.save(ctx)

    x, y = Int.(entity.data["nodes"][1])

    Ahorn.rectangle(ctx, x, y, 8, 8)
    Ahorn.clip(ctx)

    Ahorn.drawRectangle(ctx, x, y, 8, 8, triggerColor, triggerEdgeColor)
    Ahorn.drawCenteredText(ctx, "E", x, y, 8, 8)

    Ahorn.restore(ctx)

    xT, yT = Ahorn.position(entity)
    Ahorn.drawArrow(ctx, xT + 4, yT + 4, x + 4, y + 4, Ahorn.colors.selection_selected_fc, headLength=6)
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::AttachTriggerController, room::Maple.Room)
    Ahorn.Cairo.save(ctx)

    x, y = Ahorn.position(entity)

    Ahorn.rectangle(ctx, x, y, 8, 8)
    Ahorn.clip(ctx)

    Ahorn.drawRectangle(ctx, x, y, 8, 8, triggerColor, triggerEdgeColor)
    Ahorn.drawCenteredText(ctx, "T", x, y, 8, 8)

    Ahorn.restore(ctx)
end

end
