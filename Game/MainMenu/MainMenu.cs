using Godot;
using System;


public class MainMenu : Control
{
    Button _play;
    Button _quit;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _play = GetNode<Button>("Play");
        _play.Connect("pressed",_play,"_onPlayPressed");

        _quit = GetNode<Button>("Quit");
        _quit.Connect("pressed", _quit, "_onQuitPressed");
    }

    void _onPlayPressed()
    {
        GD.Print("Play pressed!");
        GetTree().ChangeScene("res://Game/Loading/MainMenuLoad.tscn");
    }

        void _onQuitPressed()
    {
        GD.Print("Quit pressed!");
        GetTree().Quit();
    }


  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(float delta)
  {
      
  }
}
