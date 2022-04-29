extends Sprite


var botLevels_1 = []
var currentSelected = 0		# Spot of the FrameSelector within the botLevels_1[]
var currentColumnSpot = 0	# Spot of the FrameSelector based on the column
var currentRowSpot = 0
var enterPressed = false

var botLevelsNames = ["Easy bot", "Mid", "Hard"]


export (Texture) var selectorTexture
export (int) var amountOfRows = 3      # The total amount of rows the character select is able to show 
export (Vector2) var avatarsOffset
export (int) var botBlackLevel
#export (Label) var selectorLabel_1 = "Easy bot"

onready var mainScript = get_parent()
onready var gridContainer = get_node("../GridContainer")
onready var selector1 = get_node("../Selector1")
onready var selectorLabel1 = selector1.get_node("Label")
onready var nextSelector = get_node("../Selector2")
onready var botBlue = get_node("/root/LobbySingle/GridContainer/GridContainerBots/HBoxContainerBotsEasy/BotEasyBlue")

# Called when the node enters the scene tree for the first time.
func _ready():
	for nameOfBotLevels_1 in get_tree().get_nodes_in_group("BotLevels_1"):
		botLevels_1.append(nameOfBotLevels_1)
	
	texture = selectorTexture
	
	selectorLabel1.text = "Easy bot"
	print(selectorLabel1.text)
	
	
		
	
	
func _input(event):
	if event is InputEventKey and event.pressed and not enterPressed:
		
		
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
			selectorLabel1.text = "Easy bot"
		elif currentSelected == 1:
			selectorLabel1.text = "Mid bot"
		elif currentSelected == 2:
			selectorLabel1.text = "Hard bot"
		print(selectorLabel1.text)
		
		if event.scancode == KEY_ENTER:
			enterPressed = true
			botBlackLevel = currentSelected
			print(botBlackLevel)
			nextSelector.previousSelected = true
			mainScript._black = currentSelected
			
			if botBlue.visible == true:
				nextSelector.visible = true
