module JungleHelperWipeDropdown

using ..Ahorn

# Inject Jungle Helper wipes into the dropdown in map metadata.
Ahorn.MetadataWindow.metaDropdownOptions["Wipe"]["Arrow (Jungle Helper + Maddie's Helping Hand)"] = "MaxHelpingHand/CustomWipe:JungleHelper/Arrow"
Ahorn.MetadataWindow.metaDropdownOptions["Wipe"]["Dots (Jungle Helper + Maddie's Helping Hand)"] = "MaxHelpingHand/CustomWipe:JungleHelper/Dots"
Ahorn.MetadataWindow.metaDropdownOptions["Wipe"]["Rocks (Jungle Helper + Maddie's Helping Hand)"] = "MaxHelpingHand/CustomWipe:JungleHelper/Rocks"
Ahorn.MetadataWindow.metaDropdownOptions["Wipe"]["Spinners (Jungle Helper + Maddie's Helping Hand)"] = "MaxHelpingHand/CustomWipe:JungleHelper/Speen"
Ahorn.MetadataWindow.metaDropdownOptions["Wipe"]["Vines (Jungle Helper + Maddie's Helping Hand)"] = "MaxHelpingHand/CustomWipe:JungleHelper/Vines"

end