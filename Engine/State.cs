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

using static ExtraMath;


public partial class Engine
{
    
    [Serializable]
    public class Pawn
    {
        public uint _moduleid;
        public uint _tileid;

        public bool IsAlive{get; set;}

        public bool IsInPlay{get => _tileid == 0;}
        
        public PawnModule module {get => Engine.possiblepawns[_moduleid];}
    }
    public class Tile
    {
        public uint _tileid;
        public uint 
    }

    [Serializable]
    public class State
    {   
        public uint _governorid;
        public Pawn[] pawns;

        public Governor Governor { get => Engine.possiblegovernors[_governorid]; }
        public Dictionary<string, object> Settings = new Dictionary<string, object>();
    }
}