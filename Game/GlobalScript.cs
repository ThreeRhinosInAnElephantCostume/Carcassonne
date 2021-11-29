/*

    GlobalScripts.cs
    This script should be the first thing to be loaded in, and consequently the first _Ready() call. 
    Do sanity checks and setups (e.g ensuring that certain directories exist) here.

*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using ExtraMath;
using Godot;
using Newtonsoft.Json;
using static System.Math;
using static Utils;
using Expression = System.Linq.Expressions.Expression;

public class GlobalScript : Node
{
    public override void _Ready()
    {
        List<string> requiredpaths = new List<string>()
        {
            Constants.TILE_DIRECTORY,
            Constants.TILESET_DIRECTORY,
            Constants.TILE_MODEL_DIRECTORY,
        };
        foreach (var it in requiredpaths)
        {
            EnsurePathExists(it);
        }
    }
}
