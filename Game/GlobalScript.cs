

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
    readonly System.Threading.Mutex _saveSettingsMX = new System.Threading.Mutex();
    int _saveIndex = 0;
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
        WriteFile(Constants.DataPaths.SETTINGS_PATH, data);
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
    public static void AddTheme(string name, PersonalTheme theme)
    {
        if (PersonalThemesList == null)
            PersonalThemesList = new List<PersonalTheme>();
        if (PersonalThemes == null)
            PersonalThemes = new Dictionary<string, PersonalTheme>();
        PersonalThemesList.Add(theme);
        PersonalThemes.Add(name, theme);
    }
    public override void _Ready()
    {
        List<string> requiredpaths = new List<string>()
        {
            Constants.DataPaths.TILE_DIRECTORY,
            Constants.DataPaths.TILESET_DIRECTORY,
            Constants.DataPaths.TILE_MODEL_DIRECTORY,
            Constants.DataPaths.PLAYER_THEMES_DIRECTORY,
            Constants.DataPaths.THEMEABLE_ICONS_DIRECTORY,
        };
        foreach (var it in requiredpaths)
        {
            EnsurePathExists(it);
        }
        if (!FileExists(Constants.DataPaths.SETTINGS_PATH))
        {
            SaveSettings(false);
        }
        var nsettings = DeserializeFromFile<SettingsSystem.MainSettings>(Constants.DataPaths.SETTINGS_PATH, true);
        if (nsettings == null)
        {
            Assert(FileExists(Constants.DataPaths.SETTINGS_PATH), "Error settings writing to " + Constants.DataPaths.SETTINGS_PATH);
            GD.PrintErr("An error occurend while loading settings from " + Constants.DataPaths.SETTINGS_PATH);
            GD.PrintErr("Falling back on default settings");
        }
        else
        {
            Settings = nsettings;
            Settings.CompleteLoad();
        }
        SaveSettings(true);

        if (!FileExists(Constants.DataPaths.DEFAULT_PLAYER_THEME_PATH))
        {
            GD.PrintErr(Constants.DataPaths.DEFAULT_PLAYER_THEME_PATH + " not found");
            var theme = new PersonalTheme();
            theme.PrimaryColor = new Color(1f, 0f, 0f);
            theme.SecondaryColor = new Color(0f, 1f, 0f);
            theme.TertiaryColor = new Color(0f, 0f, 1f);
            theme.IconPath = Constants.DataPaths.DEFAULT_PLAYER_ICON_PATH;
            theme.AvatarPath = Constants.DataPaths.DEFAULT_PLAYER_AVATAR_PATH;
            SerializeToFile(Constants.DataPaths.DEFAULT_PLAYER_THEME_PATH, theme);
        }
        DefaultTheme = DeserializeFromFile<PersonalTheme>(Constants.DataPaths.DEFAULT_PLAYER_THEME_PATH);

        // TODO: Load those from files.
        AddTheme("red", new PersonalTheme()
        {
            PrimaryColor = new Color(0.7f, 0f, 0f),
            SecondaryColor = new Color(0.9f, 0.1f, 0.1f),
            TertiaryColor = new Color(0.75f, 0.3f, 0.3f),
            IconPath = Constants.DataPaths.DEFAULT_PLAYER_ICON_PATH,
            AvatarPath = Constants.DataPaths.DEFAULT_PLAYER_AVATAR_PATH,
        });
        AddTheme("black", new PersonalTheme()
        {
            PrimaryColor = new Color(0.01f, 0.01f, 0.01f),
            SecondaryColor = new Color(0.33f, 0.23f, 0.35f),
            TertiaryColor = new Color(0.9f, 0.9f, 0.9f),
            IconPath = Constants.DataPaths.DEFAULT_PLAYER_ICON_PATH,
            AvatarPath = Constants.DataPaths.DEFAULT_PLAYER_AVATAR_PATH,
        });
        AddTheme("blue", new PersonalTheme()
        {
            PrimaryColor = new Color(0.0f, 0f, 0.7f),
            SecondaryColor = new Color(0.1f, 0.1f, 0.9f),
            TertiaryColor = new Color(0.3f, 0.3f, 0.75f),
            IconPath = Constants.DataPaths.DEFAULT_PLAYER_ICON_PATH,
            AvatarPath = Constants.DataPaths.DEFAULT_PLAYER_AVATAR_PATH,
        });
        AddTheme("yellow", new PersonalTheme()
        {
            PrimaryColor = new Color(0.8f, 0.8f, 0.0f),
            SecondaryColor = new Color(0.9f, 0.9f, 0.2f),
            TertiaryColor = new Color(1f, 1f, 0.3f),
            IconPath = Constants.DataPaths.DEFAULT_PLAYER_ICON_PATH,
            AvatarPath = Constants.DataPaths.DEFAULT_PLAYER_AVATAR_PATH,
        });
        AddTheme("green", new PersonalTheme()
        {
            PrimaryColor = new Color(0.0f, 0.7f, 0f),
            SecondaryColor = new Color(0.1f, 0.9f, 0.1f),
            TertiaryColor = new Color(0.3f, 0.75f, 0.3f),
            IconPath = Constants.DataPaths.DEFAULT_PLAYER_ICON_PATH,
            AvatarPath = Constants.DataPaths.DEFAULT_PLAYER_AVATAR_PATH,
        });
        AddTheme("white", new PersonalTheme()
        {
            PrimaryColor = new Color(0.9f, 0.9f, 0.9f),
            SecondaryColor = new Color(0.3f, 0.3f, 0.3f),
            TertiaryColor = new Color(0.1f, 0.1f, 0.1f),
            IconPath = Constants.DataPaths.DEFAULT_PLAYER_ICON_PATH,
            AvatarPath = Constants.DataPaths.DEFAULT_PLAYER_AVATAR_PATH,
        });
        AddTheme("gray", new PersonalTheme()
        {
            PrimaryColor = new Color(0.5f, 0.5f, 0.5f),
            SecondaryColor = new Color(0.6f, 0.6f, 0.6f),
            TertiaryColor = new Color(0.3f, 0.3f, 0.3f),
            IconPath = Constants.DataPaths.DEFAULT_PLAYER_ICON_PATH,
            AvatarPath = Constants.DataPaths.DEFAULT_PLAYER_AVATAR_PATH,
        });

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
