﻿// Licensed to the .NET Foundation under one or more agreements.
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

public class PropView : Panel
{
    Camera _camera;
    Spatial _viewRoot;
    public override void _Ready()
    {
        _viewRoot = GetNode<Spatial>("ViewportContainer/Viewport/PropViewRoot");
        _camera = GetNode<Camera>("ViewportContainer/Viewport/PropViewRoot/PropViewCamera");
    }
}
