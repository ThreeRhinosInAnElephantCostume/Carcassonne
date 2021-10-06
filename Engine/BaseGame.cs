using System.Reflection.PortableExecutable;
using Godot;


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

public partial class Engine
{

    public class RoadNode : Tile.InternalNode
    {
        public static Tile.NodeType t = new Tile.NodeType("Road", 'R');
        public override Tile.NodeType type => t;
    }
    public class CityNode : Tile.InternalNode
    {
        public static Tile.NodeType t = new Tile.NodeType("City", 'C');
        public override Tile.NodeType type => t;
    }
    public class FarmNode : Tile.InternalNode
    {
        public static Tile.NodeType t = new Tile.NodeType("Farm", 'F');
        public override Tile.NodeType type => t;
    }
    public class BaseGameTileset : Tileset
    {
        public override bool HasStarter {get => true;}
        public override Tile Starter => Engine.GenerateTile("Base/Starter");
        public override uint NDefaultTiles {get => 71;}
        protected override List<Tile> _GenerateTiles(uint n)
        {
            List<Tile> l = new List<Tile>((int)NDefaultTiles);

            l.AddRange(Engine.GenerateTiles("Base/RoadCross", 16));
            l.AddRange(Engine.GenerateTiles("Base/RoadStraight", 31));
            l.AddRange(Engine.GenerateTiles("Base/RoadTurn", 24));

            // // 16
            // l.AddRange(Engine.GenerateTiles("Base/CityBellend", 4));
            // l.AddRange(Engine.GenerateTiles("Base/CityBellendRoadEnd", 4));
            // l.AddRange(Engine.GenerateTiles("Base/CityBellendRoadStraight", 4));
            // l.AddRange(Engine.GenerateTiles("Base/CityBellendRoadCross", 4));

            // // 23
            // l.AddRange(Engine.GenerateTiles("Base/CityCentre", 3));
            // l.AddRange(Engine.GenerateTiles("Base/CityCorridor", 4));
            // l.AddRange(Engine.GenerateTiles("Base/CityTurn", 8));
            // l.AddRange(Engine.GenerateTiles("Base/CityOpening", 4));
            // l.AddRange(Engine.GenerateTiles("Base/CityOpeningRoadEnd", 4));

            // // 32
            // l.AddRange(Engine.GenerateTiles("Base/RoadCross", 8));
            // l.AddRange(Engine.GenerateTiles("Base/RoadStraight", 8));
            // l.AddRange(Engine.GenerateTiles("Base/RoadTurn", 16));

            Assert(l.Count == n);
            return l;
        }
        
    }
    public class PlaceTileAction : Action
    {
        public Vector2I pos;
        public int rot;
        public PlaceTileAction(Vector2I pos, int rot)
        {
            this.IsFilled = true;
            this.pos = pos;
            this.rot = rot;
        }
    }
    [ActionExec(typeof(PlaceTileAction))]
    void PlaceCurrentTileExec(Action _act)
    {
        var act = (PlaceTileAction) _act;

        AssertState(State.PLACE_TILE);
        Tile c = tilemanager.CurrentTile();

        c.Rotate(act.rot);
        if(!map.CanPlaceTile(c, act.pos))
        {
            c.Rotate(-act.rot);
            throw new Exception("INVALID TILE PLACEMENT");
        }
        map.PlaceTile(c, act.pos);

        if(tilemanager.NextTile() == null)
        {
            CurrentState = State.GAME_OVER;
            CurrentPlayer = null;
            return;
        }

        CurrentState = State.PLACE_PAWN;

    }
    public class SkipPawnAction : Action
    {
        public SkipPawnAction()
        {
            this.IsFilled = true;
        }
    }
    [ActionExec(typeof(SkipPawnAction))]
    void SkipPawnExec(Action _act)
    {
        var act = (SkipPawnAction) _act;

        CurrentState = State.PLACE_TILE;

        if(tilemanager.NextTile() == null)
        {
            CurrentState = State.GAME_OVER;
            CurrentPlayer = null;
            return;
        }
        NextPlayer();
    }
    protected class StartBaseGameAction : Action
    {
        public ulong seed;
        public int players;
        public StartBaseGameAction(ulong seed, int players)
        {
            IsFilled = true;
            this.players = players;
            this.seed = seed;
        }
    }
    [ActionExec(typeof(StartBaseGameAction))]
    void StartBaseGameExec(Action _act)
    {
        var act = (StartBaseGameAction)_act;

        rng = new RNG(act.seed);

        Assert(act.players >= MIN_PLAYERS && act.players <= MAX_PLAYERS);

        tilemanager = new TileManager(this);

        BaseGameTileset tileset = new BaseGameTileset();

        tilemanager.AddTiles(tileset.GenerateTiles(), true);

        tilemanager.NextTile();

        for(int i = 0; i < act.players; i++)
        {
            AddPlayer();
        }
        CurrentPlayer = _players[0];

        CurrentState = State.PLACE_TILE;

        Assert(tileset.Starter != null && tileset.HasStarter);

        map = new Map(tileset.Starter);
    }
    public static Engine CreateBaseGame(ulong seed, int players)
    {
        Engine eng = new Engine();
        eng.ExecuteAction(new StartBaseGameAction(seed, players));
        return eng;
    }
}