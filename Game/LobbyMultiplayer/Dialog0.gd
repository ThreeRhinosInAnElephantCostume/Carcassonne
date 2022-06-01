extends WindowDialog

var players
var HBox2
var HBox3
var HBox4

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
	
	
	var name0 = find_node("Name0").text.strip_edges()
	var name1 = find_node("Name1").text.strip_edges()
	var name2 = find_node("Name2").text.strip_edges()
	var name3 = find_node("Name3").text.strip_edges()
	var name4 = find_node("Name4").text.strip_edges()
	print("___________")
	print("imię gracza 1:")
	print(name0)
	print("imię gracza 2:")
	print(name1)
	print("imię gracza 3:")
	print(name2)
	print("imię gracza 4:")
	print(name3)
	print("imię gracza 5:")
	print(name4)
	print("___________")
	if name0.empty():
		name0 = "Player 1"
	selectorLabel0.text = name0
	if name1.empty():
		name1 = "Player 2"
	selectorLabel1.text = name1
	if name2.empty():
		name2 = "Player 3"
	selectorLabel2.text = name2
	if name3.empty():
		name3 = "Player 4"
	selectorLabel3.text = name3
	if name4.empty():
		name4 = "Player 5"
	selectorLabel4.text = name4
	
	
	hide()


func _on_Ok_button_down():

	players = find_node("Players").text.strip_edges()
	print("___________")
	print("Liczba graczy:")
	print(players)
	print("___________")
	
	HBox2 = find_node("HBox2")
	HBox3 = find_node("HBox3")
	HBox4 = find_node("HBox4")
	
	if int(players) == 3:
		HBox2.visible = true
		HBox3.visible = false
		HBox4.visible = false
	elif int(players) == 4:
		HBox2.visible = true
		HBox3.visible = true
		HBox4.visible = false
	elif int(players) > 4:
		HBox2.visible = true
		HBox3.visible = true
		HBox4.visible = true
	else:
		HBox2.visible = false
		HBox3.visible = false
		HBox4.visible = false
	
	mainScript._amountOfPlayers = players
	#mainScript._namePlayer0 = name0
	mainScript.PlayersVisibilityOn()
