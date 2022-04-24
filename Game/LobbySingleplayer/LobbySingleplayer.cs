// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
using ExtraMath;
using Godot;
using Newtonsoft.Json;
using static System.Math;
using static Utils;
using Expression = System.Linq.Expressions.Expression;
using Thread = System.Threading.Thread;

public class LobbySingleplayer : Control
{
    Button _play;
    Button _quit;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _play = this.GetNodeSafe<Button>("Play");
        _play.OnButtonPressed(OnPlayPressed);

        _quit = this.GetNodeSafe<Button>("Quit");
        _quit.OnButtonPressed(OnQuitPressed);
    }

    void OnPlayPressed()
    {
        var aitheme = Globals.PersonalThemes["red"].Copy();
        aitheme.IconPath = "res://GUI/avatars/avatarbot3.png";
        aitheme.AvatarPath = "res://GUI/avatars/avatarbot3.png";
        var generators = new List<Game.AgentGenerator>()
        {
            (g, e, i, p, rng) => new Game.GameLocalAgent(g, $"Player", p, Globals.PersonalThemes["blue"].Copy()),
            (g, e, i, p, rng) => new Game.GameAIAgent(g, $"AI", p, new AI.RandomAI(new RNG(rng.NextULong())), aitheme),
        };
        var ui = (InGameUI)Globals.Scenes.InGameUIPacked.Instance();
        var game = Game.NewLocalGame(ui, generators, "BaseGame/BaseTileset.json", 666);
        ui.SetGame(game);
        GetTree().Root.AddChild(ui);
        DestroyNode(this);
    }

    void OnQuitPressed()
    {
        GD.Print("Quit pressed!");
        GetTree().Quit();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {

    }
}
