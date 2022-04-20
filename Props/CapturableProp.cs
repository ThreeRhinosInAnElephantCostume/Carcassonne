﻿// Licensed to the .NET Foundation under one or more agreements.
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
using Carcassonne;
using ExtraMath;
using Godot;
using Newtonsoft.Json;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;
using Expression = System.Linq.Expressions.Expression;

[Tool]
public class CapturableProp : SpatialProp, IProp
{
    int _requiredMeeple = 0;
    [Export(PropertyHint.Enum, "None,Farmer,Knight,Highwayman,Monk")]
    public int RequiredMeeple
    {
        get => _requiredMeeple;
        set { _requiredMeeple = value; (this as IProp).UpdateProp(); }
    }
    int _requiredState = 0;
    [Export(PropertyHint.Enum, "None,Unoccupied,Captured,Contested,Potential")]
    public int RequiredState
    {
        get => _requiredState;
        set { _requiredState = value; (this as IProp).UpdateProp(); }
    }

    public bool Potential = false;
    (OccupierContainer container, Map.Graph graph) _data;
    public (OccupierContainer container, Map.Graph graph)? Data
    {
        get => _data;
        set
        {
            _data = value.Value;
            foreach (var it in this.FindChildren<CapturableProp>())
            {
                it.Data = value;
            }
        }
    }

    void IProp.UpdateTheme()
    {
        var prop = (IProp)this;
        if(Engine.EditorHint)
        {
            Visible = true;
            return;
        }
        if (Data == null || (((this as IProp).CurrentTheme) == null && RequiredState != 0))
        {
            Visible = false;
            return;
        }
        var data = Data.Value;
        Assert(data.container != null);
        bool showprop = true;
        showprop = (RequiredMeeple) switch
        {
            (0) => true,
            _ => data.container.Occupiers.Any(it => it is Meeple meep &&
                 meep.CurrentRole == (Meeple.Role)RequiredMeeple)
        };
        if (!showprop)
            goto end;
        int nowners;
        if(data.graph == null)
            nowners = data.container.Occupiers.Count;
        else
            nowners = data.graph.Owners.Count;
        showprop = (RequiredState) switch
        {
            0 => true,
            // Unoccupied
            1 => nowners == 0,
            // Captured
            2 => nowners == 1,
            // Contested
            3 => nowners > 1,
            // Potential
            4 => Potential,
            _ => throw new Exception("impossible RequiredState")
        };
end:;
        Visible = showprop;
    }
    bool _dirty = true;

    PersonalTheme IProp._theme { get; set; } = null;
    string IProp._examplePlayerTheme { get; set; } = "";
    List<IProp> IProp._children { get; set; } = new List<IProp>();
    IProp IProp._parent { get; set; }
    public override void _Ready()
    {
        (this as IProp).InitHierarchy();
    }
    public override void _Process(float delta)
    {
        if (_dirty)
        {
            _dirty = false;
            (this as IProp).UpdateProp();
        }
    }

}
