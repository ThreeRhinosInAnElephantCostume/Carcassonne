using System.ComponentModel;
using Godot;
using System;


public class PlacedTile : Node2D
{
    public static Vector2 outersize = new Vector2(100, 100);
    const int edgewidth = 5;
    static Color edgecolor = new Color(0.9f, 0.9f, 0.9f, 0.9f);

    public string TileOverride{get; set;} = "";
    public Engine.Tile tile = null;
    public override void _Ready()
    {
        if(TileOverride != "")
            tile = TileMap.TileGenerator(TileOverride);
    }
    public override void _Process(float delta)
    {
        
    }
    public override void _Draw()
    {
        base._Draw();
        DrawRect(new Rect2(-outersize/2, outersize/2), edgecolor, false, edgewidth);
    }
}
