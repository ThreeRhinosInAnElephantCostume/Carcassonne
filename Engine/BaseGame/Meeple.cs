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
using Godot;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

namespace Carcassonne
{
    public class Meeple : Pawn
    {
        public enum Role
        {
            NONE,
            FARMER,
            KNIGHT,
            HIGHWAYMAN,
            MONK
        }
        public Role CurrentRole { get; set; } = Role.NONE;
        public Meeple(Player player)
        {
            this.Owner = player;
        }
    }
}
