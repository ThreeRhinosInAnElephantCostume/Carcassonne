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


public partial class Engine
{
    public const uint N_SIDES = 4;
    public const uint N_CONNECTORS = 3;
    public class Tile
    {
        public class NodeType
        {
            uint id;
            string Name{get;}
            public static bool operator ==(NodeType n0, NodeType n1)
            {
                return n0.id == n1.id;
            }
            public static bool operator !=(NodeType n0, NodeType n1)
            {
                return n0.id != n1.id;
            }
            public bool Equals(NodeType other)
            {
                if (ReferenceEquals(other, null)) return false;
                if (ReferenceEquals(other, this)) return true;
                return this.id == other.id;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if(!(obj is NodeType))
                    return false;
                return Equals(obj as NodeType);
            }

            public override int GetHashCode()
            {
                return id.GetHashCode();
            }
            NodeType(uint id, string name)
            {
                this.id = id;
                this.Name = name;
            }
            NodeType(string name) : this(CreateID(), name) {}
        }
        public abstract class InternalNode
        {
            public abstract NodeType type{get;}
            public List<Connection> connections = new List<Connection>();
            public Tile tile;
        }
        public class Connection 
        { 
            public InternalNode node{get;}
            Connection other = null;
            public bool IsConnected{get => other != null;}
            public NodeType Type{get => node.type;}
            public virtual bool CanConnect(Connection other)
            {
                return this.Type.GetType() == other.Type.GetType();
            }
            public virtual void Connect(Connection other)
            {
                if(!CanConnect(other))
                    throw new Exception("Attempting to connect incompatible connectors");
                this.other = other;
                if(!other.IsConnected)
                    other.Connect(this);
            }
            Connection(InternalNode node)
            {
                this.node = node;
                node.connections.Add(this);
            }
        }
        public class Side
        {
            public Connection[] connectors {get; protected set;} = new Connection[N_CONNECTORS];
            public Tile owner {get; protected set;}
            public Tile attached {get; protected set;} = null;
            public bool IsAttached{get => attached != null;}
            public bool CanAttachVerbose(Side side, out List<int> invalidconnections, ref int conindex)
            {
                invalidconnections = new List<int>((int)N_SIDES);
                bool can = true;
                if(side.IsAttached)
                    return false;
                for(uint i = 0; i < N_CONNECTORS; i++)
                {
                    if(!side.connectors[N_CONNECTORS-i-1].CanConnect(connectors[i]))
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
                if(side.IsAttached)
                    return false;
                for(uint i = 0; i < N_CONNECTORS; i++)
                {
                    if(!side.connectors[N_CONNECTORS-i-1].CanConnect(connectors[i]))
                        return false;
                }
                return true;
            }
            public void Attach(Side side)
            {
                if(this.IsAttached)
                    throw new Exception("Attempting to connect a side that's already connected");
                if(!CanAttach(side))
                    throw new Exception("Attempting to connect incompatible/already connected sides");
                if(side.owner == this.owner)
                    throw new Exception("Attempting to connect a tile to itself");
                this.attached = side.owner;
                side.attached = this.owner;
                for(uint i = 0; i < N_CONNECTORS; i++)
                {
                    side.connectors[N_CONNECTORS-i-1].Connect(this.connectors[i]);
                }
            }
        }
        public Connection[] connections {get; set;} = new Connection[N_SIDES*N_CONNECTORS];
        public Side[] sides {get; set;} = new Side[N_SIDES];
        public Tile[] neighbours {get; set;} = new Tile[N_SIDES];
        public Side Up {get => sides[0];}
        public Side Right {get => sides[1];}
        public Side Down {get => sides[2];}
        public Side Left {get => sides[3];}
        public Vector2I position {get; set;} = new Vector2I(0,0);
        public bool IsPlaced{get; protected set;} = false;
        public void Rotate(int r)
        {
            r %= (int)N_SIDES;
            Side[] nsides = new Side[N_SIDES];
            for(uint i = 0; i < N_SIDES; i++)
            {
                nsides[i] = sides[i + r];
                for(uint ii = 0; ii < N_CONNECTORS; ii++)
                {
                    connections[i*3 + ii] = nsides[i].connectors[ii];
                }
            }
            sides = nsides;
        }
    }
}