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
        public NodeType type { get; }
        public List<Tile.Connection> connections = new List<Tile.Connection>();
        public Tile tile;
        public Map.Graph graph;
        public int mark;
        public InternalNode(NodeType type)
        {
            this.type = type;
        }
    }
}
