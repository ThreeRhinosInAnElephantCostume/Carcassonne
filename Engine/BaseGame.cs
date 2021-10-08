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

using Carcassonne;
using static Carcassonne.GameEngine;

namespace Carcassonne
{
    public partial class GameEngine
    {

        public const int RoadID  = -1;
        public const int CityID = -2;
        public const int FarmID = -3;
        public static NodeType RoadType = new NodeType(unchecked((uint)RoadID), "Road", 'R');
        public static NodeType CityType = new NodeType(unchecked((uint)CityID), "City", 'C');
        public static NodeType FarmType = new NodeType(unchecked((uint)FarmID), "Farm", 'F');
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
            public ITileset tileset;
            public StartBaseGameAction(ulong seed, ITileset tileset, int players)
            {
                IsFilled = true;
                this.players = players;
                this.seed = seed;
                this.tileset = tileset;
            }
        }
        [ActionExec(typeof(StartBaseGameAction))]
        void StartBaseGameExec(Action _act)
        {
            var act = (StartBaseGameAction)_act;

            rng = new RNG(act.seed);

            Assert(act.players >= MIN_PLAYERS && act.players <= MAX_PLAYERS);

            tilemanager = new TileManager(this);

            tilemanager.AddTiles(act.tileset.GenerateTiles(rng), true);

            tilemanager.NextTile();

            for(int i = 0; i < act.players; i++)
            {
                AddPlayer();
            }
            CurrentPlayer = _players[0];

            CurrentState = State.PLACE_TILE;

            Assert(act.tileset.HasStarter);

            var starter = act.tileset.GenerateStarter(rng);

            Assert(starter != null);

            map = new Map(starter);
        }
        public static GameEngine CreateBaseGame(ulong seed, int players, ITileset tileset)
        {
            Assert(seed != 0, "Invalid seed - some random generators might not like it");
            Assert(players > 1);
            Assert(tileset != null);

            GameEngine eng = new GameEngine();
            eng.ExecuteAction(new StartBaseGameAction(seed, tileset, players));
            return eng;
        }
    }
}