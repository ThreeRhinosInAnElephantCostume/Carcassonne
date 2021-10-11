using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Threading;
using Carcassonne;
using ExtraMath;
using Godot;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

[Serializable]
public class TilePrototype
{
    [Export]
    public int[] NodeTypes { get; set; }
    [Export]
    public int[] Assignments { get; set; }
    [Export]
    public bool UserEditable { get; set; }
    public Dictionary<int, List<int>> NodeAttributes = new Dictionary<int, List<int>>();
    public List<int> TileAttributes = new List<int>();
    public bool IsValid
    {
        get
        {
            return
                (Assignments != null && NodeTypes != null) &&
                (Assignments.Length == GameEngine.N_CONNECTORS * GameEngine.N_SIDES) &&
                (NodeTypes.Length > 0) &&
                (!NodeTypes.ToList().Contains((int)NodeType.ERR)) &&
                (Assignments.ToList().FindAll(i => (i < 0 || i >= NodeTypes.Length)).Count == 0);
        }
    }

    public Tile Convert()
    {
        Assert(NodeTypes != null);
        Assert(NodeTypes.Length > 0);
        Assert(Assignments != null);
        Assert(Assignments.Length == 12);

        List<InternalNode> nodes = new List<InternalNode>();
        List<Tile.Connection> connections = new List<Tile.Connection>();

        foreach (var it in NodeTypes)
        {
            nodes.Add(new InternalNode((NodeType)it));
        }

        foreach (var it in Assignments)
        {
            Assert(it < nodes.Count && it >= 0);
            connections.Add(new Tile.Connection(nodes[it]));
        }

        var t = new Tile(nodes.ToArray(), connections.ToArray());

        foreach (var it in nodes)
        {
            it.tile = t;
        }

        return t;
    }
    public TilePrototype(NodeType[] nodes, int[] assignments)
    {
        if (nodes == null)
            NodeTypes = new int[0];
        else
        {
            NodeTypes = new int[nodes.Length];
            for (int i = 0; i < NodeTypes.Length; i++)
            {
                NodeTypes[i] = (int)nodes[i];
            }
        }
        if (assignments == null)
            assignments = new int[0];
        this.Assignments = assignments;
        this.UserEditable = false;
    }
    public TilePrototype() : this(null, null)
    {

    }
}
