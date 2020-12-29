module JungleHelperAutoFallingBlockDelayed
using ..Ahorn, Maple

@mapdef Entity "JungleHelper/AutoFallingBlockDelayed" AutoFallingBlockDelayed(x::Integer, y::Integer, width::Integer = 8, height::Integer = 8, tiletype::String = "0",delay::Number=2.0,ShakeDelay::Number=0.5, silent::Bool=false)
								
const placements = Ahorn.PlacementDict(
	"Delayed Auto-Falling Block (Jungle Helper)" => Ahorn.EntityPlacement(
		AutoFallingBlockDelayed,
		"rectangle",
		Dict{String, Any}(),
		Ahorn.tileEntityFinalizer
	)
)

Ahorn.editingOptions(entity::AutoFallingBlockDelayed) = Dict{String, Any}(
	"tiletype" => Ahorn.tiletypeEditingOptions()
)

Ahorn.minimumSize(entity::AutoFallingBlockDelayed) = 8, 8
Ahorn.resizable(entity::AutoFallingBlockDelayed) = true, true

Ahorn.selection(entity::AutoFallingBlockDelayed) = Ahorn.getEntityRectangle(entity)

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::AutoFallingBlockDelayed, room::Maple.Room) = Ahorn.drawTileEntity(ctx, room, entity)

end
