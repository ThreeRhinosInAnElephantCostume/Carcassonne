using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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

public class Test2D_MM : Control
{
    public static GameEngine GlobalGame = null;
    OptionButton _tilesetSelector;
    LineEdit _nPlayersEdit;
    Button _startGameButton;
    OptionButton _saveSelector;
    Button _loadGameButton;

    int nplayers = 2;

    void NPlayersChanged()
    {
        int n = 0;
        if (int.TryParse(_nPlayersEdit.Text, out n) && n > 1)
        {
            this.nplayers = n;
            _startGameButton.Disabled = false;
        }
        else
            _startGameButton.Disabled = true;
    }
    void KillYourself()
    {
        this.GetParent().RemoveChild(this);
        this.QueueFree();
    }
    void StartGamePressed()
    {
        GetTree().ChangeScene("res://Test/Test2D_GUI.tscn");
        var game = GameEngine.CreateBaseGame(new GameExternalDataLoader(), 666, nplayers, _tilesetSelector.Text);
        GlobalGame = game;
        KillYourself();
    }
    void LoadGamePressed()
    {
        GetTree().ChangeScene("res://Test/Test2D_GUI.tscn");
        var f = new Godot.File();
        Assert(f.Open(_saveSelector.Text, Godot.File.ModeFlags.Read));
        byte[] dt = f.GetBuffer((int)f.GetLen());
        f.Close();
        GameEngine game = GameEngine.Deserialize(new GameExternalDataLoader(), dt);
        GlobalGame = game;
        KillYourself();
    }

    public override void _Ready()
    {
        _tilesetSelector = GetNode<OptionButton>("HBoxContainer/VBoxContainer/StartGameContainer/OptionButton");
        _nPlayersEdit = GetNode<LineEdit>("HBoxContainer/VBoxContainer/StartGameContainer/LineEdit");
        _nPlayersEdit.Text = nplayers.ToString();
        _nPlayersEdit.Connect("text_changed", this, "NPlayersChanged");
        _startGameButton = GetNode<Button>("HBoxContainer/VBoxContainer/StartGameContainer/Button3");
        _startGameButton.Connect("pressed", this, "StartGamePressed");

        _saveSelector = GetNode<OptionButton>("HBoxContainer/VBoxContainer/LoadGameContainer/OptionButton");
        _loadGameButton = GetNode<Button>("HBoxContainer/VBoxContainer/LoadGameContainer/Button3");
        _loadGameButton.Connect("pressed", this, "LoadGamePressed");

        var tilesets = ListDirectoryFilesRecursively("res://Data/Tilesets").FindAll(s => s.EndsWith(".json"));
        if (tilesets.Count == 0)
        {
            _startGameButton.Disabled = true;
            _tilesetSelector.Text = "<<No tilesets found>>";
        }
        else
        {
            foreach (var it in tilesets)
            {
                _tilesetSelector.AddItem(it);
                _tilesetSelector.Selected = 0;
            }
        }

        var saves = ListDirectoryFiles("res://Test/Saves").FindAll(s => s.EndsWith(".carcassonne"));
        if (saves.Count == 0)
        {
            _loadGameButton.Disabled = true;
            _saveSelector.Text = "<<No saves found>>";
        }
        else
        {
            foreach (var it in saves)
            {
                _saveSelector.AddItem(it);
                _saveSelector.Selected = 0;
            }
        }
    }
}
