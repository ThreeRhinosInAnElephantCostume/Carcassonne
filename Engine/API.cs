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

namespace Carcassonne
{
    public partial class GameEngine
    {
        public State CurrentState { get; protected set; }
        public bool IsGameOver { get => CurrentState == State.GAME_OVER; }
        public uint Turn { get; protected set; }
        public List<Player> Players { get => _players.ToList(); }
        public List<GameEngine.Action> History { get => History.ToList(); }
        public int GetPlayerScore(Player player)
        {
            return basescore[player];
        }
        public int GetPlayerEndScore(Player player)
        {
            throw new NotImplementedException();
        }
        public int GetPlayerProbableScore(Player player)
        {
            throw new NotImplementedException();
        }
        public void PlaceCurrentTile(Vector2I pos, int rot)
        {
            PlaceTileAction act = new PlaceTileAction(pos, rot);
            ExecuteAction(act);
        }
        public void SkipPlacingPawn()
        {
            ExecuteAction(new SkipPawnAction());
        }
        public Tile GetCurrentTile()
        {
            return tilemanager.CurrentTile();
        }
        public Tile GetUpcomingTile()
        {
            return tilemanager.PeekTile();
        }
        public List<Tile> GetQueuedTiles()
        {
            return tilemanager.TileQueue.ToList();
        }

        public List<(Vector2I pos, int rot)> PossiblePlacements()
        {
            AssertState(State.PLACE_TILE);
            Assert(tilemanager.CurrentTile() != null);

            return map.TryFindAllFits(tilemanager.CurrentTile());
        }


        public Player PeekNextPlayer()
        {
            int indx = _players.IndexOf(CurrentPlayer);
            Assert(indx >= 0);
            return _players[(indx + 1) % _players.Count];
        }



    }
}
