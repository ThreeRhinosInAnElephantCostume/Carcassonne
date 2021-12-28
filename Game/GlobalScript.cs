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
    public override void _Ready()
    {
        List<string> requiredpaths = new List<string>()
        {
            Constants.TILE_DIRECTORY,
            Constants.TILESET_DIRECTORY,
            Constants.TILE_MODEL_DIRECTORY,
            Constants.PLAYER_THEMES_DIRECTORY,
            Constants.THEMEABLE_ICONS_DIRECTORY,
        };
        foreach (var it in requiredpaths)
        {
            EnsurePathExists(it);
        }
        if (!FileExists(Constants.SETTINGS_PATH))
        {
            SerializeToFile(Constants.SETTINGS_PATH, Settings);
        }
        if (!FileExists(Constants.DEFAULT_PLAYER_THEME_PATH))
        {
            var theme = new PersonalTheme();
            theme.PrimaryColor = new Color(0.2f, 0.5f, 1f);
            theme.SecondaryColor = new Color(0.05f, 0.1f, 1f);
            theme.TertiaryColor = new Color(0f, 0f, 0.5f);
            SerializeToFile(Constants.DEFAULT_PLAYER_THEME_PATH, theme);
        }
        Settings = DeserializeFromFile<MainSettings>(Constants.SETTINGS_PATH);
        Settings.CompleteLoad();

        // Example use for OnChangeHandle:

        // Settings.OnChangeHandle += (s, o) => GD.Print(s, "=", o);

        // Settings.Audio.Volume.Value = 1;
        // Settings.FullScreen.Value = true;
        // Settings.Resolution.Value = new Vector2I(100, 100);

        Settings.FullScreen.OnModification += v => OS.WindowFullscreen = v;
        Settings.Resolution.OnModification += v => OS.WindowSize = (Vector2)v;
        Settings.NotifyChangeOnAll();
        Settings.ClearModified();
    }
    public GlobalScript()
    {
        Assert(GS == null, "Attempted to create multiple instances of GlobalScript!");
        GS = this;
    }
}
