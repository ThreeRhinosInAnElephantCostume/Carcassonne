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
    public class Player
    {
        State state{get;}

        public int Score { get => state.GetPlayerScore(this); }
        public int EndScore { get => state.GetPlayerEndScore(this); } 
        public int ProbableScore 
        { 
            get => state.GetPlayerProbableScore(this); 
            set => state.SetPlayerScore(this, value);
        }

        public Player(State state)
        {
            this.state = state;
        }
    }   
}