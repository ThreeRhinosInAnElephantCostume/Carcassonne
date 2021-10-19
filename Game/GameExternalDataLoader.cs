using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using Carcassonne;
using ExtraMath;
using Godot;
using Newtonsoft.Json;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

public class GameExternalDataLoader : IExternalDataSource
{
    ITileset IExternalDataSource.GetTileset(string name)
    {
        return TileDataLoader.LoadTileset(name);
    }
}
