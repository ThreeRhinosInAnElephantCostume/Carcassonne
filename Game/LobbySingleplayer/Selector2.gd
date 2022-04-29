extends Sprite


var botLevels_2 = []
var currentSelected = 0		# Spot of the FrameSelector within the botLevels_1[]
var currentColumnSpot = 0	# Spot of the FrameSelector based on the column
var currentRowSpot = 0
var enterPressed = false
var previousSelected = false

var botLevelsNames = ["Easy bot", "Mid", "Hard"]


export (Texture) var selectorTexture
export (int) var amountOfRows = 3      # The total amount of rows the character select is able to show 
export (Vector2) var avatarsOffset
export (int) var botBlackLevel

onready var gridContainer = get_parent().get_node("GridContainer")

onready var selector2 = get_node("../Selector2")
onready var selectorLabel2 = selector2.get_node("Label")
onready var nextSelector = get_node("../Selector3")
onready var botYellow = get_node("/root/LobbySingle/GridContainer/GridContainerBots/HBoxContainerBotsEasy/BotEasyYellow")


# Called when the node enters the scene tree for the first time.
func _ready():
	for nameOfBotLevels_2 in get_tree().get_nodes_in_group("BotLevels_2"):
		botLevels_2.append(nameOfBotLevels_2)
	
	texture = selectorTexture
	
	selectorLabel2.text = "Easy bot"
	print(selectorLabel2.text)
	
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
			selectorLabel2.text = "Easy bot"
		elif currentSelected == 1:
			selectorLabel2.text = "Mid bot"
		elif currentSelected == 2:
			selectorLabel2.text = "Hard bot"
		print(selectorLabel2.text)
		
		if event.scancode == KEY_ENTER:
			enterPressed = true
			botBlackLevel = currentSelected
			print(botBlackLevel)
			
			if botYellow.visible == true:
				nextSelector.visible = true
