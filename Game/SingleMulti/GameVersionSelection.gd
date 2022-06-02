extends Sprite


var gameVersions = []
var currentSelected = 0		# Spot of the FrameSelector within the gameVersion[]
var currentColumnSpot = 0	# Spot of the FrameSelector based on the column
var currentRowSpot = 0
var GUS = null

export (Texture) var selectorTexture
export (int) var amountOfRows = 1      # The total amount of rows the character select is able to show 
export (Vector2) var paintingOffset

onready var gridContainer = get_parent().get_node("GridContainer")

# Called when the node enters the scene tree for the first time.
func _ready():
	GUS = get_tree().root.get_node("GlobalUtilsScript")
	for nameOfGameVersion in get_tree().get_nodes_in_group("GameVersions"):
		gameVersions.append(nameOfGameVersion)
	texture = selectorTexture
func _single_player_selected():
	GUS.SetMainScene(load("res://Game/LobbySingleplayer/LobbySingleplayer.tscn").instance())
func _multi_player_selected():
	GUS.SetMainScene(load("res://Game/LobbyMultiplayer/LobbyMultiplayer.tscn").instance())
	
func _input(event):
	if event is InputEventKey and !event.pressed:
		if event.scancode == KEY_RIGHT:
			if(currentColumnSpot < gridContainer.columns - 1):
				print("KEY_RIGHT was pressed")
				currentSelected += 1
				currentColumnSpot += 1
				position.x += paintingOffset.x
			
		elif event.scancode == KEY_LEFT:
			if(currentColumnSpot > 0):
				print("KEY_LEFT was pressed")
				currentSelected -= 1
				currentColumnSpot -= 1
				position.x -= paintingOffset.x
				
		elif event.scancode == KEY_ENTER:
			print("KEY_ENTER was pressed")
			if gameVersions[currentSelected].name == "Single":
				_single_player_selected()
			elif gameVersions[currentSelected].name == "Multi":
				_multi_player_selected()
