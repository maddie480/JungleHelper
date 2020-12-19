module JungleHelperWipeDropdown

using ..Ahorn

# Inject Jungle Helper wipes into the dropdown in map metadata.
Ahorn.MetadataWindow.metaDropdownOptions["Wipe"]["Vines (Jungle Helper)"] = "JungleHelper/SpriteWipe:JungleHelper/Ch3Vine"

end