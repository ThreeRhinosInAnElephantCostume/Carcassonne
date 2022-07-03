extends Sprite


var botLevels_4 = []
var currentSelected = 0		# Spot of the FrameSelector within the botLevels_1[]
var currentColumnSpot = 0	# Spot of the FrameSelector based on the column
var currentRowSpot = 0
var enterPressed = false
var previousSelected = false

var botLevelsNames = ["Easy bot", "Mid", "Hard"]


export (Texture) var selectorTexture
export (int) var amountOfRows = 3      # The total amount of rows the character select is able to show 
export (Vector2) var avatarsOffset
export (int) var botGreenLevel

onready var mainScript = get_parent()
onready var gridContainer = get_parent().get_node("GridContainer")
onready var selector4 = get_node("../Selector4")
onready var selectorLabel4 = selector4.get_node("Label")
#onready var nextSelector = get_node("../Selector4")
#onready var botGreen = get_node("/root/LobbySingle/GridContainer/GridContainerBots/HBoxContainerBotsEasy/BotEasyGreen")


# Called when the node enters the scene tree for the first time.
func _ready():
    for nameOfBotLevels_4 in get_tree().get_nodes_in_group("BotLevels_4"):
        botLevels_4.append(nameOfBotLevels_4)
    
    texture = selectorTexture
    
    selectorLabel4.text = "Easy bot"
    print(selectorLabel4.text)
    
    print(previousSelected)
    
    
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
                
        if currentSelected == 0:
            selectorLabel4.text = "Easy bot"
        elif currentSelected == 1:
            selectorLabel4.text = "Mid bot"
        elif currentSelected == 2:
            selectorLabel4.text = "Hard bot"
        print(selectorLabel4.text)
        
        if event.scancode == KEY_ENTER:
            enterPressed = true
            botGreenLevel = currentSelected
            print(botGreenLevel)
            mainScript._green = currentSelected


    if event is InputEventMouseButton and previousSelected and not enterPressed and mainScript._popupClosed:
        if event.button_index == BUTTON_LEFT and event.pressed:
            print("KLIK")
            enterPressed = true
            botGreenLevel = currentSelected
            print(botGreenLevel)
            mainScript._green = currentSelected


func _on_BotEasyGreen_mouse_entered():
    if(enterPressed):
        return
    currentSelected = 0
    position.y = 130
    selectorLabel4.text = "Easy bot"


func _on_BotMidGreen_mouse_entered():
    if(enterPressed):
        return
    currentSelected = 1
    position.y = 130 + 206
    selectorLabel4.text = "Mid bot"


func _on_BotHardGreen_mouse_entered():
    if(enterPressed):
        return
    currentSelected = 2
    position.y = 130 + 206 + 206
    selectorLabel4.text = "Hard bot"
