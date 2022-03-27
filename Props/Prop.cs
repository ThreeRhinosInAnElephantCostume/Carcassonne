using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using static Utils;
using Expression = System.Linq.Expressions.Expression;

[Tool]
public class Prop : Spatial
{
    string _examplePlayerTheme = "";
    [Export(PropertyHint.File, "*.json,")]
    public string ExamplePlayerTheme
    {
        get => _examplePlayerTheme; set
        {
            _examplePlayerTheme = value;
            if (!FileExists(ExamplePlayerTheme))
            {
                GD.Print("File not found: " + ExamplePlayerTheme);
                return;
            }
            PersonalTheme ntheme = null;
            try
            {
                ntheme = DeserializeFromFile<PersonalTheme>(ExamplePlayerTheme);
            }
            catch (Exception)
            {
                GD.PrintErr("Failed to deserialize theme: " + ExamplePlayerTheme);
                return;
            }
            this.CurrentTheme = ntheme;
        }
    }
    List<Action<PersonalTheme>> _themeSetters = new List<Action<PersonalTheme>>();
    PersonalTheme _theme = null;
    PersonalTheme CurrentTheme { get => _theme; set { _theme = value; UpdateTheme(); } }
    void UpdateTheme()
    {
        if (CurrentTheme == null)
            return;
        _themeSetters.ForEach(it => it(CurrentTheme));
    }
    void LoadThemeSetters()
    {
        _themeSetters = new List<Action<PersonalTheme>>();
        Utils.GetChildrenRecrusively<Spatial>(this).FindAll(it => it is IPropElement)
            .ForEach(it =>
            {
                var prop = (IPropElement)it;
                _themeSetters.AddRange(prop.GetThemeSetters());
                prop.OnChangeHandle = () => this.LoadThemeSetters();
            });
        UpdateTheme();
    }
    bool _dirty = true;
    public void MarkForLoad()
    {
        _dirty = true;
    }
    public override void _Ready()
    {
        if (Engine.EditorHint && ExamplePlayerTheme == "" && CurrentTheme == null)
        {
            ExamplePlayerTheme = Constants.DEFAULT_PLAYER_THEME_PATH;
        }
    }
    public override void _Process(float delta)
    {
        if (_dirty)
        {
            _dirty = false;
            LoadThemeSetters();
        }
    }
}
