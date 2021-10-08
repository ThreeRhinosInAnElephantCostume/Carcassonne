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

public class NodeTypePrototype : Resource
{
    [Export]
    public string Name { get; set; }
    [Export]
    public uint ID { get; set; }
    [Export]
    public string Abrv { get; set; }

    public NodeType Convert()
    {
        Assert(Abrv.Length > 0);
        return new NodeType(ID, Name, Abrv[0]);
    }
    public NodeTypePrototype(uint ID = 0, string Name = "NullType", string Abrv = "#")
    {
        this.ID = ID;
        this.Name = Name;
        this.Abrv = Abrv;
        if (Abrv.Length != 1)
            GD.PrintErr("Invalid Abrv length");
    }
    public NodeTypePrototype(NodeType nt) : this(nt.ID, nt.Name, nt.Abrv)
    {

    }

}
