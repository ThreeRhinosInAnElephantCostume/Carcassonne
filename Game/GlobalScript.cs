/*

    GlobalScripts.cs
    This script should be the first thing to be loaded in, and consequently the first _Ready() call. 
    Do sanity checks and setups (e.g ensuring that certain directories exist) here.

*/
using System;
using System.Collections.Concurrent;
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
using Newtonsoft.Json;
using static System.Math;
using static Globals;
using static Utils;
using Expression = System.Linq.Expressions.Expression;

public class GlobalScript : Node
{
    public static GlobalScript GS;
    ConcurrentQueue<Action> _toExec = new ConcurrentQueue<Action>();
    void DequeDeferred()
    {
        System.Action action;
        Assert(_toExec.TryDequeue(out action), "Error: queue desynchronization");
        action();
    }
    public void QueueDeferred(System.Action action)
    {
        Assert(action != null);
        _toExec.Enqueue(action);
        CallDeferred(nameof(DequeDeferred));
    }
    public override void _Ready()
    {
        List<string> requiredpaths = new List<string>()
        {
            Constants.TILE_DIRECTORY,
            Constants.TILESET_DIRECTORY,
            Constants.TILE_MODEL_DIRECTORY,
        };
        foreach (var it in requiredpaths)
        {
            EnsurePathExists(it);
        }
        if (!FileExists(Constants.SETTINGS_PATH))
        {
            SerializeToFile(Constants.SETTINGS_PATH, Settings);
        }
        Settings = DeserializeFromFile<MainSettings>(Constants.SETTINGS_PATH);
        Settings.CompleteLoad();

        // Example use for OnChangeHandle:

        // Settings.OnChangeHandle += (s, o) => GD.Print(s, "=", o);

        // Settings.Audio.Volume.Value = 1;
        // Settings.FullScreen.Value = true;
        // Settings.Resolution.Value = new Vector2I(100, 100);

        OS.WindowFullscreen = Settings.FullScreen;
        OS.WindowSize = (Vector2)Settings.Resolution.Value;
    }
    public GlobalScript()
    {
        Assert(GS == null, "Attempted to create multiple instances of GlobalScript!");
        GS = this;
    }
}
