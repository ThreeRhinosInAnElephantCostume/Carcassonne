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
    public State state{get; protected set;}

    public static Engine CreateBaseGame()
    {
        Engine eng = new Engine();
        State state = eng.state = new State(0);
        Module bgm = new BaseGameModule(state, new List<Type>());
        state.modules.Add(bgm);
        state.Initialize();
        return eng;
    }
    protected Engine()
    {

    }
}