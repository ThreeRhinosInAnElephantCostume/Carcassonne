// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
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
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

public class PotentialTile : TestTile
{
    public TileMap map;
    readonly List<Polygon2D> sides = new List<Polygon2D>();

    [Export]
    public float reader = 0;
    bool[] _possiblerots = new bool[4] { true, true, true, true };
    public bool[] PossibleRots
    {
        get
        {
            return _possiblerots;
        }
        set
        {
            _possiblerots = value;
        }
    }
    [Export]
    public bool Up { get => PossibleRots[0]; set => PossibleRots[0] = value; }
    [Export]
    public bool Right { get => PossibleRots[1]; set => PossibleRots[1] = value; }
    [Export]
    public bool Down { get => PossibleRots[2]; set => PossibleRots[2] = value; }
    [Export]
    public bool Left { get => PossibleRots[3]; set => PossibleRots[3] = value; }
    [Export]
    public Color edgecolor { get; set; } = new Color(0.7f, 0.7f, 0.7f, 0.7f);
    [Export]
    public float edgewidth = 3;
    public void Trigger(int indx)
    {
        if (map == null)
            return;
        if (!PossibleRots[indx])
            return;
        map.TriggerPlacement(GridPosition, indx);
    }
    public void MouseOver(int indx, bool isover)
    {
        if (map == null || !PossibleRots[indx])
            return;
        if (isover == false)
            map.DisablePotentiaPlacement();
        else
            map.SetPotentiaPlacement(this.GridPosition, indx);

    }

    public override void _Ready()
    {

        for (int i = 0; i < 4; i++)
        {
            Polygon2D p = (Polygon2D)GetNode(i.ToString());
            foreach (Node it in p.GetChildren())
            {
                if (it is Area)
                    it.QueueFree();
            }
            var a = new Sidepiece(this, i, p.Polygon);
            a.InputPickable = true;
            p.AddChild(a);


            sides.Add(p);
        }
    }
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion move)
        {

        }
    }
    public override void _Draw()
    {
        DrawRect(new Rect2(-outersize / 2, outersize), edgecolor, false, edgewidth);

        for (int i = 0; i < 4; i++)
        {
            sides[i].Visible = _possiblerots[i];
        }

    }
}
