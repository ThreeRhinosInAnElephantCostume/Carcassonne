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

namespace Carcassonne
{
    public partial class GameEngine
    {
        public class Player
        {
            GameEngine eng { get; }

            public int Score { get => eng.GetPlayerScore(this); }
            public int EndScore { get => eng.GetPlayerEndScore(this); }
            public int ProbableScore { get => eng.GetPlayerProbableScore(this); }

            public Player(GameEngine eng)
            {
                this.eng = eng;
            }
        }
    }
}
