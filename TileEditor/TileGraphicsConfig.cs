using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Threading;
using Carcassonne;
using ExtraMath;
using Godot;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

public class TileGraphicsConfig
{
    public class Config
    {
        public int Rotation { get; set; } = 0;
        public Dictionary<string, int> Nodeassociations { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> Attributeassociations { get; set; } = new Dictionary<string, int>();
        public List<string> Unassociated { get; set; } = new List<string>();

    }
    public string Path { get; set; } = "";
    public Dictionary<string, Config> Configs { get; set; } = new Dictionary<string, Config>();
}
