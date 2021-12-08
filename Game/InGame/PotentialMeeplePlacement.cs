﻿using System;
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

public class PotentialMeeplePlacement : Spatial
{
    const string PLACE_ACTION = "map_tile_place";
    static string[] ACTIONS = new string[] { PLACE_ACTION };
    Area _area;
    public Game.GameLocalAgent Agent { get; set; }
    public int Index { get; set; }
    public bool IsAttribute { get; set; }
    void MouseEntered()
    {

    }
    void MouseExited()
    {

    }
    void AreaInputEvent(Camera camera, InputEvent @event, Vector3 clickpos, Vector3 clicknormal, int shapeindx)
    {
        if (InputMap.EventIsAction(@event, PLACE_ACTION) && Input.IsActionJustPressed(PLACE_ACTION))
        {
            if (IsAttribute)
                Agent.PlaceMeepleOnAttribute(Index);
            else
                Agent.PlaceMeepleOnNode(Index);
        }
    }

    public override void _Ready()
    {
        foreach (var it in ACTIONS)
            Assert(InputMap.HasAction(it));

        _area = GetNode<Area>("Area");
        _area.Connect("mouse_entered", this, "MouseEntered");
        _area.Connect("mouse_exited", this, "MouseExited");
        _area.Connect("input_event", this, "AreaInputEvent");
    }
}