extends Sprite


var avatars_0 = []
var currentSelected = 0		# Spot of the FrameSelector within the botLevels_1[]
var currentColumnSpot = 0	# Spot of the FrameSelector based on the column
var currentRowSpot = 0
var enterPressed = false

var avatarsNames = ["Fawn", "Monk", "Warrior"]


export (Texture) var selectorTexture
export (int) var amountOfRows = 3      # The total amount of rows the character select is able to show 
export (Vector2) var avatarsOffset
export (int) var playerRedAvatar
#export (Label) var selectorLabel_0 = "Fawn"

onready var mainScript = get_parent()
onready var gridContainer = get_node("../GridContainer")
onready var selector0 = get_node("../Selector0")
onready var selectorLabel0 = selector0.get_node("Label")
onready var nextSelector = get_node("../Selector1")
onready var avatarBlack = get_node("/root/LobbyMulti/GridContainer/GridContainerAvatars/HBoxContainerAvatar1/Avatar1Black")

# Called when the node enters the scene tree for the first time.
func _ready():
	for nameOfAvatars_0 in get_tree().get_nodes_in_group("Avatars_0"):
		avatars_0.append(nameOfAvatars_0)
	
	texture = selectorTexture


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
				

		if event.scancode == KEY_ENTER:
			enterPressed = true
			playerRedAvatar = currentSelected
			print(playerRedAvatar)
			nextSelector.previousSelected = true
			mainScript._red = currentSelected
			
			if avatarBlack.visible == true:
				nextSelector.visible = true
