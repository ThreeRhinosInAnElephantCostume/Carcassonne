/*
    *** Player.cs ***

    The definition for the Player class. See Engine/Agent.cs for more.
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
using Carcassonne;
using ExtraMath;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

namespace Carcassonne
{
    public partial class GameEngine
    {
        public class Player : Agent
        {
            GameEngine eng { get; }

            public int Score { get; set; }
            public int PotentialScore { get; set; }
            public int EndScore => Score + PotentialScore;

            public List<Pawn> Pawns { get; set; } = new List<Pawn>();

            public Player(GameEngine eng, int ID) : base(ID)
            {
                this.eng = eng;
            }
        }
    }
}
