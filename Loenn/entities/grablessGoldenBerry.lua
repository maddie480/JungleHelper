local strawberry = {}

strawberry.name = "JungleHelper/TreeDepthController"
strawberry.depth = -100
strawberry.nodeLineRenderType = "fan"
strawberry.nodeLimits = {0, -1}

function strawberry.texture(room, entity)
    local hasNodes = entity.nodes and #entity.nodes > 0

    if hasNodes then
        return "collectables/ghostgoldberry/wings01"
    else
        return "collectables/goldberry/wings01"
    end
end

function strawberry.nodeTexture(room, entity)
    local hasNodes = entity.nodes and #entity.nodes > 0

    if hasNodes then
        return "collectables/goldberry/seed00"
    end
end

strawberry.placements = {
    {
        name = "golden"
    }
}

return strawberry
