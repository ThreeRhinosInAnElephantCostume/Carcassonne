extends Sprite


var avatars_0 = []
var currentSelected = 0		# Spot of the FrameSelector within the botLevels_1[]
var currentColumnSpot = 0	# Spot of the FrameSelector based on the column
var currentRowSpot = 0
var enterPressed = false


export (Texture) var selectorTexture
export (int) var amountOfRows = 3      # The total amount of rows the character select is able to show 
export (Vector2) var avatarsOffset
export (int) var playerRedAvatar

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
    if event is InputEventKey and event.pressed and not enterPressed and mainScript._popupClosed:
        
        
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


    if event is InputEventMouseButton and not enterPressed and mainScript._popupClosed:
        if event.button_index == BUTTON_LEFT and event.pressed:
            print("KLIK")
            enterPressed = true
            playerRedAvatar = currentSelected
            print(playerRedAvatar)
            nextSelector.previousSelected = true
            mainScript._red = currentSelected
            
            if avatarBlack.visible == true:
                nextSelector.visible = true


func _on_Avatar1Red_mouse_entered():
    if(enterPressed):
        return
    currentSelected = 0
    position.y = 130


func _on_Avatar2Red_mouse_entered():
    if(enterPressed):
        return
    currentSelected = 1
    position.y = 130 + 206


func _on_Avatar4Red_mouse_entered():
    if(enterPressed):
        return
    currentSelected = 2
    position.y = 130 + 206 + 206
