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
    public partial class GameEngine
    {
        public abstract class Agent
        {
            public long ID { get; protected set; }
            public Agent(long id)
            {
                this.ID = id;
            }
            public Agent(RNG rng) : this(rng.NextLong())
            {

            }
        }
    }
}