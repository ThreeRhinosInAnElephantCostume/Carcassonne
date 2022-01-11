﻿using System;
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

public static partial class Globals
{
    public static MainSettings Settings { get; set; } = new MainSettings();
    public static PackedScene InGameUIPacked { get; set; }
    public static PackedScene MainMenuPacked { get; set; }
    public static PackedScene PotentialTilePacked { get; set; }
    public static PackedScene MeeplePlacementPacked { get; set; }
    public static PackedScene PotentialMeeplePlacementPacked { get; set; }
}
