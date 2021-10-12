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
    public static List<T> GetChildrenRecrusively<T>(Node root, bool restrichtomatchingparents = false) where T : Node
    {
        List<T> ret = new List<T>(8);

        void SearchRecursively(Node n)
        {
            foreach (var it in n.GetChildren())
            {
                Assert(it is Node);
                if (it is T)
                {
                    ret.Add((T)it);
                }
                if (!restrichtomatchingparents || it is T)
                {
                    SearchRecursively((Node)it);
                }
            }
        }
        SearchRecursively(root);

        return ret;
    }
    [Conditional("DEBUG")]
    public static void Assert(Error b, string msg)
    {
        if (b == Error.Ok)
            return;
        GD.PrintErr($"ASSERTION FAILURE (Error.{b.ToString()}):");
        GD.PrintErr(msg);
        //Debugger.Break();
        throw new AssertionFailureException(msg);
    }
    [Conditional("DEBUG")]
    public static void Assert(Error b)
    {
        Assert(b, $"An unspecified assertion failure, error code {b.ToString()}, has been triggered!");
    }
    public static List<string> ListDirectoryContents(string path, Func<string, bool> filter, bool skiphidden = true)
    {
        List<string> ret = new List<string>(8);
        Directory dm = new Directory();
        Assert(dm.Open(path) == Error.Ok);
        dm.ListDirBegin(true, skiphidden);
        while (true)
        {
            string s = dm.GetNext();
            if (s == "")
                break;
            if (filter(ConcatPaths(path, s)))
                ret.Add(s);
        }
        dm.ListDirEnd();
        return ret;
    }
    public static List<string> ListDirectoryContents(string path, bool skiphidden = true)
    {
        return ListDirectoryContents(path, s => true, skiphidden);
    }
    public static List<string> ListDirectoryFiles(string path, bool skiphidden = true)
    {
        return ListDirectoryContents(path, s =>
        {
            return new Directory().FileExists(s);
        }, skiphidden);
    }
    public static List<string> ListDirectorySubDirs(string path, bool skiphidden = true)
    {
        return ListDirectoryContents(path, s =>
        {
            return new Directory().DirExists(s);
        }, skiphidden);
    }

    public static T FindChild<T>(Node parent)
    {
        foreach (var it in parent.GetChildren())
        {
            if (it.GetType() == typeof(T))
                return (T)it;
        }
        return default(T);
    }
}
