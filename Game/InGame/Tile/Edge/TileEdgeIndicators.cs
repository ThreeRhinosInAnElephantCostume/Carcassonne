using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Enumeration;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using Carcassonne;
using ExtraMath;
using Godot;
using static System.Math;
using static Utils;

public class TileEdgeIndicators : Spatial
{
    MeshInstance _outline;
    Spatial _connectionIndicatorRoot;
    readonly List<ConnectionIndicator> _indicators = new List<ConnectionIndicator>();
    void SetUpFromNeighboursNoReset(Game game, Vector2I pos)
    {
        var map = game.Engine.map;
        int i = 0;
        foreach (var npos in pos.Neighbours)
        {
            var n = map[npos];
            if (n != null)
            {
                for (int ii = i; ii < i + 3; ii++)
                {
                    int oii = AbsMod(ii + 6, GameEngine.N_CONNECTORS * GameEngine.N_SIDES);
                    _indicators[ii].Type = n.Connections[oii].INode.Graph.Type;
                    var owners = game.Engine.ListGraphOwners(n.Connections[oii].INode.Graph);
                    if (owners.Count != 1)
                    {
                        _indicators[ii].OwningAgent = null;
                        continue;
                    }
                    _indicators[ii].OwningAgent = game.GetAgent(owners[0]);
                }
            }
            i += 3;
        }
    }
    public void SetUpFromNeighbours(Game game, Vector2I pos)
    {
        Reset();
        SetUpFromNeighboursNoReset(game, pos);
    }
    public void SetUpFromTile(Tile3D tile)
    {
        for (int i = 0; i < tile.AssociatedTile.Connections.Count; i++)
        {
            _indicators[i].Type = tile.AssociatedTile.Connections[i].INode.Graph.Type;
            _indicators[i].Owner = null;
        }
    }
    public void SetUpFromNeighboursAndtile(Game game, Vector2I pos, Tile3D tile)
    {
        Reset();
        SetUpFromTile(tile);
        SetUpFromNeighboursNoReset(game, pos);
    }
    public void Reset()
    {
        foreach (var it in _indicators)
        {
            it.Type = NodeType.ERR;
            it.OwningAgent = null;
        }
    }
    public override void _Ready()
    {
        _outline = this.GetNodeSafe<MeshInstance>("Outline");
        _connectionIndicatorRoot = this.GetNodeSafe<Spatial>("ConnectionIndicators");
        for (int i = 0; i < GameEngine.N_CONNECTORS * GameEngine.N_SIDES; i++)
        {
            ConnectionIndicator ind = _connectionIndicatorRoot.GetNodeSafe<ConnectionIndicator>(i.ToString());
            _indicators.Add(ind);
        }
    }

}
