/*
    *** Agent.cs ***

    An agent is any entity that's not directly reperesented on the map. 
    Agents should be uniquely identifiable by their ID.
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
        public abstract class Agent
        {
            public long ID { get; protected set; }
            public Agent(long id)
            {
                this.ID = id;
            }
            public Agent(GameEngine eng) : this(eng.NextUniqueID())
            {

            }
        }
    }
}
