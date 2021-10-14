﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using System.Xml.Linq;
using Carcassonne;
using ExtraMath;
using Godot;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;


namespace Carcassonne
{
    public class Tile
    {
        public class Connection
        {
            public InternalNode INode { get; }
            public Connection Other { get; protected set; } = null;
            public bool IsConnected { get => Other != null; }
            public NodeType Type { get => INode.Type; }
            public void DebugValidate()
            {
                Assert(INode != null);
                if (Other != null)
                {
                    Assert(Other.IsConnected);
                    Assert(Other.Other == this);
                    Assert(Other.Type == this.Type);
                    Assert(Other.INode != this.INode);
                    Assert(this.INode.Graph != null);
                    Assert(Other.INode.Graph == this.INode.Graph);
                }
            }
            public bool CanConnect(Connection other)
            {
                return this.Type == other.Type;
            }
            public void Connect(Connection other)
            {
                Assert(CanConnect(other), "Attempting to connect incompatible connectors");

                this.Other = other;
                if (!other.IsConnected)
                    other.Connect(this);
            }
            public Connection(InternalNode node)
            {
                this.INode = node;
                if (!node.Connections.Contains(this))
                    node.Connections.Add(this);
            }
        }
        public class Side
        {
            public Connection[] Connections { get; protected set; } = new Connection[N_CONNECTORS];
            public Tile Owner { get; protected set; }
            public Tile Other { get; protected set; } = null;
            public bool IsAttached { get => Other != null; }
            public bool CanAttachVerbose(Side side, out List<int> invalidconnections, ref int conindex)
            {
                invalidconnections = new List<int>((int)N_SIDES);
                bool can = true;
                if (side.IsAttached)
                    return false;
                for (uint i = 0; i < N_CONNECTORS; i++)
                {
                    if (!side.Connections[N_CONNECTORS - i - 1].CanConnect(Connections[i]))
                    {
                        can = false;
                        invalidconnections.Add(conindex);
                    }
                    conindex++;
                }
                return can;
            }
            public bool CanAttach(Side side)
            {
                if (side.IsAttached)
                    return false;
                for (uint i = 0; i < N_CONNECTORS; i++)
                {
                    if (!side.Connections[N_CONNECTORS - i - 1].CanConnect(Connections[i]))
                        return false;
                }
                return true;
            }
            public void Attach(Side side)
            {
                Assert(!this.IsAttached, "Attempting to connect a side that's already connected");
                Assert(CanAttach(side), "Attempting to connect incompatible/already connected sides");
                Assert(side.Owner != this.Owner, "Attempting to connect a tile to itself");
                this.Other = side.Owner;
                side.Other = this.Owner;
                for (uint i = 0; i < N_CONNECTORS; i++)
                {
                    side.Connections[N_CONNECTORS - i - 1].Connect(this.Connections[i]);
                }
            }
            public Side(Tile owner, Connection[] connections)
            {
                this.Owner = owner;
                this.Connections = connections;
            }
        }
        public InternalNode[] Nodes { get; protected set; }
        public Connection[] Connections { get; set; } = new Connection[N_SIDES * N_CONNECTORS];
        public Side[] Sides { get; set; } = new Side[N_SIDES];
        public Tile[] Neighbours { get; set; } = new Tile[N_SIDES];
        public Side Up { get => Sides[0]; }
        public Side Right { get => Sides[1]; }
        public Side Down { get => Sides[2]; }
        public Side Left { get => Sides[3]; }
        public Vector2I Position { get; set; } = new Vector2I(0, 0);
        public bool IsPlaced { get; protected set; } = false;
        public string DebugRepresentation
        {
            get
            {
                Assert(N_CONNECTORS == 3);
                Assert(N_SIDES == 4);
                string ret = "";
                ret += " ";
                for (int i = 0; i < N_CONNECTORS; i++)
                {
                    ret += GameEngine.GetTypeAbrv(Sides[0].Connections[i].INode.Type);
                }
                ret += "\n";
                for (int i = 0; i < N_CONNECTORS; i++)
                {
                    ret += GameEngine.GetTypeAbrv(Sides[3].Connections[i].INode.Type);
                    ret += "   ";
                    ret += GameEngine.GetTypeAbrv(Sides[1].Connections[i].INode.Type);
                    ret += "\n";
                }
                ret += " ";
                for (int i = 0; i < N_CONNECTORS; i++)
                {
                    ret += GameEngine.GetTypeAbrv(Sides[2].Connections[i].INode.Type);
                }
                ret += "\n";
                return ret;
            }
        }
        public void ReoderConnections()
        {
            for (int i = 0; i < N_SIDES; i++)
            {
                for (int ii = 0; ii < N_CONNECTORS; ii++)
                {
                    Connections[i * N_CONNECTORS + ii] = Sides[i].Connections[ii];
                }
            }
        }
        public void Rotate(int r)
        {
            r = AbsMod(r, (int)N_SIDES);
            if (r == 0)
                return;
            // GD.Print(r);
            // GD.Print(DebugRepresentation);
            Side[] nsides = new Side[N_SIDES];
            for (uint i = 0; i < N_SIDES; i++)
            {
                nsides[AbsMod((int)(i + r), N_SIDES)] = Sides[i];
            }
            Sides = nsides;
            ReoderConnections();
            // GD.Print(DebugRepresentation);
            // GD.Print("----");
        }
        public Tile(InternalNode[] nodes, Connection[] connections)
        {
            Assert(connections.Length == N_CONNECTORS * N_SIDES);
            this.Connections = connections;
            this.Nodes = nodes;
            for (int i = 0; i < N_SIDES; i++)
            {
                Sides[i] = new Side(this, connections.ToList().GetRange(i * N_CONNECTORS, N_CONNECTORS).ToArray()); // I know...
            }
        }
    }
}
