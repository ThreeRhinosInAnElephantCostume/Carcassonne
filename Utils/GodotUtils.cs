using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using ExtraMath;
using Godot;
using static System.Math;
using Expression = System.Linq.Expressions.Expression;

public static partial class Utils
{
    public static T FindChild<T>(Node parent) 
    {
        foreach(var it in parent.GetChildren())
        {
            if(it.GetType() == typeof(T))
                return (T)it;
        }
        return default(T);
    }
}