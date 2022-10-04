extends Sprite


var avatars_4 = []
var currentSelected = 0		# Spot of the FrameSelector within the avatars_4[]
var currentColumnSpot = 0	# Spot of the FrameSelector based on the column
var currentRowSpot = 0
var enterPressed = false
var previousSelected = false

export (Texture) var selectorTexture
export (int) var amountOfRows = 3      # The total amount of rows the character select is able to show 
export (Vector2) var avatarsOffset
export (int) var playerGreenAvatar

onready var mainScript = get_parent()
onready var gridContainer = get_node("../GridContainer")
onready var selector4 = get_node("../Selector4")
onready var selectorLabel4 = selector4.get_node("Label")

# Called when the node enters the scene tree for the first time.
func _ready():
    for nameOfAvatars_4 in get_tree().get_nodes_in_group("Avatars_4"):
        avatars_4.append(nameOfAvatars_4)
    
    texture = selectorTexture


func _input(event):
    if event is InputEventKey and event.pressed and previousSelected and not enterPressed and mainScript._popupClosed:
        
        
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
            playerGreenAvatar = currentSelected
            print(playerGreenAvatar)
            mainScript._green = currentSelected
            
    if event is InputEventMouseButton and previousSelected and not enterPressed and mainScript._popupClosed:
        if event.button_index == BUTTON_LEFT and event.pressed:
            print("KLIK")
            enterPressed = true
            playerGreenAvatar = currentSelected
            print(playerGreenAvatar)
            mainScript._green = currentSelected


func _on_Avatar1Green_mouse_entered():
    if(enterPressed):
        return
    currentSelected = 0
    position.y = 130


func _on_Avatar2Green_mouse_entered():
    if(enterPressed):
        return
    currentSelected = 1
    position.y = 130 + 206


func _on_Avatar4Green_mouse_entered():
    if(enterPressed):
        return
    currentSelected = 2
    position.y = 130 + 206 + 206
