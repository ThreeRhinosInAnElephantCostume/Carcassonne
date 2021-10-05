using System.Reflection.Metadata;
using System.Net.NetworkInformation;
using System.Threading.Tasks.Dataflow;
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
    MethodInfo[] actionmethods = typeof(Engine).GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance )
        .Where(m => m.GetCustomAttributes(typeof(ActionExec), false).Length > 0)
        .ToArray();
    public abstract class Action
    {
        public bool IsFilled{get; protected set;} = false;
    }
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ActionExec : System.Attribute
    {
        public Type type{get; protected set;}
        public ActionExec(Type type)
        {
            this.type = type;
        }
    }
    protected delegate void ActionDelegate(Action act);
    public void ExecuteAction(Action action)
    {
        Assert(actionmethods.Length > 0);
        
        if(!action.IsFilled)
            throw new Exception("Attempting to execute an incomplete action!"); 

        bool found = false;
        foreach(var it in actionmethods)
        {
            if(((ActionExec)it.GetCustomAttribute(typeof(ActionExec))).type == action.GetType())
            { 
                found = true;

                ActionDelegate d = (ActionDelegate) Delegate.CreateDelegate(typeof(ActionDelegate), this, it, true);

                d(action);
            }
        }
        if(!found)
            throw new Exception("Unsupported action!");
        _history.Add(action);
    }
    public static Engine CreateFromAction(Action action)
    {
        Engine eng = new Engine();
        eng.ExecuteAction(action);
        return eng;
    }
    public static Engine CreateFromHistory(List<Action> history)
    {
        Engine eng = new Engine();
        foreach(var it in history)
        {
            eng.ExecuteAction(it);
        }
        return eng;
    }
}