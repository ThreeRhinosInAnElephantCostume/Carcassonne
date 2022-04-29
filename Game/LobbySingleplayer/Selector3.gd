extends Sprite


var botLevels_3 = []
var currentSelected = 0		# Spot of the FrameSelector within the botLevels_1[]
var currentColumnSpot = 0	# Spot of the FrameSelector based on the column
var currentRowSpot = 0
var enterPressed = false
var previousSelected = false

var botLevelsNames = ["Easy bot", "Mid", "Hard"]


export (Texture) var selectorTexture
export (int) var amountOfRows = 3      # The total amount of rows the character select is able to show 
export (Vector2) var avatarsOffset
export (int) var botYellowLevel

onready var mainScript = get_parent()
onready var gridContainer = get_parent().get_node("GridContainer")
onready var selector3 = get_node("../Selector3")
onready var selectorLabel3 = selector3.get_node("Label")
onready var nextSelector = get_node("../Selector4")
onready var botGreen = get_node("/root/LobbySingle/GridContainer/GridContainerBots/HBoxContainerBotsEasy/BotEasyGreen")


# Called when the node enters the scene tree for the first time.
func _ready():
	for nameOfBotLevels_3 in get_tree().get_nodes_in_group("BotLevels_3"):
		botLevels_3.append(nameOfBotLevels_3)
	
	texture = selectorTexture
	
	selectorLabel3.text = "Easy bot"
	print(selectorLabel3.text)
	
	print(previousSelected)
	
	
func _input(event):
	if event is InputEventKey and event.pressed and previousSelected and not enterPressed:
		
		
		if event.scancode == KEY_UP:
			if(currentRowSpot > 0):
				print("KEY_UP was pressed")
				currentSelected -= 1
				currentRowSpot -= 1
				position.y -= avatarsOffset.y
			
		elif event.scancode == KEY_DOWN:
			if(currentRowSpot < amountOfRows - 1):
				print("KEY_DOWN was pressed")
				currentSelected += 1
				currentRowSpot += 1
				position.y += avatarsOffset.y
				
		if currentSelected == 0:
			selectorLabel3.text = "Easy bot"
		elif currentSelected == 1:
			selectorLabel3.text = "Mid bot"
		elif currentSelected == 2:
			selectorLabel3.text = "Hard bot"
		print(selectorLabel3.text)
		
		if event.scancode == KEY_ENTER:
			enterPressed = true
			botYellowLevel = currentSelected
			print(botYellowLevel)
			nextSelector.previousSelected = true
			mainScript._yellow = currentSelected
			
			if botGreen.visible == true:
				nextSelector.visible = true
