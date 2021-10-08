using System.Reflection.PortableExecutable;
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
    public partial class GameEngine
    {
        public const int N_SIDES = 4;
        public const int N_CONNECTORS = 3;
        public const int MIN_PLAYERS = 2;
        public const int MAX_PLAYERS = 8;
    }

}