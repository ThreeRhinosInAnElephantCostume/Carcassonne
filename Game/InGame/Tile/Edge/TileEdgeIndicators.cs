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

    const float EMISSION_CHANGE_FACTOR = 0.8f;
    const float EMISSION_CHANGE_SPEED = 0.004f;
    const float EMISSION_MIN = 0.8f;
    const float EMISSION_MAX = 2f;
    readonly EmissionCurve _emissionCurve =
        new Utils.EmissionCurve(EMISSION_CHANGE_FACTOR, EMISSION_CHANGE_SPEED, EMISSION_MIN, EMISSION_MAX);
    MeshInstance _outline;
    Spatial _connectionIndicatorRoot;
    readonly List<ConnectionIndicator> _indicators = new List<ConnectionIndicator>();
    void SetUpConnectionFromTile(Tile3D tile, int indx)
    {
        int i = AbsMod((indx - (GameEngine.N_TOTAL_CONNECTORS / 2)), (GameEngine.N_CONNECTORS * GameEngine.N_SIDES));
        i = AbsMod((i - tile.Rot * GameEngine.N_CONNECTORS), GameEngine.N_TOTAL_CONNECTORS);
        var con = tile.AssociatedTile.Connections[i];
        _indicators[indx].Type = con.INode.Type;
        //Console.WriteLine($"indx: {indx} i: {i} type: {con.INode.Type}");
        _indicators[indx].Owner = null;
    }
    void SetUp(Game game, Vector2I pos, Tile3D tile = null)
    {
        var map = game.Engine.map;
        int i = 0;
        var revn = (pos.Neighbours.Skip(2).Concat(pos.Neighbours.Take(2))).ToList();
        foreach (var npos in revn)
        {
            var n = map[npos];
            if (n != null)
            {
                for (int ii = i; ii < i + 3; ii++)
                {
                    //int oii = AbsMod(ii-(GameEngine.N_TOTAL_CONNECTORS/2), GameEngine.N_TOTAL_CONNECTORS);
                    int oii = ii;
                    _indicators[ii].Type = n.Connections[oii].INode.Type;
                    if (n.Connections[oii].INode.Graph == null)
                    {
                        _indicators[ii].OwningAgent = null;
                        continue;
                    }
                    var owners = game.Engine.ListGraphOwners(n.Connections[oii].INode.Graph);
                    if (owners.Count != 1)
                    {
                        _indicators[ii].OwningAgent = null;
                        continue;
                    }
                    _indicators[ii].OwningAgent = game.GetAgent(owners[0]);
                }
            }
            else if (tile != null)
            {
                for (int ii = i; ii < i + 3; ii++)
                {
                    SetUpConnectionFromTile(tile, ii);
                }
            }
            else
            {
                for (int ii = i; ii < i + 3; ii++)
                {
                    _indicators[ii].Owner = null;
                    _indicators[ii].Type = NodeType.ERR;
                }
            }
            i += 3;
        }
    }
    public void SetUpFromNeighbours(Game game, Vector2I pos)
    {
        Reset();
        SetUp(game, pos);
    }
    public void SetUpFromTile(Tile3D tile)
    {
        Reset();
        for (int _i = 0; _i < tile.AssociatedTile.Connections.Count; _i++)
        {
            SetUpConnectionFromTile(tile, _i);
        }
    }
    public void SetUpFromNeighboursAndtile(Game game, Vector2I pos, Tile3D tile)
    {
        Reset();
        SetUp(game, pos, tile);
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
    public override void _Process(float delta)
    {
        var mat = (SpatialMaterial)_outline.GetActiveMaterial(0);
        mat.EmissionEnergy = _emissionCurve.Next(delta);
    }
}
