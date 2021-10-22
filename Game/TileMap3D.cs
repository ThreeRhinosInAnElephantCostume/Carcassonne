using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Threading;
using Carcassonne;
using ExtraMath;
using Godot;
using Newtonsoft.Json;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

public class TileMap3D : Spatial
{
    public GameEngine _game;
    public class TileModel
    {
        public PackedScene scene;
        public TileGraphicsConfig.Config config;
    }
    Dictionary<string, TileModel> _models = new Dictionary<string, TileModel>();
    Dictionary<Vector2I, Tile3D> _tiles = new Dictionary<Vector2I, Tile3D>();
    List<Tile3D> _allTiles = new List<Tile3D>();
    Tile3D _currentTile = null;
    List<PlaceableTile> _placeableTile = new List<PlaceableTile>();
    PackedScene _packedPlaceableTile;
    public Tile3D CreateTile(Tile tile)
    {
        string protpath = (string)tile.MetaData;
        Assert(_models.ContainsKey(protpath));
        var tile3d = new Tile3D(tile, _models[protpath]);
        _allTiles.Add(tile3d);
        return tile3d;
    }
    public void PlaceTile(PlaceableTile pot)
    {
        _game.PlaceCurrentTile(pot.GridPosition, pot.TileRotation);
        if (_game.CurrentState == State.PLACE_PAWN)
            _game.SkipPlacingPawn();
        _tiles.Add(pot.GridPosition, pot.tile3d);
        if (pot.tile3d.GetParent() != null)
            pot.tile3d.GetParent().RemoveChild(pot.tile3d);
        this.AddChild(pot.tile3d);
        pot.tile3d.Visible = true;
        pot.tile3d.GridPosition = pot.GridPosition;
        pot.Enabled = false;
        UpdateDisplay();
    }
    public void UpdateDisplay()
    {
        foreach (var it in _game.map.GetPlacedTiles())
        {
            if (!_tiles.ContainsKey(it.Position))
            {
                var tile3d = CreateTile(it);
                tile3d.GridPosition = it.Position;
                _tiles.Add(it.Position, tile3d);
                AddChild(tile3d);
            }
        }
        _placeableTile.ForEach(p => DestroyNode(p));
        _placeableTile.Clear();
        if (_game.CurrentState == State.PLACE_TILE)
        {
            var tile3d = CreateTile(_game.CurrentTile);
            _currentTile = tile3d;
            List<(Vector2I pos, int rot)> possibilities = _game.PossibleTilePlacements();
            Dictionary<Vector2I, List<int>> poss = new Dictionary<Vector2I, List<int>>();
            foreach (var it in possibilities)
            {
                if (!poss.ContainsKey(it.pos))
                    poss.Add(it.pos, new List<int>());
                poss[it.pos].Add(it.rot);
            }
            foreach (var it in poss)
            {
                var pot = (PlaceableTile)_packedPlaceableTile.Instance();
                pot.Translation = GridPosTo3D(it.Key);
                pot.Init(tile3d, it.Key, it.Value, PlaceTile);
                _placeableTile.Add(pot);
                this.AddChild(pot);
            }
        }
    }
    public override void _Ready()
    {
        _game = GameEngine.CreateBaseGame(TileDataLoader.Loader, 777, 2,
            ConcatPaths(Constants.TILESET_DIRECTORY, "BaseGame/BaseTileset.json"));
        Assert(_game != null);
        foreach (var it in ListDirectoryFilesRecursively(Constants.TILE_MODEL_DIRECTORY, s => s.EndsWith(".json")))
        {
            string modelpath = it.Replace(".json", ".tscn");
            if (!FileExists(modelpath))
                continue;
            var gconfig = DeserializeFromFile<TileGraphicsConfig>(it);
            var scene = ResourceLoader.Load<PackedScene>(modelpath);
            Assert(scene != null);
            foreach (var t in gconfig.Configs)
            {
                var tm = new TileModel();
                tm.config = t.Value;
                tm.scene = scene;
                if (!_models.ContainsKey(t.Key))
                    _models.Add(t.Key, tm);
            }
        }
        _packedPlaceableTile = ResourceLoader.Load<PackedScene>("res://Game/PlaceableTile.tscn");
        UpdateDisplay();
    }
}
