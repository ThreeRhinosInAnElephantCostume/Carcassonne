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
    public class Tile
    {
        public class Connection 
        { 
            public InternalNode node{get;}
            public Connection Other {get; protected set;} = null;
            public bool IsConnected{get => Other != null;}
            public NodeType Type{get => node.type;}
            public virtual bool CanConnect(Connection other)
            {
                return this.Type == other.Type;
            }
            public virtual void Connect(Connection other)
            {
                Assert(CanConnect(other), "Attempting to connect incompatible connectors");

                this.Other = other;
                if(!other.IsConnected)
                    other.Connect(this);
            }
            public Connection(InternalNode node)
            {
                this.node = node;
                if(!node.connections.Contains(this))
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
                Assert(!this.IsAttached, "Attempting to connect a side that's already connected");
                Assert(CanAttach(side), "Attempting to connect incompatible/already connected sides");
                Assert(side.owner != this.owner, "Attempting to connect a tile to itself");
                this.attached = side.owner;
                side.attached = this.owner;
                for(uint i = 0; i < N_CONNECTORS; i++)
                {
                    side.connectors[N_CONNECTORS-i-1].Connect(this.connectors[i]);
                }
            }
            public Side(Tile owner, Connection[] connections)
            {
                this.owner =owner;
                this.connectors = connections;
            }
        }
        public InternalNode[] nodes{get; protected set;}
        public Connection[] connections {get; set;} = new Connection[N_SIDES*N_CONNECTORS];
        public Side[] sides {get; set;} = new Side[N_SIDES];
        public Tile[] neighbours {get; set;} = new Tile[N_SIDES];
        public Side Up {get => sides[0];}
        public Side Right {get => sides[1];}
        public Side Down {get => sides[2];}
        public Side Left {get => sides[3];}
        public Vector2I position {get; set;} = new Vector2I(0,0);
        public bool IsPlaced{get; protected set;} = false;
        public string DebugRepresentation
        {
            get
            {
                Assert(N_CONNECTORS == 3);
                Assert(N_SIDES == 4);
                string ret = "";
                ret += " ";
                for(int i = 0; i < N_CONNECTORS; i++ )
                {
                    ret += sides[0].connectors[i].node.type.Abrv;
                }
                ret += "\n";
                for(int i = 0; i < N_CONNECTORS; i++)
                {
                    ret += sides[3].connectors[i].node.type.Abrv;
                    ret += "   ";
                    ret += sides[1].connectors[i].node.type.Abrv;
                    ret += "\n";
                }
                ret += " ";
                for(int i = 0; i < N_CONNECTORS; i++ )
                {
                    ret += sides[2].connectors[i].node.type.Abrv;
                }
                ret += "\n";
                return ret;
            }
        }
        public void ReoderConnections()
        {
            for(int i = 0; i < N_SIDES; i++)
            {
                for(int ii = 0; ii < N_CONNECTORS; ii++)
                {
                    connections[i * N_CONNECTORS + ii] = sides[i].connectors[ii];
                }
            }
        }
        public void Rotate(int r)
        {
            r =  AbsMod(r, (int)N_SIDES);
            if(r == 0)
                return;
            // GD.Print(r);
            // GD.Print(DebugRepresentation);
            Side[] nsides = new Side[N_SIDES];
            for(uint i = 0; i < N_SIDES; i++)
            {
                nsides[AbsMod((int)(i + r), N_SIDES)] = sides[i];
            }
            sides = nsides;
            ReoderConnections();
            // GD.Print(DebugRepresentation);
            // GD.Print("----");
        }
        public Tile( InternalNode[] nodes, Connection[] connections)
        {
            Assert(connections.Length == N_CONNECTORS * N_SIDES);
            this.connections = connections;
            this.nodes = nodes;
            for(int i = 0; i < N_SIDES; i++)
            {
                sides[i] = new Side(this, connections.ToList().GetRange(i*N_CONNECTORS, N_CONNECTORS).ToArray()); // I know...
            }
        }
    }
}