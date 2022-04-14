tool
extends EditorPlugin

var dock: Control = null
func _scene_changed(root: Node):
	if(dock != null):
		dock.set("SceneRoot", root)


func _enter_tree():
	add_custom_type("MeshProp", "MeshInstance", preload("res://Props/MeshProp.cs"), preload("res://Icons/Nodes/MeshPropIcon.png"))
	add_custom_type("BannerProp", "Spatial", preload("res://Props/BannerProp.cs"), preload("res://Icons/Nodes/BannerPropIcon.png"))
	add_custom_type("SpatialProp", "Spatial", preload("res://Props/SpatialProp.cs"), preload("res://Icons/Nodes/SpatialPropIcon.png"))
	add_custom_type("ControlProp", "Control", preload("res://Props/ControlProp.cs"), preload("res://Icons/Nodes/ControlPropIcon.png"))
	add_custom_type("Sprite2DProp", "Sprite", preload("res://Props/Sprite2DProp.cs"), preload("res://Icons/Nodes/Sprite2DPropIcon.png"))

	add_custom_type("CapturableProp", "Spatial", preload("res://Props/CapturableProp.cs"), preload("res://Icons/Nodes/SpatialPropIcon.png"))

	dock = preload("res://addons/ThemeSystem/ThemeEditorDock.tscn").instance()
	add_control_to_dock(DOCK_SLOT_LEFT_UL, dock)
	connect("scene_changed", self, "_scene_changed")

func _exit_tree():
	remove_custom_type("BannerProp")
	remove_custom_type("MeshProp")
	remove_custom_type("SpatialProp")
	remove_custom_type("ControlProp")
	remove_custom_type("Sprite2DProp")
	remove_control_from_docks(dock)
	dock.free()
	dock = null