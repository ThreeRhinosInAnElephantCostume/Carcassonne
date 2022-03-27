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

public class MainMenu : Control
{
    AudioPlayer _gameAudio;

    Button _play;
    Button _quit;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _gameAudio = GetNode<AudioPlayer>("/root/AudioPlayer");
        _gameAudio.PlayMainMenuMusic(0);

        _play = GetNode<Button>("Play");
        _play.Connect("pressed", this, nameof(OnPlayPressed));

        _quit = GetNode<Button>("Quit");
        _quit.Connect("pressed", this, nameof(OnQuitPressed));
    }

    void OnPlayPressed()
    {
        GD.Print("Play pressed!");
        GetTree().Root.AddChild(Globals.InGameUIPacked.Instance());
        DestroyNode(this);
    }

    void OnQuitPressed()
    {
        GD.Print("Quit pressed!");
        GetTree().Quit();
    }

    private void _onPlayMouseEntered()
    {
        _gameAudio.PlaySound("TileOverSpotSound");
    }

    private void _onQuitMouseEntered()
    {
        _gameAudio.PlaySound("TileOverSpotSound");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {

    }
}
