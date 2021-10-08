﻿using System;
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
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

namespace Carcassonne
{
    public interface ITileset
    {
        bool HasStarter { get; }
        int NPossibleStarters { get; }
        int NPossibleTiles { get; }
        int NTiles { get; }
        Tile GenerateStarter(RNG rng);
        List<Tile> GenerateTiles(RNG rng);
    }
}
