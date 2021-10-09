tool
extends EditorPlugin

var main_panel

func _enter_tree():
    print("0")
    main_panel = load("res://TileEditor/TileEditor.tscn").instance()
    get_editor_interface().get_editor_viewport().add_child(main_panel)
    make_visible(false)

func _exit_tree():
    if main_panel:
            main_panel.queue_free()

func has_main_screen():
    return true


func make_visible(visible):
    if main_panel:
            main_panel.visible = visible


func get_plugin_name():
    return "Tile Editor"


func get_plugin_icon():
    return get_editor_interface().get_base_control().get_icon("Node", "EditorIcons")