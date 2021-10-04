
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
    public abstract class Action
    {
        public bool IsFilled{get; protected set;} = false;
    }
    void PlaceCurrentTile(Vector2I pos, int rot)
    {
        
    }
    public void ExecuteAction(Action action)
    {
        if(!action.IsFilled)
            throw new Exception("Attempting to execute an empty action!");
        History.Add(action);
        switch(action)
        {
            case PlaceTileAction act:
                 PlaceCurrentTile(act.pos, act.rot);
                 break;
        };
    }
}