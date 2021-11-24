/*
    *** API.cs ***

    Helper functions, meant to facilitate interaction between the engine and the outside world.
*/

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
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

namespace Carcassonne
{
    public partial class GameEngine
    {
        ///<summary>The current state of the game, do note that most functions will assert against this state.</summary>
        public State CurrentState { get; protected set; }
        ///<summary>The current tile.</summary>
        public Tile CurrentTile => _tileManager.CurrentTile();
        ///<summary>Equivalent to CurrentState == State.GAME_OVER</summary>
        public bool IsGameOver { get => CurrentState == State.GAME_OVER; }
        ///<summary>The current turn. Incremented whenever CurrentPlayer chnages.</summary>
        public uint Turn { get; protected set; }
        ///<summary>A list of players. The order of play should be directly equitable to this list.</summary>
        public List<Player> Players { get => _players.ToList(); }
        ///<summary>The list of Actions that resulted in the current state of the GameEngine.</summary>
        public List<GameEngine.Action> History { get => _history.ToList(); }
        ///<summary>
        /// Places CurrentTile.
        /// Following this, one of the following things will happen:
        /// 0. CurrentState will switch to State.PLACE_PAWN.
        /// 1. If there are no valid pawn placements, CurrentState will switch to State.PLACE_TILE and the player will change.
        /// 2. If there are no valid pawn placements, and there are no tiles left, CurrentState will change to State.GAME_OVER
        ///</summary>
        public void PlaceCurrentTile(Vector2I pos, int rot)
        {
            AssertState(State.PLACE_TILE);
            PlaceTileAction act = new PlaceTileAction(pos, rot);
            ExecuteAction(act);
        }
        ///<summary>
        /// Skips placing a pawn.
        /// Following this, one of the following things will happen:
        /// 0. CurrentState will switch to State.PLACE_TILE and CurrentPlayer will change.
        /// 1. If there are no tiles left to place, CurrentState will change to State.GAME_OVER.
        ///</summary>
        public void SkipPlacingPawn()
        {
            AssertState(State.PLACE_PAWN);
            ExecuteAction(new SkipPawnAction());
        }

        ///<summary>
        /// Places one of CurrentPlayers free meeples on CurrentTile's node specified by `index`
        /// Following this, one of the following things will happen:
        /// 0. CurrentState will switch to State.PLACE_TILE and CurrentPlayer will change.
        /// 1. If there are no tiles left to place, CurrentState will change to State.GAME_OVER.
        ///</summary>
        public void PlacePawnOnNode(int index)
        {
            AssertState(State.PLACE_PAWN);
            var act = new PlacePawnAction(index, false);
            ExecuteAction(act);
        }
        ///<summary>
        /// Places one of CurrentPlayers free meeples on CurrentTile's attribute specified by `index`
        /// Following this, one of the following things will happen:
        /// 0. CurrentState will switch to State.PLACE_TILE and CurrentPlayer will change.
        /// 1. If there are no tiles left to place, CurrentState will change to State.GAME_OVER.
        ///</summary>
        public void PlacePawnOnAttribute(int index)
        {
            AssertState(State.PLACE_PAWN);
            var act = new PlacePawnAction(index, true);
            ExecuteAction(act);
        }
        ///<summary>
        /// Returns the next tile. 
        /// Do note that it may differ from the actual next tile, as the engine might skip tiles in order to avoid a deadlock.
        ///</summary>
        public Tile GetUpcomingTile()
        {
            return _tileManager.PeekTile();
        }
        ///<summary>Returns queued tiles in order that they will appear in. See GetUpcomingTile for remarks.</summary>
        public List<Tile> GetQueuedTiles()
        {
            return _tileManager.TileQueue.ToList();
        }
        ///<summary>Returns a list of players with the highest score, more than one item in the resulting list indicates a draw.</summary>
        public List<Player> GetWinners()
        {
            List<Player> ret = Players.ToList();
            ret.Sort((p0, p1) => (p0.EndScore).CompareTo(p1.EndScore));
            return Players.FindAll(p => p.EndScore == Players[0].EndScore);
        }
        ///<summary>Returns a list of valid values for PlaceCurrentTile.</summary>
        public List<(Vector2I pos, int rot)> PossibleTilePlacements()
        {
            AssertState(State.PLACE_TILE);
            Assert(_tileManager.CurrentTile() != null);

            return map.TryFindAllFits(_tileManager.CurrentTile());
        }
        ///<summary>Returns a list of valid values for PlacePawnOnNode.</summary>
        public List<int> PossibleMeepleNodePlacements()
        {
            AssertState(State.PLACE_PAWN);

            return GetPossibleMeeplePlacements(CurrentPlayer, _lastTile)
                .FindAll(o => o is InternalNode)
                .ConvertAll(o => ((InternalNode)o).Index);
        }
        ///<summary>Returns a list of valid values for PlacePawnOnAttribute.</summary>
        public List<int> PossibleMeepleAttributePlacements()
        {
            AssertState(State.PLACE_PAWN);

            return GetPossibleMeeplePlacements(CurrentPlayer, _lastTile)
                .FindAll(o => o is Tile.TileAttribute)
                .ConvertAll(o => _lastTile.Attributes.IndexOf((Tile.TileAttribute)o));
        }
        ///<summary>Returns a list of graphs owned by a given player, contested indicates whether there are other players with equivalent stake in the graph.</summary>
        public List<(Map.Graph graph, bool contested)> GetGraphsOwnedBy(Player player)
        {
            return map.Graphs
                .FindAll(g => GetGraphOwners(g).Contains(player))
                .ConvertAll(g => (g, GetGraphOwners(g).Count > 1)); // soooo inefficient
        }
        ///<summary>Returns a list of players with an equivalent stake in the given graph.</summary>
        public List<Player> ListGraphOwners(Map.Graph graph)
        {
            return GetGraphOwners(graph).FindAll(a => a is Player).ConvertAll(a => (Player)a);
        }
        ///<summary>Returns a list of players having a stake in the given graph, as well as their stake.</summary>
        public List<(Player player, int stake)> ListGraphStakeholders(Map.Graph graph)
        {
            return GetGraphStakeholders(graph).FindAll(a => a.agent is Player).ConvertAll(a => (((Player)a.agent), a.stake));
        }

        ///<summary>Returns the next player. Does not account for the game ending.</summary>
        public Player PeekNextPlayer()
        {
            int indx = _players.IndexOf(CurrentPlayer);
            Assert(indx >= 0);
            return _players[(indx + 1) % _players.Count];
        }
        ///<summary>Returns the number of free meeples owned by the player.</summary>
        public int CountPlayerMeeplesLeft(Player player)
        {
            return player.Pawns.Count(p => p is Meeple && !p.IsInPlay);
        }
        ///<summary>Returns the number of placed meeples owned by the player.</summary>
        public int CountPlayerMeeplesPlaced(Player player)
        {
            return player.Pawns.Count(p => p is Meeple && p.IsInPlay);
        }

    }
}
