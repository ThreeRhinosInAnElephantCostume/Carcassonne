/*
    *** BaseGame.cs ***

    Most of the functions used by base game logic live here.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using Carcassonne;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

namespace Carcassonne
{
    public partial class GameEngine
    {
        Tile _lastTile;
        List<TileMonasteryAttribute> _activeMonasteries = new List<TileMonasteryAttribute>();
        bool NextTileEnsurePlaceable()
        {
            if (_tileManager.NextTile() == null)
                return false;
            int tries = 0;
            while (PossibleTilePlacements().Count == 0)
            {
                if (_tileManager.NQueued < tries)
                    return false;
                if (_tileManager.SwapTile() == null)
                    return false;
                tries++;
            }
            return true;
        }
        void EndGame()
        {
            UpdatePoints();
            CurrentState = State.GAME_OVER;
            CurrentPlayer = null;
            return;
        }
        int CalculateScore(Map.Graph g)
        {
            if (g.Type == NodeType.FARM)
            {
                List<Map.Graph> completedcities = new List<Map.Graph>();
                g.Nodes.FindAll(it => it.AttributeTypes.Contains(NodeAttributeType.NEAR_CITY)).ForEach
                (
                    it =>
                    {
                        it.Connections.ForEach
                        (
                            c =>
                            {
                                int indx = it.ParentTile.Connections.IndexOf(c);
                                var p = AbsMod(indx - 1, N_CONNECTORS * N_SIDES);
                                var n = AbsMod(indx + 1, N_CONNECTORS * N_SIDES);
                                void f(Map.Graph g)
                                {
                                    if (!completedcities.Contains(g) && g.Type == NodeType.CITY && g.IsClosed)
                                        completedcities.Add(g);
                                };
                                f(it.ParentTile.Connections[p].INode.Graph);
                                f(it.ParentTile.Connections[n].INode.Graph);
                            }
                        );
                    }
                );
                Assert(completedcities.TrueForAll(it => it.Type == NodeType.CITY && it.IsClosed));
                return FARM_PER_CITY_POINTS * completedcities.Count;
            }
            bool closed = g.IsClosed;
            int n = g.Tiles.Count;
            int points = 0;
            if (g.Type == NodeType.CITY && g.Tiles.Count < CITY_SMALL_THRESHOLD)
            {
                points = CITY_SMALL_POINTS * n +
                CITY_COMPLETE_BONUS_POINTS * g.Nodes.Count(n => n.AttributeTypes.Contains(NodeAttributeType.CITY_BONUS));
            }
            else
            {
                points = (g.Type, closed) switch
                {
                    (NodeType.CITY, true) =>
                        n * CITY_COMPLETE_POINTS +
                        CITY_COMPLETE_BONUS_POINTS * g.Nodes.Count(n => n.AttributeTypes.Contains(NodeAttributeType.CITY_BONUS)),
                    (NodeType.CITY, false) =>
                        n * CITY_INCOMPLETE_POINTS +
                        CITY_INCOMPLETE_BONUS_POINTS * g.Nodes.Count(n => n.AttributeTypes.Contains(NodeAttributeType.CITY_BONUS)),
                    (NodeType.ROAD, true) =>
                        n * ROAD_COMPLETE_POINTS,
                    (NodeType.ROAD, false) =>
                        n * ROAD_INCOMPLETE_POINTS,
                    _ => throw new Exception(),
                };
            }
            return points;
        }
        void UpdatePoints()
        {
            foreach (var it in Players)
                it.PotentialScore = 0;
            foreach (var g in map.Graphs)
            {
                if (g.Owners.Count == 0)
                    continue;
                int score = CalculateScore(g);
                bool complete = (g.IsClosed && g.Type != NodeType.FARM);
                foreach (var owner in GetGraphOwners(g))
                {
                    Assert(owner is Player);
                    Player p = (Player)owner;
                    if (complete)
                    {
                        p.Score += score;
                    }
                    else
                    {
                        p.PotentialScore += score;
                    }
                }
                if (complete)
                {
                    g.Owners.FindAll(o => (o is Meeple)).ForEach(m => ((Meeple)m).Remove());
                }
            }
            foreach (var it in _activeMonasteries.FindAll(o => (o.Owner is Meeple)))
            {
                Assert(it.Owner != null);
                int n = MONASTERY_NEIGHBOURS.Count(pos => map[it.tile.Position + pos] != null);
                var m = (Meeple)it.Owner;
                Player p = (Player)m.Owner;
                if (n == MONASTERY_NEIGHBOURS.Count)
                {
                    _activeMonasteries.Remove(it);
                    m.Remove();
                    p.Score += n * MONASTERY_COMPLETE_POINTS;
                }
                else
                {
                    p.PotentialScore += n * MONASTERY_INCOMPLETE_POINTS;
                }
            }
        }
        List<(Agent agent, int stake)> GetGraphStakeholders(Map.Graph g)
        {
            List<(Agent owner, int stake)> owners = new List<(Agent, int)>();
            g.Owners.ForEach
            (
                it =>
                {
                    if (it is Occupier occupier)
                    {
                        Assert(occupier.HasOwner);
                        int indx = owners.FindIndex(it => it.owner == occupier.Owner);
                        if (indx == -1)
                            owners.Add((occupier.Owner, occupier.Weight));
                        else
                        {
                            var dt = owners[indx];
                            owners[indx] = (dt.owner, dt.stake + occupier.Weight);
                        }
                    }
                }
            );
            owners.Sort((v0, v1) => (-v0.stake).CompareTo(-v1.stake)); // reverse sort (descending)
            return owners;
        }
        List<Agent> GetGraphOwners(Map.Graph g)
        {
            var ret = new List<Agent>();
            var stakeholders = GetGraphStakeholders(g);
            if (stakeholders.Count > 0)
            {
                foreach (var it in stakeholders)
                {
                    if (it.stake < stakeholders[0].stake)
                        break;
                    ret.Add(it.agent);
                }
            }
            return ret;
        }
        int GetFreeMeepleCount(Player player)
        {
            return player.Pawns.Count(it => it is Meeple && !it.IsInPlay);
        }
        Meeple GetFreeMeeple(Player player)
        {
            var ret = player.Pawns.Find(it => it is Meeple && !it.IsInPlay);
            Assert(ret != null);
            return (Meeple)ret;
        }
        List<object> GetPossibleMeeplePlacements(Player player, Tile tile)
        {
            if (GetFreeMeepleCount(player) == 0)
                return new List<object>();
            var nodes = tile.Nodes.FindAll((InternalNode n) => GetGraphOwners(n.Graph).Count == 0).ToList<object>();
            var mons = tile.Attributes.FindAll(it => it is TileMonasteryAttribute && ((TileMonasteryAttribute)it).Owner == null).ToList<object>();
            return nodes.Concat(mons).ToList();
        }
        public class TileMonasteryAttribute : Tile.TileAttribute
        {
            public Occupier Owner { get; set; } = null;
            public TileMonasteryAttribute(Tile tile) : base(tile, TileAttributeType.MONASTERY)
            {

            }
        }
        public static GameEngine CreateBaseGame(IExternalDataSource datasource, ulong seed, int players, string tileset)
        {
            Assert(seed != 0, "Invalid seed - some random generators might not like it");
            Assert(players > 1);
            Assert(tileset != null);

            GameEngine eng = new GameEngine(datasource);
            eng.ExecuteAction(new StartBaseGameAction(seed, tileset, players));
            return eng;
        }
    }
}
