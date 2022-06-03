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
using Carcassonne;
using ExtraMath;
using Godot;
using Newtonsoft.Json;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;
using Expression = System.Linq.Expressions.Expression;
using Thread = System.Threading.Thread;

[Tool]
public class ScrollTest : ScrollContainer
{
    [Export(PropertyHint.Range, "1,1000")]
    public float Speed = 10f;
    Control _content;
    public float Scroll = 0;
    float limit = 0;
    public override void _Ready()
    {
        this.ScrollVertical = 0;
        this.ScrollVerticalEnabled = true;
    }
    public override void _Process(float delta)
    {
        _content = this.GetChildren<Control>().Find(it => !(it is ScrollBar));
        if (_content == null)
            return;
        limit = _content.RectSize.y - RectSize.y;
        if (limit <= 0)
        {
            this.ScrollVertical = 0;
            Scroll = 0;
            return;
        }
        Scroll += (Speed * delta);
        Scroll %= limit;
        this.ScrollVertical = (int)Scroll;
    }
}
