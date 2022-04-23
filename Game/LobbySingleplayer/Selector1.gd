extends Sprite


var botLevels_1 = []
var currentSelected = 0		# Spot of the FrameSelector within the botLevels_1[]
var currentColumnSpot = 0	# Spot of the FrameSelector based on the column
var currentRowSpot = 0

export (Texture) var selectorTexture
export (int) var amountOfRows = 3      # The total amount of rows the character select is able to show 
export (Vector2) var avatarsOffset

onready var gridContainer = get_parent().get_node("GridContainer")

# Called when the node enters the scene tree for the first time.
func _ready():
	for nameOfBotLevels_1 in get_tree().get_nodes_in_group("BotLevels_1"):
		botLevels_1.append(nameOfBotLevels_1)
	
	texture = selectorTexture
	
func _input(event):
	if event is InputEventKey and event.pressed:
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
