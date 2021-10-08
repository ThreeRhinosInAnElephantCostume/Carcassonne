﻿using System;
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

public class TilePrototype : Resource
{
    [Export]
    public NodeTypePrototype[] Nodes;
    [Export]
    public int[] Assignments;
    [Export]
    public bool UserEditable;


    public Tile Convert()
    {
        Assert(Nodes != null);
        Assert(Nodes.Length > 0);
        Assert(Assignments != null);
        Assert(Assignments.Length == 12);

        List<InternalNode> nodes = new List<InternalNode>();
        List<Tile.Connection> connections = new List<Tile.Connection>();

        foreach (var it in Nodes)
        {
            nodes.Add(new InternalNode(it.Convert()));
        }

        foreach (var it in Assignments)
        {
            Assert(it < nodes.Count && it >= 0);
            connections.Add(new Tile.Connection(nodes[it]));
        }

        return new Tile(nodes.ToArray(), connections.ToArray());

    }
    public TilePrototype(NodeTypePrototype[] nodes = null, int[] assignments = null)
    {
        if (nodes == null)
            nodes = new NodeTypePrototype[0];
        if (assignments == null)
            assignments = new int[0];
        this.Nodes = nodes;
        this.Assignments = assignments;
        this.UserEditable = false;
    }

}
