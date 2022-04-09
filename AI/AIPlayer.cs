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
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

namespace AI
{
    public abstract class AIPlayer
    {
        public RNG rng{get; protected set;}
        public abstract void MakeMove(GameEngine engine);
        public AIPlayer(RNG rng)
        {
            this.rng = rng;
        }
    }
}