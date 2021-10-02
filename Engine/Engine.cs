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

public partial class Engine
{
    public struct Action
    {
        uint libraryid;
        uint actionid;
        byte[] actiondata;
    }
    public abstract class Governor
    {
        bool CanExecute(State state, Action move);
        void Execute(State state, Action move);
        List<Action> BasicActions();
        List<Action> PossibleActions();
    }
    public abstract class PawnModule
    {
        
    }
    public class Meeple : PawnModule
    {

    }
    static List<Governor> possiblegovernors = new List<Governor>();
    static List<Governor> possiblepawns = new List<PawnModule>();

    State state = new State();
}