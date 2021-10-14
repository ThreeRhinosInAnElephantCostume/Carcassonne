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
        public List<Player> GetWinners()
        {
            List<Player> ret = Players.ToList();
            ret.Sort((p0, p1) => (p0.EndScore).CompareTo(p1.EndScore));
            return Players.FindAll(p => p.EndScore == Players[0].EndScore);
        }
        public List<(Vector2I pos, int rot)> PossibleTilePlacements()
        {
            AssertState(State.PLACE_TILE);
            Assert(tilemanager.CurrentTile() != null);

            return map.TryFindAllFits(tilemanager.CurrentTile());
        }

        public List<int> PossibleMeepleNodePlacements()
        {
            AssertState(State.PLACE_PAWN);

            return GetPossibleMeeplePlacements(_lastTile)
                .FindAll(o => o is InternalNode)
                .ConvertAll(o => ((InternalNode)o).Index);
        }
        public List<int> PossibleMeepleAttributePlacements()
        {
            AssertState(State.PLACE_PAWN);

            return GetPossibleMeeplePlacements(_lastTile)
                .FindAll(o => o is Tile.TileAttribute)
                .ConvertAll(o => _lastTile.Attributes.IndexOf((Tile.TileAttribute)o));
        }
        public Vector2I CurrentPawnTarget()
        {
            AssertState(State.PLACE_PAWN);

            return _lastTile.Position;
        }

        public void PlacePawnOnNode(int index)
        {
            var act = new PlacePawnAction(index, false);
            ExecuteAction(act);
        }
        public void PlacePawnOnAttribute(int index)
        {
            var act = new PlacePawnAction(index, false);
            ExecuteAction(act);
        }

        public List<(Map.Graph graph, bool contested)> GetGraphsOwnedBy(Player player)
        {
            return map.Graphs
                .FindAll(g => GetGraphOwners(g).Contains(player))
                .ConvertAll(g => (g, GetGraphOwners(g).Count > 1));
        }


        public Player PeekNextPlayer()
        {
            int indx = _players.IndexOf(CurrentPlayer);
            Assert(indx >= 0);
            return _players[(indx + 1) % _players.Count];
        }

        public int CountPlayerMeeplesLeft(Player player)
        {
            return player.Pawns.Count(p => p is Meeple && !p.IsInPlay);
        }
        public int CountPlayerMeeplesPlaced(Player player)
        {
            return player.Pawns.Count(p => p is Meeple && p.IsInPlay);
        }

    }
}
