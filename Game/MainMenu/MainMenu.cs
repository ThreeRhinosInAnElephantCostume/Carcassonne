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

public class MainMenu : Control, SaveLoadGame.ISaveLoadHandler
{
    AudioPlayer _gameAudio;

    Button _play;
    Button _statistics;
    Button _help;
    Button _credits;
    Button _quit;
    BaseButton _loadGameButton;
    SaveLoadGame _saveLoadGame;

    void SetButtons(bool enabled)
    {
        foreach(var it in this.GetChildren())
        {
            if(it is BaseButton b)
            {
                b.Disabled = !enabled;
            }
        }
    }
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _gameAudio = this.GetNodeSafe<AudioPlayer>("/root/AudioPlayer");
        _gameAudio.PlayMainMenuMusic(0);

        _play = this.GetNodeSafe<Button>("Play");
        _play.Connect("pressed", this, nameof(OnPlayPressed));

        _statistics = this.GetNodeSafe<Button>("Statistics");
        _statistics.Connect("pressed", this, nameof(OnStatisticsPressed));

        _help = this.GetNodeSafe<Button>("Help");
        _help.Connect("pressed", this, nameof(OnHelpPressed));

        _credits = this.GetNodeSafe<Button>("Credits");
        _credits.Connect("pressed", this, nameof(OnCreditsPressed));

        _quit = this.GetNodeSafe<Button>("Quit");
        _quit.Connect("pressed", this, nameof(OnQuitPressed));

        _loadGameButton = this.GetNodeSafe<BaseButton>("Load");
        _loadGameButton.OnButtonPressed(() => 
        {
            SetButtons(false);
            _saveLoadGame.StartLoad(this, Constants.DataPaths.SAVES_BASE_PATH, Constants.DataPaths.SAVES_BASE_PATH);
        });

        _saveLoadGame = this.GetNodeSafe<SaveLoadGame>("SaveLoadGame");
    }

    void OnPlayPressed()
    {
        SetMainScene(Globals.Scenes.SingleMultiSelectionPacked);
    }

    void OnStatisticsPressed()
    {
        SetMainScene(Globals.Scenes.StatisticsPacked);
    }

        void OnHelpPressed()
    {
        SetMainScene(Globals.Scenes.HelpPacked);
    }

    void OnCreditsPressed()
    {
        SetMainScene(Globals.Scenes.CreditsPacked);
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

    bool SaveLoadGame.ISaveLoadHandler.CanDelete(string path)
    {
        return true;
    }

    void SaveLoadGame.ISaveLoadHandler.OnSelected(string path)
    {
        InGameUI.LoadGameFromFile(path);
    }

    void SaveLoadGame.ISaveLoadHandler.OnDelete(string path)
    {
        
    }

    void SaveLoadGame.ISaveLoadHandler.OnCancelled()
    {
        SetButtons(true);
    }
}
