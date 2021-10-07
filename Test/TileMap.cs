using Godot;
using System;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime;
using System.Runtime.CompilerServices;

using static System.Math;

using static Utils;

using ExtraMath;


public class TileMap : Node2D
{
    static PackedScene placedtile = GD.Load<PackedScene>("res://Test/PlacedTile.tscn");
    static PackedScene potentialtile = GD.Load<PackedScene>("res://Test/PotentialTile.tscn");
    public Engine game;
    public PlacedTile tilesuggestion;
    public List<PlacedTile> tiledisplays = new List<PlacedTile>();
    public Dictionary<Vector2I, PotentialTile> potentialtiles = new Dictionary<Vector2I, PotentialTile>();
    public void TriggerPlacement(Vector2I pos, int rot)
    {
        game.PlaceCurrentTile(pos, rot);
        if(game.CurrentState == Engine.State.PLACE_PAWN)
            game.SkipPlacingPawn();
        CallDeferred("UpdateDisplay");
    }
    public void DisablePotentiaPlacement()
    {
        if(tilesuggestion == null)
            return;
        RemoveChild(tilesuggestion);
        tilesuggestion.QueueFree();
        tilesuggestion = null;
    }
    public void SetPotentiaPlacement(Vector2I pos, int rot)
    {
        if(game.CurrentState != Engine.State.PLACE_TILE)
            return;
        DisablePotentiaPlacement();
        tilesuggestion = (PlacedTile)placedtile.Instance();
        tilesuggestion.Rotation = (float) PI * rot * 0.5f;
        tilesuggestion.tile = game.GetCurrentTile();
        tilesuggestion.OpacityMP = 0.4f;
        tilesuggestion.GridPosition = pos;

        AddChild(tilesuggestion);

    }
    public void UpdateDisplay()
    {
        var tiles = game.map.GetPlacedTiles();
        var unplaced = tiles.ToList();
        foreach(var td in tiledisplays.ToList())
        {
            if(!td.tile.IsPlaced || !unplaced.Contains(td.tile))
            {
                RemoveChild(td);
                td.QueueFree();
                tiledisplays.Remove(td);
            }
            else
            {
                unplaced.Remove(td.tile);
                td.GridPosition = td.tile.position;
                td.Update();
            }
        }
        foreach(var t in unplaced)
        {
            PlacedTile pt = (PlacedTile)placedtile.Instance();
            pt.tile = t;
            AddChild(pt);
            tiledisplays.Add(pt);
            pt.GridPosition = t.position;
        }
        foreach(var it in potentialtiles)
        {
            RemoveChild(it.Value);
            it.Value.QueueFree();
        }
        potentialtiles.Clear();
        if(game.CurrentState == Engine.State.PLACE_TILE)
        {
            foreach(var it in game.PossiblePlacements())
            {
                PotentialTile potential;
                if(potentialtiles.ContainsKey(it.pos))
                {
                    potential = potentialtiles[it.pos];
                }
                else
                {
                    potential = (PotentialTile)potentialtile.Instance();
                    potential.map = this;
                    potential.GridPosition = it.pos;
                    this.AddChild(potential);
                    potentialtiles[it.pos] = potential;
                    potential.PossibleRots = new bool[4]{false, false, false, false};
                }
                potential.GridPosition = it.pos;
                potential.PossibleRots[it.rot] = true;
                potential.Update();
            }
        }
    }
    public override void _Ready()
    {
        Engine.tilesource = Test2D.TileGenerator;
        game = Engine.CreateBaseGame(666, 2);
        UpdateDisplay();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        
    }
}
