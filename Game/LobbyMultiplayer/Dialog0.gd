extends WindowDialog

var players

onready var mainScript = get_parent()
onready var selector0 = get_node("../Selector0")
onready var selectorLabel0 = selector0.get_node("Label")

func _ready():
	selectorLabel0.text = "Player"
	print(selectorLabel0.text)

	
func _on_Button_button_down():
	var name = find_node("Name").text.strip_edges()
	print("___________")
	print("imię:")
	print(name)
	print("___________")
	if name.empty():
		name = "Player"
	selectorLabel0.text = name
	players = find_node("Players").text.strip_edges()
	print("___________")
	print("Liczba graczy:")
	print(players)
	print("___________")
	mainScript._amountOfPlayers = players
	mainScript._namePlayer = name
	mainScript.PlayersVisibilityOn()
	hide()
	
func _on_Name_text_entered(_new_text):
	_on_Button_button_down()
	
