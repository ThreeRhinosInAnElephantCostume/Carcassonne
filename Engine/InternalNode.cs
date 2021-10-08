using System.Xml.Linq;
using System.Reflection.Metadata;
using System.Net.NetworkInformation;
using System.Threading.Tasks.Dataflow;
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

using ExtraMath;

using Carcassonne;
using static Carcassonne.GameEngine;

namespace Carcassonne
{
    public class InternalNode
    {
        public NodeType type{get;}
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