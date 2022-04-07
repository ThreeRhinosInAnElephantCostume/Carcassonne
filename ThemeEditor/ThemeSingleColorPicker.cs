// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using ExtraMath;
using Godot;
using Newtonsoft.Json;
using static System.Math;
using static Utils;
using Expression = System.Linq.Expressions.Expression;
using Thread = System.Threading.Thread;

[Tool]
public class ThemeSingleColorPicker : HBoxContainer
{
    public Action<Color> OnColorChangedHandle = c => { };
    [Export]
    public string DisplayedName
    {
        get
        {
            Assert(_label != null || Engine.EditorHint);
            if (_label == null)
                return "ERROR: NO LABEL FOUND";
            return _label.Text;
        }
        set
        {
            if (_label == null)
            {
                Defer(() => this.DisplayedName = value);
                return;
            }
            _label.Text = value;
        }
    }
    [Export]
    public Color PickedColor
    {
        get
        {
            if (_picker == null && Engine.EditorHint)
                return new Color();
            return _picker.Color;
        }
        set
        {
            if (_picker == null)
            {
                Defer(() => this.PickedColor = value);
                return;
            }
            _picker.Color = value;
            _picker.Update();
        }
    }
    Label _label;
    ColorPickerButton _picker;
    void OnColorChanged(Color color)
    {
        OnColorChangedHandle(color);
    }
    public override void _Ready()
    {
        _picker = GetNode<ColorPickerButton>("ColorPicker");
        _label = GetNode<Label>("Label");
        _picker.Connect("color_changed", this, nameof(OnColorChanged));
    }

}
