using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using Carcassonne;
using ExtraMath;
using Godot;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

namespace Carcassonne
{
    public partial class Map
    {
        public class Graph
        {
            public List<InternalNode> Nodes = new List<InternalNode>(64);
            public List<Tile> Tiles = new List<Tile>(64);
            public List<Tile.Connection> Connections = new List<Tile.Connection>(128);
            List<Tile.Connection> _openconnections = new List<Tile.Connection>();
            public List<Tile.Connection> OpenConnections
            {
                get
                {
                    if (Dirty)
                        Check();
                    return _openconnections;
                }
            }
            public NodeType Type { get; protected set; }
            public uint ID { get; protected set; }
            public List<object> Owners = new List<object>();
            public bool Dirty { get; protected set; } = true;
            bool _isclosed = true;
            public bool IsClosed
            {
                get
                {
                    if (Dirty)
                        Check();
                    return _isclosed;
                }
            }

            public void Check()
            {
                Dirty = false;
                _openconnections = new List<Tile.Connection>(Connections.Count);
                foreach (var it in Connections)
                {
                    Assert(it.node.graph == this, "Attempted to utilize an invalid graph!");
                    Assert(it.node.type == Type, "Attempted to utilize an invalid graph!");
                    Assert(!it.IsConnected || (it.IsConnected && it.Other.node.graph == this),
                        "Attempted to utilize an invalid graph!");

                    if (!it.IsConnected)
                    {
                        _openconnections.Add(it);
                    }
                }

                _isclosed = (_openconnections.Count == 0);
            }
            void MakeDirty()
            {
                Dirty = true;
            }
            bool AddUniqueTile(Tile t)
            {
                Assert(t != null);

                MakeDirty();

                if (Tiles.Contains(t))
                    return false;
                Tiles.Add(t);
                return true;
            }
            void AddConnection(Tile.Connection c)
            {
                Assert(c != null && c.node != null);
                Assert(c.node.graph == this);
                Assert(c.node.type == Type);
                Assert(!Connections.Contains(c));

                MakeDirty();

                Connections.Add(c);
            }
            public void AddNode(InternalNode n, bool addconnections = true)
            {
                Assert(n != null);
                Assert(!Nodes.Contains(n));
                Assert(n.type == Type);

                MakeDirty();

                n.graph = this;
                Nodes.Add(n);
                if (addconnections)
                {
                    foreach (var it in n.connections)
                        AddConnection(it);
                }
                AddUniqueTile(n.tile);
            }
            public Graph(NodeType type)
            {
                ID = CreateID();
                this.Type = type;

                MakeDirty();
            }
        }
        bool AddUniqueGraph(Graph g)
        {
            Assert(g != null);

            if (Graphs.Contains(g))
                return false;
            Graphs.Add(g);
            return true;
        }
        bool RemoveGraph(Graph g)
        {
            Assert(g != null);

            if (!Graphs.Contains(g))
                return false;
            Graphs.Remove(g);
            return true;
        }
        Graph MergeGraphs(Graph g0, Graph g1)
        {
            Assert(g0 != g1);
            Assert(g0 != null && g1 != null);

            (Graph greater, Graph lesser) = (g0.Nodes.Count > g1.Nodes.Count) ? (g0, g1) : (g1, g0);

            if (lesser.Owners.Count > 0)
                greater.Owners.AddRange(lesser.Owners);

            foreach (var it in lesser.Nodes)
            {
                greater.AddNode(it);
            }
            RemoveGraph(lesser);
            greater.Check();
            return greater;
        }
        Graph ExtendGraph(Graph graph, InternalNode branch)
        {
            Assert(graph != null && branch != null);
            Assert(branch.graph == null);

            graph.AddNode(branch, true);

            foreach (var it in branch.connections)
            {
                if (!it.IsConnected)
                    continue;
                if (it.Other.node.graph == null)
                {
                    graph = ExtendGraph(graph, it.Other.node);
                }
                else if (it.Other.node.graph != graph)
                {
                    graph = MergeGraphs(graph, it.Other.node.graph);
                }
            }

            return graph;
        }
        Graph CreateGraph(InternalNode origin)
        {
            Assert(origin.type != null);
            Assert(origin.graph == null);

            var g = new Graph(origin.type);
            g = ExtendGraph(g, origin);
            AddUniqueGraph(g);

            return g;
        }
        public List<Graph> UpdateGraphs(Tile tile)
        {
            Assert(tile != null);

            List<Graph> ngraphs = new List<Graph>();
            foreach (var it in tile.nodes)
            {
                var cns = it.connections.Find((Tile.Connection c) => c.IsConnected && c.Other.node.graph != null);
                if (cns != null)
                {
                    ngraphs.Add(ExtendGraph(cns.Other.node.graph, it));
                }
                else
                {
                    ngraphs.Add(CreateGraph(it));
                }
            }
            ngraphs = ngraphs.FindAll((g) => Graphs.Contains(g)).Distinct().ToList();
            return ngraphs;
        }
        public List<Graph> UpdateGraphs()
        {
            List<Graph> ngraphs = new List<Graph>(Graphs.Count * 4);
            foreach (var it in tiles)
            {
                ngraphs.AddRange(UpdateGraphs(it));
            }
            ngraphs = ngraphs.FindAll((g) => Graphs.Contains(g)).Distinct().ToList();
            return ngraphs;
        }
        void AddOwner(Graph graph, object o)
        {
            Assert(graph != null);
            Assert(o != null);

            if (!graph.Owners.Contains(o))
                graph.Owners.Add(o);
        }
        void AddOwners(Graph graph, List<object> o)
        {
            Assert(graph != null);
            Assert(o != null);

            foreach (var it in o)
            {
                AddOwner(graph, it);
            }
        }
        void RemoveOwner(Graph graph, object o)
        {
            Assert(graph != null);
            Assert(graph.Owners.Contains(o));

            graph.Owners.Remove(o);
        }
    }
}
