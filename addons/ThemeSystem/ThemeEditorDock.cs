// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using ExtraMath;
using Godot;
using Newtonsoft.Json;
using static System.Math;
using static Constants.ShaderParams.ThemableShader;
using static Utils;

[Tool]
public class ThemeEditorDock : Control
{
    ThemeConfigurationPanel _themeConfigurationPanel;
    TextureRectProp _iconProp;
    TextureRectProp _avatarProp;
    Node _sceneRoot;
    public PersonalTheme CurrentTheme { get; protected set; }
    void SetScene(Node root)
    {
        this._sceneRoot = root;
        CallDeferred("UpdateTheme");
    }
    void UpdateTheme()
    {
        this.CurrentTheme = _themeConfigurationPanel.CurrentTheme;
        if (this.CurrentTheme != null)
        {
            _iconProp.Texture = CurrentTheme.Icon;
            _avatarProp.Texture = CurrentTheme.Avatar;
            (_iconProp as IProp).CurrentTheme = CurrentTheme;
            (_avatarProp as IProp).CurrentTheme = CurrentTheme;
        }
        if (this._sceneRoot != null && Engine.EditorHint)
        {
            foreach (var it in Utils.FindChildrenRecursively<IProp>(_sceneRoot))
            {
                it.CurrentTheme = CurrentTheme;
            }
        }
    }
    public override bool _Set(string property, object value)
    {
        if (property == "SceneRoot" && (value == null || value is Node))
        {
            SetScene(value as Node);
            return true;
        }
        return base._Set(property, value);
    }
    public override void _Ready()
    {
        if (Globals.DefaultTheme == null)
            Globals.DefaultTheme = DeserializeFromFile<PersonalTheme>(Constants.DataPaths.DEFAULT_PLAYER_THEME_PATH);
        CurrentTheme = Globals.DefaultTheme;
        _avatarProp = this.GetNodeSafe<TextureRectProp>("Panel/VBoxContainer/HBoxContainer/AvatarView");
        _iconProp = this.GetNodeSafe<TextureRectProp>("Panel/VBoxContainer/HBoxContainer/IconView");
        _themeConfigurationPanel = this.GetNodeSafe<ThemeConfigurationPanel>("Panel/VBoxContainer/ThemeConfigurationPanel");
        _themeConfigurationPanel.CurrentTheme = CurrentTheme;
        _themeConfigurationPanel.OnChangeHandle = UpdateTheme;
        CallDeferred(nameof(UpdateTheme));
    }
    public override void _Process(float delta)
    {
        if (_themeConfigurationPanel == null || _themeConfigurationPanel.GetParent() != this)
            _themeConfigurationPanel = this.GetNodeSafe<ThemeConfigurationPanel>("Panel/VBoxContainer/ThemeConfigurationPanel");
        _themeConfigurationPanel.OnChangeHandle = UpdateTheme;
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
