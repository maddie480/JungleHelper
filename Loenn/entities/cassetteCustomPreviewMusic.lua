local cassette = {}

cassette.name = "JungleHelper/CassetteCustomPreviewMusic"
cassette.depth = -1000000
cassette.nodeLineRenderType = "line"
cassette.texture = "collectables/cassette/idle00"
cassette.nodeLimits = {2, 2}
cassette.placements = {
    name = "cassette",
    data = {
        musicEvent = "event:/game/general/cassette_preview",
        musicParamName = "remix",
        musicParamValue = 1.0
    }
}

return cassette
