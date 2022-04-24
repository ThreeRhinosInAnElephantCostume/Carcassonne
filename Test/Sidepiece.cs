

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

public class Sidepiece : Area2D
{
    readonly PotentialTile potential;
    readonly int indx;
    bool mouseover = false;
    readonly PlacedTile nexttile;
    readonly CollisionPolygon2D collider;
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton e)
        {
            if (e.ButtonIndex == (int)ButtonList.Left && e.Pressed && mouseover)
            {
                potential.Trigger(indx);
            }
        }
    }
    public void MouseEnter()
    {
        mouseover = true;
        var par = ((Polygon2D)GetParent());
        var c = par.Color;
        c.a = 0.4f;
        par.Color = c;
        potential.MouseOver(indx, true);
    }
    public void MouseExit()
    {
        mouseover = false;
        var par = ((Polygon2D)GetParent());
        var c = par.Color;
        c.a = 0.8f;
        par.Color = c;
        potential.MouseOver(indx, false);
    }
    public override void _Ready()
    {
        AddChild(collider);
        InputPickable = true;

        Connect("mouse_entered", this, "MouseEnter");
        Connect("mouse_exited", this, "MouseExit");
    }
    public Sidepiece()
    {
        base.SetProcessInput(true);
    }
    public Sidepiece(PotentialTile potential, int indx, Vector2[] shape)
    {
        this.potential = potential;
        this.indx = indx;
        collider = new CollisionPolygon2D();
        collider.Polygon = shape;
    }
}
