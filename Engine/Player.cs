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
        public class Player
        {
            GameEngine eng{get;}

            public int Score { get => eng.GetPlayerScore(this); }
            public int EndScore { get => eng.GetPlayerEndScore(this); } 
            public int ProbableScore { get => eng.GetPlayerProbableScore(this);  }

            public Player(GameEngine eng)
            {
                this.eng = eng;
            }
        }   
    }
}