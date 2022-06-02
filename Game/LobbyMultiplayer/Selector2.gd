extends Sprite


var avatars_2 = []
var currentSelected = 0		# Spot of the FrameSelector within the avatars_2[]
var currentColumnSpot = 0	# Spot of the FrameSelector based on the column
var currentRowSpot = 0
var enterPressed = false
var previousSelected = false

export (Texture) var selectorTexture
export (int) var amountOfRows = 3      # The total amount of rows the character select is able to show 
export (Vector2) var avatarsOffset
export (int) var playerBlueAvatar

onready var mainScript = get_parent()
onready var gridContainer = get_node("../GridContainer")
onready var selector2 = get_node("../Selector2")
onready var selectorLabel2 = selector2.get_node("Label")
onready var nextSelector = get_node("../Selector3")
onready var avatarYellow = get_node("/root/LobbyMulti/GridContainer/GridContainerAvatars/HBoxContainerAvatar1/Avatar1Yellow")

# Called when the node enters the scene tree for the first time.
func _ready():
	for nameOfAvatars_2 in get_tree().get_nodes_in_group("Avatars_2"):
		avatars_2.append(nameOfAvatars_2)
	
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
			playerBlueAvatar = currentSelected
			print(playerBlueAvatar)
			nextSelector.previousSelected = true
			mainScript._blue = currentSelected
			
			if avatarYellow.visible == true:
				nextSelector.visible = true
