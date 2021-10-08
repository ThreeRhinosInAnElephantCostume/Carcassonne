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

public static class TileGenerator
{
    public static TilePrototype LoadPrototype(string name)
    {
        List<int> assignments = new List<int>();
        List<NodeTypePrototype> nodes = new List<NodeTypePrototype>();
        void GenerateConns(NodeType t, int nodeindex, int n)
        {
            NodeTypePrototype ntp = new NodeTypePrototype(t);
            while (nodeindex >= nodes.Count)
                nodes.Add(null);
            if (nodes[nodeindex] == null)
                nodes[nodeindex] = ntp;
            for (int i = 0; i < n; i++)
            {
                assignments.Add(nodeindex);
            }
            Assert(assignments.Count <= N_SIDES * N_CONNECTORS);

        }
        switch (name)
        {
            case "Base/Starter":
                GenerateConns(GameEngine.FarmType, 0, 4);
                GenerateConns(GameEngine.RoadType, 1, 1);
                GenerateConns(GameEngine.FarmType, 2, 2);
                GenerateConns(GameEngine.RoadType, 3, 1);
                GenerateConns(GameEngine.FarmType, 4, 2);
                GenerateConns(GameEngine.RoadType, 5, 1);
                GenerateConns(GameEngine.FarmType, 0, 1);
                break;
            case "Base/RoadCross":
                GenerateConns(GameEngine.FarmType, 0, 4);
                GenerateConns(GameEngine.RoadType, 1, 1);
                GenerateConns(GameEngine.FarmType, 2, 2);
                GenerateConns(GameEngine.RoadType, 3, 1);
                GenerateConns(GameEngine.FarmType, 4, 2);
                GenerateConns(GameEngine.RoadType, 5, 1);
                GenerateConns(GameEngine.FarmType, 0, 1);
                break;
            case "Base/RoadStraight":
                GenerateConns(GameEngine.FarmType, 0, 4);
                GenerateConns(GameEngine.RoadType, 1, 1);
                GenerateConns(GameEngine.FarmType, 2, 5);
                GenerateConns(GameEngine.RoadType, 1, 1);
                GenerateConns(GameEngine.FarmType, 0, 1);
                break;
            case "Base/RoadTurn":
                GenerateConns(GameEngine.FarmType, 0, 7);
                GenerateConns(GameEngine.RoadType, 1, 1);
                GenerateConns(GameEngine.FarmType, 2, 2);
                GenerateConns(GameEngine.RoadType, 1, 1);
                GenerateConns(GameEngine.FarmType, 0, 1);
                break;
            default:
                GD.PrintErr("Failed to find node " + name);
                return null;
        }
        Assert(!nodes.Contains(null));
        Assert(assignments.Count == GameEngine.N_CONNECTORS * GameEngine.N_SIDES);
        return new TilePrototype(nodes.ToArray(), assignments.ToArray());
    }
    public static EditableTileset debugtileset
    {
        get
        {
            EditableTileset et = new EditableTileset(true);
            List<TilePrototype> tiles = new List<TilePrototype>();
            void AddTiles(string name, int n)
            {
                var tpt = LoadPrototype(name);
                TilePrototype[] tpts = new TilePrototype[n];
                for (int i = 0; i < n; i++)
                {
                    tpts[i] = tpt;
                }
                tiles.AddRange(tpts);
            }
            AddTiles("Base/RoadCross", 16);
            AddTiles("Base/RoadStraight", 31);
            AddTiles("Base/RoadTurn", 24);
            et.SingleStarter = true;
            et.NOutputTiles = 71;
            et.NPossibleTiles = 71;
            et.StarterTiles = new TilePrototype[] { LoadPrototype("Base/Starter") };
            et.Tiles = tiles.ToArray();
            return et;
        }
    }
}
