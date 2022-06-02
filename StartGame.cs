

using System;
using Godot;
using static Utils;

public class StartGame : Node
{
    public override void _Process(float delta)
    {
        var start = (ResourceLoader.Load<PackedScene>("res://Game/Loading/MainMenuLoad.tscn").Instance());
        SetMainScene(start);
        DestroyNode(this); // necessary, because there is no main scene set
    }
}
