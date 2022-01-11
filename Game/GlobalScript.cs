﻿/*

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
    System.Threading.Mutex _saveSettingsMX = new System.Threading.Mutex();
    int _saveIndex = 0;
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
    public void SaveSettingsAsync(bool clearmodified)
    {
        _saveSettingsMX.WaitOne();
        ThreadPool.QueueUserWorkItem(o =>
        {
            _saveSettingsMX.WaitOne();
            SaveSettings(clearmodified, (int)o);
            _saveSettingsMX.ReleaseMutex();
        }, _saveIndex + 1);
        _saveSettingsMX.ReleaseMutex();
    }
    void SaveSettings(bool clearmodified, int indx)
    {
        if (_saveIndex >= indx)
            GD.PrintErr($"Out-of-order save. _saveIndex=${_saveIndex}, indx=${indx}");
        var settings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All,
            PreserveReferencesHandling = PreserveReferencesHandling.All,
            ReferenceLoopHandling = ReferenceLoopHandling.Error,
            Formatting = Formatting.Indented,
        };
        var data = JsonConvert.SerializeObject(Settings, settings);
        WriteFile(Constants.SETTINGS_PATH, data);
        if (clearmodified)
            Settings.ClearModified();
        _saveIndex = indx;
    }
    public void SaveSettings(bool clearmodified)
    {
        _saveSettingsMX.WaitOne();
        SaveSettings(clearmodified, _saveIndex + 1);
        _saveSettingsMX.ReleaseMutex();
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
            SaveSettings(false);
        }
        var nsettings = DeserializeFromFile<SettingsSystem.MainSettings>(Constants.SETTINGS_PATH, true);
        if (nsettings == null)
        {
            Assert(FileExists(Constants.SETTINGS_PATH), "Error settings writing to " + Constants.SETTINGS_PATH);
            GD.PrintErr("An error occurend while loading settings from " + Constants.SETTINGS_PATH);
            GD.PrintErr("Falling back on default settings");
        }
        else
        {
            Settings = nsettings;
            Settings.CompleteLoad();
        }
        SaveSettings(true);

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
