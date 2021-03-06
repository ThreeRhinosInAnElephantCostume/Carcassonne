using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using Carcassonne;
using ExtraMath;
using Godot;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;


public class TileMap : Node2D
{
    static PackedScene placedtile = GD.Load<PackedScene>("res://Test/PlacedTile.tscn");
    static PackedScene potentialtile = GD.Load<PackedScene>("res://Test/PotentialTile.tscn");
    public GameEngine game;
    public PlacedTile tilesuggestion;
    public List<PlacedTile> tiledisplays = new List<PlacedTile>();
    public Dictionary<Vector2I, PotentialTile> potentialtiles = new Dictionary<Vector2I, PotentialTile>();

    public System.Action UpdateHandle = null;
    public void TriggerPlacement(Vector2I pos, int rot)
    {
        Assert(game.CurrentState == State.PLACE_TILE);
        game.PlaceCurrentTile(pos, rot);
        CallDeferred("UpdateDisplay");
        if (UpdateHandle != null)
            UpdateHandle();
    }
    public void DisablePotentiaPlacement()
    {
        if (tilesuggestion == null)
            return;
        RemoveChild(tilesuggestion);
        tilesuggestion.QueueFree();
        tilesuggestion = null;
    }
    public void SetPotentiaPlacement(Vector2I pos, int rot)
    {
        if (game.CurrentState != GameEngine.State.PLACE_TILE)
            return;
        DisablePotentiaPlacement();
        tilesuggestion = (PlacedTile)placedtile.Instance();
        tilesuggestion.Rotation = (float)PI * rot * 0.5f;
        tilesuggestion.RenderedTile = game.CurrentTile;
        tilesuggestion.OpacityMP = 0.4f;
        tilesuggestion.GridPosition = pos;

        AddChild(tilesuggestion);

    }
    public void UpdateDisplay()
    {
        var tiles = game.map.GetPlacedTiles();
        var unplaced = tiles.ToList();
        foreach (var td in tiledisplays.ToList())
        {
            if (!td.RenderedTile.IsPlaced || !unplaced.Contains(td.RenderedTile))
            {
                RemoveChild(td);
                td.QueueFree();
                tiledisplays.Remove(td);
            }
            else
            {
                unplaced.Remove(td.RenderedTile);
                td.GridPosition = td.RenderedTile.Position;
                td.Update();
            }
        }
        foreach (var t in unplaced)
        {
            PlacedTile pt = (PlacedTile)placedtile.Instance();
            pt.RenderedTile = t;
            AddChild(pt);
            tiledisplays.Add(pt);
            pt.GridPosition = t.Position;
        }
        foreach (var it in potentialtiles)
        {
            RemoveChild(it.Value);
            it.Value.QueueFree();
        }
        potentialtiles.Clear();
        if (game.CurrentState == GameEngine.State.PLACE_TILE)
        {
            foreach (var it in game.PossibleTilePlacements())
            {
                PotentialTile potential;
                if (potentialtiles.ContainsKey(it.pos))
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
                    potential.PossibleRots = new bool[4] { false, false, false, false };
                }
                potential.GridPosition = it.pos;
                potential.PossibleRots[it.rot] = true;
                potential.Update();
            }
        }
    }
    public void SetGame(GameEngine game)
    {
        this.game = game;
        UpdateDisplay();
    }
    public override void _Ready()
    {
        //game = GameEngine.CreateBaseGame(new GameExternalDataLoader(), 666, 5, "BaseGame/BaseTileset.json");
        //UpdateDisplay();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {

    }
}
