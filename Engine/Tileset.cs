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
    public interface ITileset
    {
        bool HasStarter{get;}
        int NPossibleStarters{get;}
        int NPossibleTiles{get;}
        int NTiles{get;}
        Tile GenerateStarter(RNG rng);
        List<Tile> GenerateTiles(RNG rng);
    }
}
