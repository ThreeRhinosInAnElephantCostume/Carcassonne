extends Sprite


var gameVersions = []
var currentSelected = 0		# Spot of the FrameSelector within the gameVersion[]
var currentColumnSpot = 0	# Spot of the FrameSelector based on the column
var currentRowSpot = 0

export (Texture) var selectorTexture
export (int) var amountOfRows = 1      # The total amount of rows the character select is able to show 
export (Vector2) var paintingOffset

onready var gridContainer = get_parent().get_node("GridContainer")

# Called when the node enters the scene tree for the first time.
func _ready():
	for nameOfGameVersion in get_tree().get_nodes_in_group("GameVersions"):
		gameVersions.append(nameOfGameVersion)
	
	texture = selectorTexture
	
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
			var GUS = get_tree().root.get_node("GlobalUtilsScript")
			if gameVersions[currentSelected].name == "Single":
				print("go to LobbySingleplayer")
				queue_free()
				get_parent().queue_free()
				var scene = load("res://Game/LobbySingleplayer/LobbySingleplayer.tscn").instance()
				GUS.SetMainScene(scene)
			elif gameVersions[currentSelected].name == "Multi":
				print("go to LobbyMultiplayer")
				queue_free()
				get_parent().queue_free()
				var scene = load("res://Game/LobbyMultiplayer/LobbyMultiplayer.tscn").instance()
				GUS.SetMainScene(scene)
