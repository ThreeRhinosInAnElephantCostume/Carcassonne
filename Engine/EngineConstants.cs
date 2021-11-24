/*
    *** EngineConstants.cs ***

    The GameEngine's many constants. 
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.PortableExecutable;
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
        public const int N_SIDES = 4;
        public const int N_CONNECTORS = 3;
        public const int MIN_PLAYERS = 2;
        public const int MAX_PLAYERS = int.MaxValue;

        // Base game
        public const int STARTER_MEEPLES = 7;
        public const int ROAD_COMPLETE_POINTS = 1; // Points per a complete road's tile
        public const int ROAD_INCOMPLETE_POINTS = 1; // Points per an incomplete roads' tile
        public const int CITY_COMPLETE_POINTS = 2; // Points per a complete city's tile
        public const int CITY_INCOMPLETE_POINTS = 1; // Points per an incomplete city's tile
        public const int CITY_COMPLETE_BONUS_POINTS = 2; // Bonus points from CITY_BONUS
        public const int CITY_INCOMPLETE_BONUS_POINTS = 1; // Bonus points from CITY_BONUS
        public const int CITY_SMALL_THRESHOLD = 3; // Cities with less tiles will be considered SMALL
        public const int CITY_SMALL_POINTS = 1; // Points per tile for small cities
        public const int FARM_PER_CITY_POINTS = 3; // Points per neighbouring enclosed city
        public const int MONASTERY_INCOMPLETE_POINTS = 1;
        public const int MONASTERY_COMPLETE_POINTS = 1;

        public readonly static List<Vector2I> MONASTERY_NEIGHBOURS = new List<Vector2I>()
        {
            new Vector2I(0, 0),
            Vector2I.Up,
            Vector2I.Right,
            Vector2I.Down,
            Vector2I.Left,
            Vector2I.Up + Vector2I.Right,
            Vector2I.Up + Vector2I.Left,
            Vector2I.Down + Vector2I.Right,
            Vector2I.Down + Vector2I.Left,
        };
    }

}
