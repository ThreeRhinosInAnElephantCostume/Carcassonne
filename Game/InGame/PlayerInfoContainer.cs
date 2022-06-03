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
using Carcassonne;
using ExtraMath;
using Godot;
using Newtonsoft.Json;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;
using Expression = System.Linq.Expressions.Expression;
using Thread = System.Threading.Thread;

public class PlayerInfoContainer : ControlProp
{
    Game _game;
    Game.GameAgent _agent;
    Carcassonne.GameEngine.Player _player;
    TextureRectProp _playerAvatarRect;
    TextureRectProp _playerMeepleRect;
    Label _nMeeplesLabel;
    Label _nPointsLabel;
    Label _playerNameLabel;
    TextureRect _playerFrame;
    Sprite _activePlayerMarker;
    public void Init(Game game, Game.GameAgent player)
    {
        this._game = game;
        this._agent = player;
        this._player = player.Player;
        Defer
        (
            () =>
            {
                (this as IProp).CurrentTheme = _agent.CurrentTheme;
                _playerAvatarRect.Texture = _agent.CurrentTheme.Avatar;
            }
        );
    }
    public void UpdatePlayer()
    {
        Defer(RealUpdatePlayer);
    }
    void RealUpdatePlayer()
    {
        Assert(_player != null);
        _nMeeplesLabel.Text = $"{_player.Pawns.Count(it => it is Meeple meep && !meep.IsInPlay)}/{_player.Pawns.Count(it => it is Meeple)}"; // dlaczego nie aktualizuje się liczba meepli?
        _nPointsLabel.Text = $"{_player.Score}(+{_player.PotentialScore})";
        _playerNameLabel.Text = _agent.Name;
        _activePlayerMarker.Visible = (_game.CurrentAgent == _agent);
    }
    public override void _Ready()
    {
        _nMeeplesLabel = this.GetNodeSafe<Label>("PlayerContainerH/PlayerStatusContainer/MeepleContainer/MeeplesPlayer");
        _nPointsLabel = this.GetNodeSafe<Label>("PlayerContainerH/PlayerStatusContainer/PointsContainer/PointsPlayer");
        _playerNameLabel = this.GetNodeSafe<Label>("PlayerContainerH/PlayerStatusContainer/PlayerName");
        _playerFrame = this.GetNodeSafe<TextureRect>("PlayerContainerH/InfoPanelAvatar");
        _playerAvatarRect = _playerFrame.GetNodeSafe<TextureRectProp>("Background");
        _playerMeepleRect = this.GetNodeSafe<TextureRectProp>("PlayerContainerH/PlayerStatusContainer/MeepleContainer/MeepleImg");
        _activePlayerMarker = this.GetNodeSafe<Sprite>("ActivePlayer");
        _activePlayerMarker.Visible = false;
    }
}
