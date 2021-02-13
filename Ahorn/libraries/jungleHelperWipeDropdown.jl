module JungleHelperWipeDropdown

using ..Ahorn

# Inject Jungle Helper wipes into the dropdown in map metadata.
Ahorn.MetadataWindow.metaDropdownOptions["Wipe"]["Arrow (Jungle Helper)"] = "JungleHelper/SpriteWipe:JungleHelper/Arrow"
Ahorn.MetadataWindow.metaDropdownOptions["Wipe"]["Dots (Jungle Helper)"] = "JungleHelper/SpriteWipe:JungleHelper/Dots"
Ahorn.MetadataWindow.metaDropdownOptions["Wipe"]["Rocks (Jungle Helper)"] = "JungleHelper/SpriteWipe:JungleHelper/Rocks"
Ahorn.MetadataWindow.metaDropdownOptions["Wipe"]["Spinners (Jungle Helper)"] = "JungleHelper/SpriteWipe:JungleHelper/Speen"
Ahorn.MetadataWindow.metaDropdownOptions["Wipe"]["Vines (Jungle Helper)"] = "JungleHelper/SpriteWipe:JungleHelper/Vines"

end