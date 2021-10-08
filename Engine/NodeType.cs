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
    public class NodeType
    {
        public uint ID{get;}
        public string Name{get;}
        public string Abrv{get;}
        public static bool operator ==(NodeType n0, NodeType n1)
        {
            if (ReferenceEquals(null, n0) || ReferenceEquals(null, n1)) 
                return ReferenceEquals(n0, n1);
            return n0.ID == n1.ID;
        }
        public static bool operator !=(NodeType n0, NodeType n1)
        {
            if (ReferenceEquals(null, n0) || ReferenceEquals(null, n1)) 
                return !ReferenceEquals(n0, n1);
            return n0.ID != n1.ID;
        }
        public bool Equals(NodeType other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(other, this)) return true;
            return this.ID == other.ID;
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
            return ID.GetHashCode();
        }
        public NodeType(uint id, string name, char abrv)
        {
            Assert(abrv != '\0');
            Assert(name.Length > 0);

            this.ID = id;
            this.Name = name;
            this.Abrv = "" + abrv;
        }
        public NodeType(string name, char abrv) : this(CreateID(), name, abrv) {}
    }
}