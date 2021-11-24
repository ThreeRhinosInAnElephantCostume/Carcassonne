/*
    *** Tile.cs ***

    A collection of 12 Connections, 4 Sides, and n >= 1 InternalNodes. 
    The Connection list should be thought as starting in the upper left corner of the tile, 
    then moving to the right, then down, then left, and then up, with 3 Connections on each side.
    Do note that rotation is implemented by physically rearraning Sides and Connections.
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
    /// <summary>
    /// A collection of 12 Connections, 4 Sides, and n >= 1 InternalNodes. 
    ///  The Connection list should be thought as starting in the upper left corner of the tile, 
    /// then moving to the right, then down, then left, and then up, with 3 Connections on each side.
    /// Do note that rotation is implemented by physically rearraning Sides and Connections.
    /// </summary>
    public class Tile
    {
        ///<summary>A representation of a tile-wide attribute, like a monastery</summary>
        public class TileAttribute
        {
            public Tile tile { get; protected set; }
            public TileAttributeType Type { get; protected set; }

            public TileAttribute(Tile tile, TileAttributeType tp)
            {
                this.tile = tile;
                this.Type = tp;
            }
        }
        ///<summary>Represents a single connection, associated with an InternalNode</summary>
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
                Assert(!node.Connections.Contains(this));
                node.Connections.Add(this);
            }
        }
        ///<summary>Represents a Tile's side and 3 of its connections, useful when attaching tiles</summary>
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
        public List<InternalNode> Nodes { get; protected set; }
        public List<Connection> Connections { get; protected set; }
        public Side[] Sides { get; protected set; } = new Side[N_SIDES];
        public Tile[] Neighbours { get; set; } = new Tile[N_SIDES];
        public Side Up { get => Sides[0]; }
        public Side Right { get => Sides[1]; }
        public Side Down { get => Sides[2]; }
        public Side Left { get => Sides[3]; }
        public Vector2I Position { get; set; } = new Vector2I(0, 0);
        public object MetaData { get; set; }
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

        public List<TileAttribute> Attributes { get; protected set; } = new List<TileAttribute>();
        public TileAttribute GenerateAttribute(TileAttributeType tp)
        {
            if (tp == TileAttributeType.MONASTERY)
                return new TileMonasteryAttribute(this);
            return new TileAttribute(this, tp);
        }
        public List<TileAttributeType> AttributeTypes => Attributes.ConvertAll(it => it.Type);
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
        ///<summary>
        /// Rotates the Tile r times. 
        /// Rotation physically rearanges the Tile's nodes.
        /// You can reverse any given rotation by applying an opposite one, 
        /// but note that Tile itself has no notion of its own rotation.
        ///</summary>
        public void Rotate(int r)
        {
            r = AbsMod(r, (int)N_SIDES);
            if (r == 0)
                return;
            Side[] nsides = new Side[N_SIDES];
            for (uint i = 0; i < N_SIDES; i++)
            {
                nsides[AbsMod((int)(i + r), N_SIDES)] = Sides[i];
            }
            Sides = nsides;
            ReoderConnections();
        }
        public Tile(List<(NodeType tp, List<NodeAttributeType> attrs)> nodetypes, List<int> assignments, List<TileAttributeType> attrs)
        {
            Assert(assignments.Count == N_CONNECTORS * N_SIDES);
            Assert(nodetypes.Count > 0);
            Assert(nodetypes.TrueForAll(nt => nt.tp != NodeType.ERR));

            int i = 0;

            Nodes = nodetypes.ConvertAll<InternalNode>(nt => new InternalNode(this, i++, nt.tp, nt.attrs));

            Connections = assignments.ConvertAll<Connection>(it => new Connection(Nodes[it]));

            for (i = 0; i < N_SIDES; i++)
            {
                Sides[i] = new Side(this, Connections.ToList().GetRange(i * N_CONNECTORS, N_CONNECTORS).ToArray()); // I know...
            }

            Attributes = attrs.ConvertAll<TileAttribute>(it => GenerateAttribute(it));

            Assert(Nodes.ToList().TrueForAll(n => n.Connections.Count > 0));
        }
    }
}
