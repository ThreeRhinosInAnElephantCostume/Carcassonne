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
using ExtraMath;
using Godot;
using Newtonsoft.Json;
using static System.Math;
using static Utils;
using Expression = System.Linq.Expressions.Expression;

public static partial class Globals
{
    public static PersonalTheme DefaultTheme { get; set; }
    public static Dictionary<string, PersonalTheme> PersonalThemes { get; set; }
    public static List<PersonalTheme> PersonalThemesList { get; set; }
    public static SettingsSystem.MainSettings Settings { get; set; } = new SettingsSystem.MainSettings();
    public static PackedScene InGameUIPacked { get; set; }
    public static PackedScene MainMenuPacked { get; set; }
    public static PackedScene PotentialTilePacked { get; set; }
    public static PackedScene MeeplePlacementPacked { get; set; }
    public static PackedScene PotentialMeeplePlacementPacked { get; set; }
}
