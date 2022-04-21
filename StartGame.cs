// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Godot;
using static Utils;

public class StartGame : Node
{
    public override void _Process(float delta)
    {
        var start = (ResourceLoader.Load<PackedScene>("res://Game/StartMenu/StartMenu.tscn").Instance());
        GetTree().Root.AddChild(start);
        DestroyNode(this);
    }
}
