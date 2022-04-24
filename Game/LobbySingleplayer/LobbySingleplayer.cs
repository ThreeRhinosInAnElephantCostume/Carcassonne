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

    int _amountOfBots = 4;  // zmienna, która będzie przechwytywana z popup'a

    enum BotLevel
    {
        Easy,
        Mid,
        Hard
    }


    List<TextureRect> _bots = new List<TextureRect>();


    // TODO: popup: (Enter your name: . How many opponents do you want? ) zczytana liczba przeciwników, min 1 max 4 (jeśli mniej niż 1, ustaw 1, jeśli więcej niż 4 ustaw 4)
    // TODO: liczba botów przekazana do _amountOfBots
    // TODO: nazwa gracza przekazana do Label gracza
    // TODO: odświeżenie sceny
    //
    // black -> blue -> yellow -> green
    // 

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _play = this.GetNodeSafe<Button>("Play");
        _play.OnButtonPressed(OnPlayPressed);

        _quit = this.GetNodeSafe<Button>("Quit");
        _quit.OnButtonPressed(OnQuitPressed);

        // widoczne boty tylko w liczbie amountOfBots
        PrepareBots(_amountOfBots);
        BotsVisibilityOn();
    }

    void OnPlayPressed()
    {
        var blacktheme = Globals.PersonalThemes["black"].Copy();
        var bluetheme = Globals.PersonalThemes["blue"].Copy();
        var yellowtheme = Globals.PersonalThemes["yellow"].Copy();
        var greentheme = Globals.PersonalThemes["green"].Copy();
        
        var generators = new List<Game.AgentGenerator>()
        {
            (g, e, i, p, rng) => new Game.GameLocalAgent(g, $"Player", p, Globals.PersonalThemes["red"].Copy()),
        };

        // choose and add bots
        ChooseBot(blacktheme, BotLevel.Easy, generators);
        if(_amountOfBots > 1)
            ChooseBot(bluetheme, BotLevel.Easy, generators);
        if(_amountOfBots > 2)    
            ChooseBot(yellowtheme, BotLevel.Easy, generators);
        if(_amountOfBots > 3)   
            ChooseBot(greentheme, BotLevel.Easy, generators);

        var ui = (InGameUI)Globals.Scenes.InGameUIPacked.Instance();
        var game = Game.NewLocalGame(ui, generators, "BaseGame/BaseTileset.json", 666);
        ui.SetGame(game);
        GetTree().Root.AddChild(ui);
        DestroyNode(this);
    }

    // choose and add bot to the game
    void ChooseBot(PersonalTheme theme, BotLevel level, List<Game.AgentGenerator> generator)
    {
        var avatarbotPath = "";
        Game.AgentGenerator bot = null; 
        switch (level)
        {
            case BotLevel.Easy:
                avatarbotPath = "res://GUI/avatars/avatarbot3.png";
                theme.IconPath = avatarbotPath;
                theme.AvatarPath = avatarbotPath;
                bot = (g, e, i, p, rng) => new Game.GameAIAgent(g, $"AI", p, new AI.RandomAI(new RNG(rng.NextULong())), theme);
                break;
            case BotLevel.Mid:
                avatarbotPath = "res://GUI/avatars/avatarbot2.png";
                theme.IconPath = avatarbotPath;
                theme.AvatarPath = avatarbotPath;
                bot = (g, e, i, p, rng) => new Game.GameAIAgent(g, $"AI", p, new AI.RandomAI(new RNG(rng.NextULong())), theme); //zmienić na AI.Medium
                break;
            case BotLevel.Hard:
                avatarbotPath = "res://GUI/avatars/avatarbot1.png";
                theme.IconPath = avatarbotPath;
                theme.AvatarPath = avatarbotPath;
                bot = (g, e, i, p, rng) => new Game.GameAIAgent(g, $"AI", p, new AI.RandomAI(new RNG(rng.NextULong())), theme); //zmienić na AI.Hard
                break;
            default:
                avatarbotPath = "res://GUI/avatars/avatarbot3.png";
                theme.IconPath = avatarbotPath;
                theme.AvatarPath = avatarbotPath;
                bot = (g, e, i, p, rng) => new Game.GameAIAgent(g, $"AI", p, new AI.RandomAI(new RNG(rng.NextULong())), theme);
                break;
        }
                
        generator.Add(bot);
    }

    void PrepareBots(int amountOfBots)
    {
        if(amountOfBots > 1)
            BotsAdd("Blue");
        if(amountOfBots > 2)    
            BotsAdd("Yellow");
        if(amountOfBots > 3)   
            BotsAdd("Green");
    }

    void BotsAdd(string color)
    {
        _bots.Add(this.GetNodeSafe<TextureRect>($"GridContainer/GridContainerBots/HBoxContainerBotsEasy/BotEasy{color}"));
        _bots.Add(this.GetNodeSafe<TextureRect>($"GridContainer/GridContainerBots/HBoxContainerBotsMid/BotMid{color}"));
        _bots.Add(this.GetNodeSafe<TextureRect>($"GridContainer/GridContainerBots/HBoxContainerBotsHard/BotHard{color}"));
    }

    void BotsVisibilityOn()
    {
        //_playerContainers.ForEach(this.Visible = false);
        for (int i = 0; i < _bots.Count; i++)
        {
            _bots[i].Visible = true;
        }
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
