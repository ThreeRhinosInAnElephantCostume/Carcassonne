extends WindowDialog

var players

onready var mainScript = get_parent()
onready var selector0 = get_node("../Selector0")
onready var selectorLabel0 = selector0.get_node("Label")
onready var selector1 = get_node("../Selector1")
onready var selectorLabel1 = selector1.get_node("Label")
onready var selector2 = get_node("../Selector2")
onready var selectorLabel2 = selector2.get_node("Label")
onready var selector3 = get_node("../Selector3")
onready var selectorLabel3 = selector3.get_node("Label")
onready var selector4 = get_node("../Selector4")
onready var selectorLabel4 = selector4.get_node("Label")

func _ready():
	selectorLabel0.text = "Player 1"
	print(selectorLabel0.text)
	selectorLabel1.text = "Player 2"
	print(selectorLabel1.text)
	selectorLabel2.text = "Player 3"
	print(selectorLabel2.text)
	selectorLabel3.text = "Player 4"
	print(selectorLabel3.text)
	selectorLabel4.text = "Player 5"
	print(selectorLabel4.text)

	
func _on_Button_button_down():
	players = find_node("Players").text.strip_edges()
	print("___________")
	print("Liczba graczy:")
	print(players)
	print("___________")
	
	var name0 = find_node("Name0").text.strip_edges()
	print("___________")
	print("imię:")
	print(name0)
	print("___________")
	if name0.empty():
		name0 = "Player"
	selectorLabel0.text = name0
	
	mainScript._amountOfPlayers = players
	#mainScript._namePlayer0 = name0
	mainScript.PlayersVisibilityOn()
	hide()
	
func _on_Name0_text_entered(_new_text):
	_on_Button_button_down()
	
