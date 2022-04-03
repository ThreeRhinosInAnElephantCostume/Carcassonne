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
public class SpatialProp : Spatial, IProp
{
    void IProp.UpdateTheme()
    {
        
    }
    bool _dirty = true;

    PersonalTheme IProp._theme { get; set; } = null;
    string IProp._examplePlayerTheme {get; set;} = "";
    List<IProp> IProp._children {get; set;} = new List<IProp>(); 
    IProp IProp._parent {get; set;}

    public void MarkForLoad()
    {
        _dirty = true;
    }
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
