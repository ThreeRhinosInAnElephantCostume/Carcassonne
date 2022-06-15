using Godot;
using static Utils;

public class Credits : Control
{
    Button _quitToMainMenuButton;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _quitToMainMenuButton = this.GetNodeSafe<Button>("VBoxContainer/QuitMainMenuButton");
        _quitToMainMenuButton.OnButtonPressed(() => 
        {
            SetMainScene(Globals.Scenes.MainMenuPacked);
        });
    }
}
