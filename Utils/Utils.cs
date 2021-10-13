
/* Utils.cs

General utilities.

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
using ExtraMath;
using Godot;
using static System.Math;
using Expression = System.Linq.Expressions.Expression;

public static partial class Utils
{
    public static T EnumNext<T>(T e) where T : Enum
    {
        var l = Enum.GetValues(typeof(T));
        bool f = false;
        foreach (var it in l)
        {
            if (f)
                return (T)it;
            if (it.Equals(e))
            {
                f = true;
            }
        }
        foreach (var it in l)
        {
            return (T)it;
        }
        throw new Exception();
    }
    public static string ConcatPaths(string p0, string p1)
    {
        Assert(p0 != null && p1 != null);
        if (p0.Length == 0 || p0 == "/")
            return p1;
        if (p1.Length == 0 || p1 == "/")
            return p0;
        bool p0is = p0.Last() == '/';
        bool p1is = p1.First() == '/';
        if (p0is ^ p1is)
            return p0 + p1;
        if (p0is)
            return p0.Remove(p0.Length-1) + p1;
        return p0 + "/" + p1;
    }
    public static string ConcatPaths(string p0, string p1, string p2)
    {
        return ConcatPaths(ConcatPaths(p0, p1), p2);
    }
    public static string ConcatPaths(string p0, string p1, string p2, string p3)
    {
        return ConcatPaths(ConcatPaths(p0, p1, p2), p3);
    }

    public static string ConcatPaths(List<string> pl)
    {
        Assert(pl != null && pl.Count > 0);

        if (pl.Count == 1)
            return pl[0];
        int i = 2;
        string s = ConcatPaths(pl[0], pl[1]);
        while (i < pl.Count)
        {
            s = ConcatPaths(s, pl[i]);
            i++;
        }
        return s;
    }

    // Python-style division remainder, for instance: AbsMod(-1, 4) == 3
    public static int AbsMod(int v, int d)
    {
        return (v % d + d) % d;
    }
    public static long AbsMod(long v, long d)
    {
        return (v % d + d) % d;
    }
    [System.Serializable]
    public class AssertionFailureException : System.Exception
    {
        public AssertionFailureException() { }
        public AssertionFailureException(string message) : base(message) { }
        public AssertionFailureException(string message, System.Exception inner) : base(message, inner) { }
        protected AssertionFailureException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    [Conditional("DEBUG")]
    public static void Assert(bool b, string msg)
    {
        if (b)
            return;
        GD.PrintErr("ASSERTION FAILURE:");
        GD.PrintErr(msg);
        //Debugger.Break();
        throw new AssertionFailureException(msg);
    }
    [Conditional("DEBUG")]
    public static void Assert(bool b)
    {
        Assert(b, "An unspecified assertion failure has been triggered!");
    }
    [Conditional("DEBUG")]
    public static void AssertNot(bool b, string msg)
    {
        Assert(!b, msg);
    }
    [Conditional("DEBUG")]
    public static void AssertNot(bool b)
    {
        Assert(!b);
    }
    static uint ID = 0;
    public static uint CreateID()
    {
        return ++ID;
    }
    public static uint LastID()
    {
        return ID - 1;
    }
    public delegate object ObjectActivator(params object[] args);
    public static ObjectActivator GetActivator<T>(ConstructorInfo ctor)
    {
        Type type = ctor.DeclaringType;
        ParameterInfo[] paramsInfo = ctor.GetParameters();

        //create a single param of type object[]
        ParameterExpression param =
            Expression.Parameter(typeof(object[]), "args");

        Expression[] argsExp =
            new Expression[paramsInfo.Length];

        //pick each arg from the params array 
        //and create a typed expression of them
        for (int i = 0; i < paramsInfo.Length; i++)
        {
            Expression index = Expression.Constant(i);
            Type paramType = paramsInfo[i].ParameterType;

            Expression paramAccessorExp =
                Expression.ArrayIndex(param, index);

            Expression paramCastExp =
                Expression.Convert(paramAccessorExp, paramType);

            argsExp[i] = paramCastExp;
        }

        //make a NewExpression that calls the
        //ctor with the args we just created
        NewExpression newExp = Expression.New(ctor, argsExp);

        //create a lambda with the New
        //Expression as body and our param object[] as arg
        LambdaExpression lambda =
            Expression.Lambda(typeof(ObjectActivator), newExp, param);

        //compile it
        ObjectActivator compiled = (ObjectActivator)lambda.Compile();
        return compiled;
    }
    public static ObjectActivator GetActivator<T>()
    {
        return GetActivator<T>(typeof(T).GetConstructors().First());
    }
}
