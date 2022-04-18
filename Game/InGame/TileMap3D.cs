using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Enumeration;
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

public class TileMap3D : Spatial
{
    public Action<Vector2I, int> OnTilePlaced;
    public Action<int> OnMeeplePlacedOnNode;
    public Action<int> OnMeeplePlacedOnAttribute;
    public Tile3D NextTile { get; protected set; }
    List<PotentialTile3D> _potentialTile3Ds = new List<PotentialTile3D>();
    Game _game;
    Dictionary<Vector2I, PotentialTile3D> _potDict = new Dictionary<Vector2I, PotentialTile3D>();
    GameEngine _engine = null;
    RNG _rng = new RNG(777);
    public GameEngine Engine
    {
        get => _engine;
        set
        {
            Assert(value != null);
            if (this._engine != value)
                Clear();
            this._engine = value;
            Update();
        }
    }
    bool _playable = false;
    public bool Playable
    {
        get => _playable;
        set
        {
            _playable = value;
            Update();
        }
    }
    public Game.GameLocalAgent Player { get; set; }
    List<Tile3D> _tiles = new List<Tile3D>();
    bool _calledUpdate = false;
    void Clear()
    {
        foreach (var it in _tiles)
        {
            DestroyNode(it);
        }
        _tiles.Clear();
    }
    public static int FigureTileRotation(Tile tile)
    {
        Assert(tile.MetaData is string);
        var unchanged = TileDataLoader.LoadTile((string)tile.MetaData);
        for (int i = 0; i < GameEngine.N_SIDES; i++)
        {
            if (RepeatN(GameEngine.N_SIDES * GameEngine.N_CONNECTORS, indx =>
                 tile.Connections[indx].INode.Index == unchanged.Connections[indx].INode.Index &&
                 tile.Connections[indx].Type == unchanged.Connections[indx].Type).TrueForAll(it => it))
            {
                return i;
            }
            unchanged.Rotate(1);
        }
        throw new Exception();
    }
    void AttachTile(Tile3D t)
    {
        if (t.GetParent() != null)
            t.GetParent().RemoveChild(t);
        AddChild(t);
    }
    void DetachTile(Tile3D t)
    {
        Assert(t.GetParent() == this);
        t.GetParent().RemoveChild(t);
    }
    void RepresentTile(Tile tile)
    {
        var t = new Tile3D(tile, FigureTileRotation(tile), _rng);
        _tiles.Add(t);
        AttachTile(t);
    }
    void ClearInteractible()
    {
        _potentialTile3Ds.ForEach(it => DestroyNode(it));
        _potentialTile3Ds.Clear();
        _potDict.Clear();
        _tiles.ForEach(it => it.ClearPotentialPlacements());
    }
    void UpdateInteractible()
    {
        ClearInteractible();
        if (!_playable)
        {
            return;
        }
        if (NextTile == null || NextTile.AssociatedTile != Engine.CurrentTile)
        {
            NextTile = new Tile3D(Engine.CurrentTile, 0, _rng);
        }
        if (Engine.CurrentState == GameEngine.State.PLACE_TILE)
        {
            foreach (var it in Engine.PossibleTilePlacements())
            {
                if (_potDict.ContainsKey(it.pos))
                {
                    _potDict[it.pos].AddRotation(it.rot);
                    continue;
                }
                var pot = Globals.PotentialTilePacked.Instance<PotentialTile3D>();
                pot.PTile = NextTile;
                pot.PotentialPosition = it.pos;
                pot.AddRotation(it.rot);
                pot.OnPlaceHandle = (Vector2I pos, int rot) =>
                {
                    Player.PlaceTile(pos, rot);
                    AttachTile(NextTile);
                    NextTile.Rot = rot;
                    NextTile.Pos = pos;
                    _tiles.Add(NextTile);
                };
                _potentialTile3Ds.Add(pot);
                _potDict.Add(pot.PotentialPosition, pot);
                AddChild(pot);
            }
        }
        else if (Engine.CurrentState == GameEngine.State.PLACE_PAWN)
        {
            var current = _tiles.Find(it => it.AssociatedTile == Engine.CurrentTile);
            Assert(current != null);
            Engine.PossibleMeepleAttributePlacements().ForEach(it => current.AddPotentialAttributePlacement(Player, it));
            Engine.PossibleMeepleNodePlacements().ForEach(it => current.AddPotentialNodePlacement(Player, it));
        }

    }
    void RealUpdate()
    {
        Assert(Engine != null);
        _calledUpdate = false;
        var unplaced = _engine.map.GetPlacedTiles().FindAll(tile => _tiles.FindIndex(it => it.AssociatedTile == tile) == -1);
        foreach (var it in unplaced)
        {
            RepresentTile(it);
        }
        foreach (var player in _engine.Players)
        {
            player.Pawns.FindAll(m => m.IsInPlay).ForEach(_m =>
            {
                var m = _m as Meeple;
                if (m == null)
                    return;
                var tile3d = this._tiles.Find(it => it.AssociatedTile == m.CurrentTile);
                Assert(tile3d != null);
                var owner = (Game.GameAgent)_game.GetAgent((Player)m.Owner);
                if (m.Container  is Tile.TileAttribute attr)
                {
                    int indx = attr.tile.Attributes.IndexOf(attr);
                    tile3d.AddAttributePlacement(owner, attr.tile.Attributes.IndexOf(attr));
                }
                else if (m.Container  is InternalNode node)
                {
                    tile3d.AddNodePlacement(owner, node.Index);
                }
                else
                    Assert(false);
            });

        }
        UpdateInteractible();
    }
    public void Update()
    {
        Assert(Engine != null);
        if (_calledUpdate)
            return;
        _calledUpdate = true;
        CallDeferred("RealUpdate");
    }

    public override void _Process(float delta)
    {

    }
    public TileMap3D(Game game)
    {
        System.Diagnostics.Debug.WriteLine("Creating new tile! Tile type: ");
        this._game = game;
        this.Engine = game.Engine;
    }
}
