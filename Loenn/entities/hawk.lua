local hawk = {}
hawk.name = "JungleHelper/Hawk"
hawk.depth = 0
hawk.placements = {
    {
        name = "default",
        data = {
            mainSpeed = 100.0,
            slowerSpeed = 80.0,
            sprite = "",
        }
    },
    {
        name = "faster",
        data = {
            mainSpeed = 220.0,
            slowerSpeed = 42.0,
            sprite = "junglehelper_hawk_alt",
        }
    }
}

hawk.fieldInformation = {
    sprite = {
        options = { "", "junglehelper_hawk_alt" }
    }
}

hawk.texture = "JungleHelper/hawk/hold03"

return hawk
