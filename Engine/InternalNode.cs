using System;
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
    public class InternalNode
    {
        public NodeType Type { get; }
        public List<Tile.Connection> Connections = new List<Tile.Connection>();
        public Tile ParentTile { get; set; }
        public Map.Graph Graph { get; set; }
        public object Mark { get; set; } // potentially useful for certain search algorithms 
        void DebugValidate()
        {
            Assert(Type != NodeType.ERR);
            Assert(ParentTile != null);
            Assert(Connections != null);
            Assert(Graph == null || Graph.Nodes.Contains(this));
            Assert(ParentTile.Nodes.Contains(this));
            Assert(Connections.TrueForAll(c => ParentTile.Connections.Contains(c)));
            Assert(Connections.TrueForAll(c => !c.IsConnected || c.Other.INode.Graph == this.Graph));
            Connections.ForEach(c => c.DebugValidate());
        }
        public InternalNode(NodeType type)
        {
            Assert(type != NodeType.ERR);
            this.Type = type;
        }
    }
}
