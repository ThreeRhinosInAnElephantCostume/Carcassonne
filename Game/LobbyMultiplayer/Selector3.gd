extends Sprite


var avatars_3 = []
var currentSelected = 0		# Spot of the FrameSelector within the avatars_3[]
var currentColumnSpot = 0	# Spot of the FrameSelector based on the column
var currentRowSpot = 0
var enterPressed = false
var previousSelected = false

export (Texture) var selectorTexture
export (int) var amountOfRows = 3      # The total amount of rows the character select is able to show 
export (Vector2) var avatarsOffset
export (int) var playerYellowAvatar

onready var mainScript = get_parent()
onready var gridContainer = get_node("../GridContainer")
onready var selector3 = get_node("../Selector3")
onready var selectorLabel3 = selector3.get_node("Label")
onready var nextSelector = get_node("../Selector4")
onready var avatarGreen = get_node("/root/LobbyMulti/GridContainer/GridContainerAvatars/HBoxContainerAvatar1/Avatar1Green")

# Called when the node enters the scene tree for the first time.
func _ready():
	for nameOfAvatars_3 in get_tree().get_nodes_in_group("Avatars_3"):
		avatars_3.append(nameOfAvatars_3)
	
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
			playerYellowAvatar = currentSelected
			print(playerYellowAvatar)
			nextSelector.previousSelected = true
			mainScript._yellow = currentSelected
			
			if avatarGreen.visible == true:
				nextSelector.visible = true
