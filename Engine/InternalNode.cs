/*
    *** InternalNode.cs ***

    An internal node represents a number of associated connections inside a tile.
*/

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
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

namespace Carcassonne
{
    public class InternalNode
    {
        // Extensibility required for potential expansions
        public class InternalNodeAttribute
        {
            public NodeAttributeType Type { get; protected set; }

            public InternalNodeAttribute(NodeAttributeType type)
            {
                this.Type = type;
            }
        }
        public NodeType Type { get; }
        public Tile ParentTile { get; protected set; }
        public int Index { get; protected set; }
        public List<Tile.Connection> Connections { get; set; } = new List<Tile.Connection>();
        public Map.Graph Graph { get; set; }
        public object Mark { get; set; } // potentially useful for certain search algorithms 
        public List<InternalNodeAttribute> Attributes { get; protected set; } = new List<InternalNodeAttribute>();

        // This is here for the sake of potential expansions 
        InternalNodeAttribute GenerateAttribute(NodeAttributeType tp)
        {
            return new InternalNodeAttribute(tp);
        }
        public List<NodeAttributeType> AttributeTypes => Attributes.ConvertAll(it => it.Type);
        public void DebugValidate()
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
        public InternalNode(Tile tile, int indx, NodeType type, List<NodeAttributeType> attributetypes)
        {
            Assert(type != NodeType.ERR);
            this.ParentTile = tile;
            this.Index = indx;
            this.Type = type;
            attributetypes.ForEach(it => Attributes.Add(GenerateAttribute(it)));
        }
    }
}
