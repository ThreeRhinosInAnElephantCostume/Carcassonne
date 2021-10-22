using System;
using Godot;
using static Utils;

[Tool]
public class MLoadGameList : ItemList
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    void LoadItems()
    {
        Clear();
        RepeatN(5, i => AddItem($"<SAVE DATA {i}>"));
    }
    public override void _Ready()
    {
        LoadItems();
    }
    public override void _Process(float delta)
    {
        if (GetItemCount() == 0)
        {
            LoadItems();
        }
    }
}
