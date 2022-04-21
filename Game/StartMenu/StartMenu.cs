// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Godot;
using static Utils;



public class StartMenu : Control
{
    Button _play;
    Button _quit;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _play = GetNode<Button>("Play");
        _play.Connect("pressed", this, nameof(OnPlayPressed));

        _quit = GetNode<Button>("Quit");
        _quit.Connect("pressed", this, nameof(OnQuitPressed));
    }

    void OnPlayPressed()
    {
        GD.Print("Play pressed!");
        GetTree().ChangeScene("res://Game/Loading/MainMenuLoad.tscn");
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
