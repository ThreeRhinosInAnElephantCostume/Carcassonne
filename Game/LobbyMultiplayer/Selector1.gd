extends Sprite


var avatars_1 = []
var currentSelected = 0		# Spot of the FrameSelector within the avatars_1[]
var currentColumnSpot = 0	# Spot of the FrameSelector based on the column
var currentRowSpot = 0
var enterPressed = false
var previousSelected = false

export (Texture) var selectorTexture
export (int) var amountOfRows = 3      # The total amount of rows the character select is able to show 
export (Vector2) var avatarsOffset
export (int) var playerBlackAvatar

onready var mainScript = get_parent()
onready var gridContainer = get_node("../GridContainer")
onready var selector1 = get_node("../Selector1")
onready var selectorLabel1 = selector1.get_node("Label")
onready var nextSelector = get_node("../Selector2")
onready var avatarBlue = get_node("/root/LobbyMulti/GridContainer/GridContainerAvatars/HBoxContainerAvatar1/Avatar1Blue")

# Called when the node enters the scene tree for the first time.
func _ready():
	for nameOfAvatars_1 in get_tree().get_nodes_in_group("Avatars_1"):
		avatars_1.append(nameOfAvatars_1)
	
	texture = selectorTexture


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
				

		if event.scancode == KEY_ENTER:
			enterPressed = true
			playerBlackAvatar = currentSelected
			print(playerBlackAvatar)
			nextSelector.previousSelected = true
			mainScript._black = currentSelected
			
			if avatarBlue.visible == true:
				nextSelector.visible = true
