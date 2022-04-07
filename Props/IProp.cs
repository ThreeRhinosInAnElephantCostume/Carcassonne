// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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

public interface IProp
{
    PersonalTheme _theme { get; protected set; }
    PersonalTheme CurrentTheme
    {
        get
        {
            if (_theme == null && Engine.EditorHint)
            {
                if (Globals.DefaultTheme == null)
                    Globals.DefaultTheme = DeserializeFromFile<PersonalTheme>(Constants.DataPaths.DEFAULT_PLAYER_THEME_PATH);
                return Globals.DefaultTheme;
            }
            return _theme;
        }
        set { _theme = value; UpdateProp(); }
    }
    string _examplePlayerTheme { get; protected set; }
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

    List<IProp> _children { get; set; }
    IProp _parent { get; set; }
    public void AddSubProp(IProp child)
    {
        if (child == this)
            return;
        _children.Add(child);
        child.UpdateProp();
    }
    public void UpdateTheme();
    public void UpdateProp()
    {
        if (_parent != null)
            this._theme = _parent._theme;
        if (CurrentTheme == null)
            return;
        foreach (var it in _children)
        {
            it._theme = _theme;
            it.UpdateProp();
        }
        UpdateTheme();
    }
    public void InitHierarchy()
    {
        if (this is Node node)
        {

            Node Parent = node.GetParent();
            while (Parent != null)
            {
                if (Parent is IProp parent)
                {
                    _parent = parent;
                    parent.AddSubProp(this);
                    break;
                }
                Parent = Parent.GetParent();
            }
            if (Engine.EditorHint && ExamplePlayerTheme == "" && CurrentTheme == null)
            {
                ExamplePlayerTheme = Constants.DataPaths.DEFAULT_PLAYER_THEME_PATH;
            }
        }
    }
}
