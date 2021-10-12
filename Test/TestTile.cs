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
using static System.Math;
using static Utils;

public class TestTile : Node2D
{
    [Export]
    public float size = 100;

    public Vector2 outersize = new Vector2();
    Vector2I _gridposition = new Vector2I();
    public Vector2I GridPosition
    {
        get
        {
            return _gridposition;
        }
        set
        {
            _gridposition = value;
            this.Position = new Vector2(_gridposition.x * size, -_gridposition.y * size);
        }
    }
    public override void _Ready()
    {
        outersize = new Vector2(size, size);
    }
    static int _ev = 0;
    public override void _Process(float delta)
    {
        outersize = new Vector2(size, size);
        if (Godot.Engine.EditorHint)
        {
            if (_ev >= 15)
            {
                Update();
                _ev = 0;
            }
            _ev++;
        }
    }
    public TestTile()
    {
        outersize = new Vector2(size, size);
    }
}
