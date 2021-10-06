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


public class Test2D : Node2D
{
    public static Engine.Tile TileGenerator(string name)
    {
        List<Engine.Tile.InternalNode> nodes = new List<Engine.Tile.InternalNode>();
        List<Engine.Tile.Connection> conns  = new List<Engine.Tile.Connection>();
        void GenerateConns<T>(int nodeindex, int n) where T : Engine.Tile.InternalNode, new()
        {
            while(nodeindex >= nodes.Count)
                nodes.Add(null);
            if(nodes[nodeindex] == null)
                nodes[nodeindex] = new T();
            Assert(nodes[nodeindex].type == new T().type);
            for(int i = 0; i < n; i++)
                conns.Add(new Engine.Tile.Connection(nodes[nodeindex]));
        }
        switch(name)
        {
            case "Base/Starter":
                GenerateConns<Engine.FarmNode>(0, 4);
                GenerateConns<Engine.RoadNode>(1, 1);
                GenerateConns<Engine.FarmNode>(2, 2);
                GenerateConns<Engine.RoadNode>(3, 1);
                GenerateConns<Engine.FarmNode>(4, 2);
                GenerateConns<Engine.RoadNode>(5, 1);
                GenerateConns<Engine.FarmNode>(0, 1);
                break;
            case "Base/RoadCross":
                GenerateConns<Engine.FarmNode>(0, 4);
                GenerateConns<Engine.RoadNode>(1, 1);
                GenerateConns<Engine.FarmNode>(2, 2);
                GenerateConns<Engine.RoadNode>(3, 1);
                GenerateConns<Engine.FarmNode>(4, 2);
                GenerateConns<Engine.RoadNode>(5, 1);
                GenerateConns<Engine.FarmNode>(0, 1);
                break;
            case "Base/RoadStraight":
                GenerateConns<Engine.FarmNode>(0, 4);
                GenerateConns<Engine.RoadNode>(1, 1);
                GenerateConns<Engine.FarmNode>(2, 5);
                GenerateConns<Engine.RoadNode>(1, 1);
                GenerateConns<Engine.FarmNode>(0, 1);
                break;
            case "Base/RoadTurn":
                GenerateConns<Engine.FarmNode>(0, 7);
                GenerateConns<Engine.RoadNode>(1, 1);
                GenerateConns<Engine.FarmNode>(2, 2);
                GenerateConns<Engine.RoadNode>(1, 1);
                GenerateConns<Engine.FarmNode>(0, 1);
                break;
            default:
                GD.PrintErr("Failed to find node " + name);
                return null;
        }
        Assert(!nodes.Contains(null));
        Assert(conns.Count == Engine.N_CONNECTORS*Engine.N_SIDES);
        return new Engine.Tile(nodes.ToArray(), conns.ToArray());
    }
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        
    }
}
