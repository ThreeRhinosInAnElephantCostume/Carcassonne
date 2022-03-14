using Godot;
using System;


public class StartMenu : Control
{
    Button _play;
    Button _quit;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _play = GetNode<Button>("Play");
        _play.Connect("pressed", this, nameof(OnPlayPressed));

        _quit = GetNode<Button>("Quit");
        _quit.Connect("pressed", this, nameof(OnQuitPressed));
    }

    void OnPlayPressed()
    {
        GD.Print("Play pressed!");
        GetTree().ChangeScene("res://Game/Loading/MainMenuLoad.tscn");
    }

    void OnQuitPressed()
    {
        GD.Print("Quit pressed!");
        GetTree().Quit();
    }


  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(float delta)
  {
      
  }
}
