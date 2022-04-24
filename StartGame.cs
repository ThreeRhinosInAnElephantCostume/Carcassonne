

using System;
using Godot;
using static Utils;

public class StartGame : Node
{
    public override void _Process(float delta)
    {
        var start = (ResourceLoader.Load<PackedScene>("res://Game/StartMenu/StartMenu.tscn").Instance());
        GetTree().Root.AddChild(start);
        DestroyNode(this);
    }
}
