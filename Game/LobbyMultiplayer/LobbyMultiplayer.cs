

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

public class LobbyMultiplayer : Control
{
    Button _play;
    Button _quit;
    readonly int _amountOfPlayers = 2;  // zmienna, która będzie przechwytywana z popup'a

    enum PlayerAvatar
    {
        Avatar1,
        Avatar2,
        Avatar4
    }

    readonly List<TextureRect> _players = new List<TextureRect>();
    readonly int _black, _blue, _yellow, _green;
    readonly string _namePlayer = "Player";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _play = this.GetNodeSafe<Button>("Play");
        _play.OnButtonPressed(OnPlayPressed);

        _quit = this.GetNodeSafe<Button>("Quit");
        _quit.OnButtonPressed(OnQuitPressed);

        this.GetNodeSafe<WindowDialog>("Dialog0").PopupCentered();

        // widoczni gracze tylko w liczbie amountOfPlayers
        PreparePlayers(_amountOfPlayers);
        PlayersVisibilityOn();
    }

    void OnPopupHide()
    {
        PreparePlayers(_amountOfPlayers);
        PlayersVisibilityOn();
    }

    void OnPlayPressed()
    {
        // Load avatars of players
        PlayerAvatar _playerBlack = (PlayerAvatar)_black;
        GD.Print($"Black player is {_playerBlack}");
        PlayerAvatar _playerBlue = (PlayerAvatar)_blue;
        GD.Print($"Blue player is {_playerBlue}");
        PlayerAvatar _playerYellow = (PlayerAvatar)_yellow;
        GD.Print($"Yellow player is {_playerYellow}");
        PlayerAvatar _playerGreen = (PlayerAvatar)_green;
        GD.Print($"Green player is {_playerGreen}");

        var redtheme = Globals.PersonalThemes["red"].Copy();
        var blacktheme = Globals.PersonalThemes["black"].Copy();
        var bluetheme = Globals.PersonalThemes["blue"].Copy();
        var yellowtheme = Globals.PersonalThemes["yellow"].Copy();
        var greentheme = Globals.PersonalThemes["green"].Copy();

        var avatarplayerPath = "res://GUI/avatars/avatar3.png";
        redtheme.IconPath = avatarplayerPath;
        redtheme.AvatarPath = avatarplayerPath;

        var generators = new List<Game.AgentGenerator>()
        {
            (g, e, i, p, rng) => new Game.GameLocalAgent(g, _namePlayer, p, redtheme),
        };

        // choose and add players
        ChoosePlayer(blacktheme, _playerBlack, generators);
        if (_amountOfPlayers > 2)
            ChoosePlayer(bluetheme, _playerBlue, generators);
        if (_amountOfPlayers > 3)
            ChoosePlayer(yellowtheme, _playerYellow, generators);
        if (_amountOfPlayers > 4)
            ChoosePlayer(greentheme, _playerGreen, generators);

        var ui = (InGameUI)Globals.Scenes.InGameUIPacked.Instance();
        ulong seed = new RNG().NextULong(); // TODO: find a better way to do this
        seed = 666; // <--- UNCOMMENT FOR TESTING
        GD.Print("Seed is: ", seed);
        var game = Game.NewLocalGame(ui, generators, "BaseGame/BaseTileset.json", seed);
        ui.SetGame(game);
        GetTree().Root.AddChild(ui);
        DestroyNode(this);
    }

    // choose and add player to the game
    void ChoosePlayer(PersonalTheme theme, PlayerAvatar avatar, List<Game.AgentGenerator> generator)
    {
        var avatarPath = "";
        Game.AgentGenerator player = null;
        switch (avatar)
        {
            case PlayerAvatar.Avatar1:
                avatarPath = "res://GUI/avatars/avatar1.png";
                theme.IconPath = avatarPath;
                theme.AvatarPath = avatarPath;
                player = (g, e, i, p, rng) => new Game.GameLocalAgent(g, $"Player", p, theme);
                break;
            case PlayerAvatar.Avatar2:
                avatarPath = "res://GUI/avatars/avatar2.png";
                theme.IconPath = avatarPath;
                theme.AvatarPath = avatarPath;
                player = (g, e, i, p, rng) => new Game.GameLocalAgent(g, $"Player", p, theme);
                break;
            case PlayerAvatar.Avatar4:
                avatarPath = "res://GUI/avatars/avatar4.png";
                theme.IconPath = avatarPath;
                theme.AvatarPath = avatarPath;
                player = (g, e, i, p, rng) => new Game.GameLocalAgent(g, $"Player", p, theme);
                break;
            default:
                avatarPath = "res://GUI/avatars/avatar3.png";
                theme.IconPath = avatarPath;
                theme.AvatarPath = avatarPath;
                player = (g, e, i, p, rng) => new Game.GameLocalAgent(g, $"Player", p, theme);
                break;
        }

        generator.Add(player);
    }

    void PreparePlayers(int amountOfPlayers)
    {
        if (amountOfPlayers > 2)
            PlayersAdd("Blue");
        if (amountOfPlayers > 3)
            PlayersAdd("Yellow");
        if (amountOfPlayers > 4)
            PlayersAdd("Green");
    }

    void PlayersAdd(string color)
    {
        _players.Add(this.GetNodeSafe<TextureRect>($"GridContainer/GridContainerAvatars/HBoxContainerAvatar1/Avatar1{color}"));
        _players.Add(this.GetNodeSafe<TextureRect>($"GridContainer/GridContainerAvatars/HBoxContainerAvatar2/Avatar2{color}"));
        _players.Add(this.GetNodeSafe<TextureRect>($"GridContainer/GridContainerAvatars/HBoxContainerAvatar4/Avatar4{color}"));
    }

    void PlayersVisibilityOn()
    {
        //_playerContainers.ForEach(this.Visible = false);
        for (int i = 0; i < _players.Count; i++)
        {
            _players[i].Visible = true;
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
