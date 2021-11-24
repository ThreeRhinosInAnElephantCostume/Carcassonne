/*
    *** Pawn.cs ***

    A pawn effectively an interface for Agents (See Engine/Agent.cs) to interact with the Map, and 
    its Tiles.

    See also: Engine/BaseGame/Player.cs
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
    public abstract class Pawn
    {
        public bool HasOwner { get => Owner != null; }
        public Agent Owner { get; protected set; }
        public Tile CurrentTile { get; set; }
        public abstract bool IsInPlay { get; }
    }
}
