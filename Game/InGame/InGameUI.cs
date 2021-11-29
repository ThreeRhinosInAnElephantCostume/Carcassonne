using System;
using System.Collections.Generic;
using System.Diagnostics;
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

public class InGameUI : Control, Game.IGameHandles
{
    Game _game = null;
    TileMap3D _map;
    GameEngine _engine => _game.Engine;
    Spatial _inGame3D;
    Viewport _viewport;
    public void Start(Game game)
    {
        this._game = game;
        UpdateUI();
    }
    public void UpdateUI()
    {
        _map.Playable = _game.CurrentAgent != null && _game.CurrentAgent is Game.GameLocalAgent;
        if (_map.Playable)
        {
            _map.Player = (Game.GameLocalAgent)_game.CurrentAgent;
        }
        _map.Update();
    }
    public override void _Ready()
    {
        _inGame3D = GetNode<Spatial>("MainViewportContainer/Viewport/InGame3D");
        _viewport = GetNode<Viewport>("MainViewportContainer/Viewport");

        _game = Game.NewLocalGame(this, 8, "BaseGame/BaseTileset.json");

        _map = new TileMap3D();
        _map.Engine = _game.Engine;
        _inGame3D.AddChild(_map);

        Start(_game);


        //GetTree().CurrentScene = this;
    }

    void Game.IGameHandles.OnAction(Game.GameAgent agent, GameEngine.Action action)
    {
        UpdateUI();
    }

    void Game.IGameHandles.OnGameOver(List<Game.GameAgent> winners)
    {
        UpdateUI();
    }

    void Game.IGameHandles.OnNextPlayerTurn(Game.GameAgent agent)
    {
        UpdateUI();
    }

    public override void _Input(InputEvent @event)
    {
        _viewport._Input(@event);
    }
    public override void _UnhandledInput(InputEvent @event)
    {
        _viewport._Input(@event);
    }
}
