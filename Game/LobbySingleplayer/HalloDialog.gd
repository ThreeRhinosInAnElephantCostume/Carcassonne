extends WindowDialog

signal update(name, opponents)

func open( name = "", opponents = ""):
	find_node("Name").text = name
	find_node("Opponents").text = opponents
	popup_centered()
	
func _on_Button_button_down():
	var name = find_node("Name").text.strip_edges()
	if name.empty():
		name = "Player"
	var opponents = find_node("Opponents").text.strip_edges()
	emit_signal("updated", name, opponents)
	hide()
	
func _on_Name_text_entered(_new_text):
	_on_Button_button_down()
