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
#if GODOT 
using Godot;
#endif
using Newtonsoft.Json;
using static System.Math;
using Expression = System.Linq.Expressions.Expression;

public static partial class Utils
{
#if GODOT
    public static void DestroyNode(Node node)
    {
        if (node.GetParent() is Node parent)
            parent.RemoveChild(node);
        node.QueueFree();
    }
    public static string EnsurePathExists(string path)
    {
        Assert(!FileExists(path), "This function is meant for directories, not files");
        if (DirectoryExists(path))
            return "";
        string dif = "";
        List<string> parts = new List<string>();
        while (!DirectoryExists(path))
        {
            string subpath = "";
            if (path.Last() == '/')
                path = path.Remove(path.Length - 1);
            while (path.Last() != '/')
            {
                subpath += path.Last();
                path = path.Remove(path.Length - 1);
            }
            parts.Add(new string(subpath.Reverse().ToArray()));
        }
        var dm = new Directory();
        parts.Reverse();
        foreach (var it in parts)
        {
            path = ConcatPaths(path, it);
            dm.MakeDir(path);
            dif = ConcatPaths(dif, it);
        }
        return dif;
    }
    public static T DeserializeFromFile<T>(string path, bool failsafe = false) where T : class
    {
        Assert(FileExists(path));
        T o = null;
        try
        {
            o = JsonConvert.DeserializeObject<T>(ReadFile(path));
        }
        catch (Exception)
        {
            if (!failsafe)
                throw;
            return null;
        }

        if (!failsafe)
            Assert(o != null);

        return o;
    }
    public static void SerializeToFile<T>(string path, T o) where T : class
    {
        Assert(o != null);

        var data = JsonConvert.SerializeObject(o);
        WriteFile(path, data);
    }
    public static bool FileExists(string path)
    {
        return new Directory().FileExists(path);
    }
    public static string ReadFile(string path)
    {
        Assert(FileExists(path));
        File fm = new File();
        Assert(fm.Open(path, File.ModeFlags.Read));
        string dt = fm.GetAsText();
        fm.Close();
        return dt;
    }
    public static void WriteFile(string path, string data)
    {
        Assert(data != null);

        File fm = new File();
        Assert(fm.Open(path, File.ModeFlags.Write));
        fm.StoreString(data);
        fm.Close();
    }
    public static bool DirectoryExists(string path)
    {
        return new Directory().DirExists(path);
    }
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
    public static List<string> ListDirectoryFilesRecursively(string path, Func<string, bool> filter)
    {
        List<string> ret = new List<string>();
        Directory dm = new Directory();
        void ListRecursively(string path)
        {
            foreach (var it in ListDirectoryContents(path))
            {
                if (dm.DirExists(it))
                {
                    ListRecursively(it);
                }
                else
                {
                    Assert(dm.FileExists(it));
                    if (filter(it))
                    {
                        ret.Add(it);
                    }
                }
            }
        }
        ListRecursively(path);
        return ret;
    }
    public static List<string> ListDirectoryFilesRecursively(string path)
    {
        return ListDirectoryFilesRecursively(path, s => true);
    }
    public static List<string> ListDirectoryContents(string path, Func<string, bool> filter, bool relativepaths = false, bool skiphidden = true)
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
                ret.Add((relativepaths) ? s : ConcatPaths(path, s));
        }
        dm.ListDirEnd();
        return ret;
    }
    public static List<string> ListDirectoryContents(string path, bool relativepaths = false, bool skiphidden = true)
    {
        return ListDirectoryContents(path, s => true, relativepaths, skiphidden);
    }
    public static List<string> ListDirectoryFiles(string path, bool relativepaths = false, bool skiphidden = true)
    {
        return ListDirectoryContents(path, s =>
        {
            return new Directory().FileExists(s);
        }, relativepaths, skiphidden);
    }
    public static List<string> ListDirectorySubDirs(string path, bool relativepaths = false, bool skiphidden = true)
    {
        return ListDirectoryContents(path, s =>
        {
            return new Directory().DirExists(s);
        }, relativepaths, skiphidden);
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
#endif
}
