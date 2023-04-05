local attachTriggerController = {}
attachTriggerController.name = "JungleHelper/AttachTriggerController"
attachTriggerController.depth = -1000
attachTriggerController.placements = {
    {
        name = "default",
        data = {
            entityFilter = "",
            triggerFilter = "",
        }
    }
}

attachTriggerController.nodeLineRenderType = "line"
attachTriggerController.nodeLimits = { 1, 1 }

attachTriggerController.nodeTexture = "ahorn/JungleHelper/attach_trigger_entity"
attachTriggerController.texture = "ahorn/JungleHelper/attach_trigger_trigger"

attachTriggerController.justification = { 0, 0 }
attachTriggerController.nodeJustification = { 0, 0 }

return attachTriggerController
