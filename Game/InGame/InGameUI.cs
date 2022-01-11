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
    Spatial _previewRoot;
    Camera _mainCamera;
    VBoxContainer _mainInfoContainer;
    Label _stateLabel;
    List<Label> _playerLabels = new List<Label>();

    List<HBoxContainer> _playerContainers = new List<HBoxContainer>();
    List<Label> _playerPoints = new List<Label>();
    List<Label> _playerStatus = new List<Label>();

    HSlider _musicVolumeSlider;
    HSlider _effectsVolumeSlider;

    AudioPlayer _gameAudio;
    //Viewport _viewport;
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
        //		_stateLabel.Text = $"STATE: {_engine.CurrentPlayer}";
        for (int i = 0; i < _engine.Players.Count; i++)
        {
            var p = _engine.Players[i];
            var l = _playerLabels[i];
            var pp = _playerPoints[i];
            var s = _playerStatus[i];
            //			l.Text = $"Player {p.ID}: {p.Score} (+{p.PotentialScore})\n";
            pp.Text = $"{p.Score} (+{p.PotentialScore})";
            if (p == _engine.CurrentPlayer)
            {
                //				l.Text += " <<<<<";
                s.Text = "active";
            }
            else
            {
                s.Text = "";
            }
        }
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
    public override void _Process(float delta)
    {
        if (_map == null)
            return;
        if (_map.NextTile != null && _map.NextTile.GetParent() == null)
        {
            _previewRoot.AddChild(_map.NextTile);
        }
        _previewRoot.Rotation = new Vector3(-_mainCamera.Rotation.x, _mainCamera.Rotation.y, _mainCamera.Rotation.z);
    }

    public override void _Ready()
    {
        _inGame3D = GetNode<Spatial>("InGame3D");

        _mainInfoContainer = GetNode<VBoxContainer>("CanvasLayer/GameUIRoot/HBoxContainer/MainInfoContainer");

        _game = Game.NewLocalGame(this, 4, "BaseGame/BaseTileset.json");
        _previewRoot = GetNode<Spatial>("CanvasLayer/GameUIRoot/HBoxContainer/VBoxContainer/AspectRatioContainer/ViewportContainer/Viewport/PreviewRoot");

        _mainCamera = GetNode<Camera>("InGame3D/Camera");

        _map = new TileMap3D(_game);
        _map.Engine = _game.Engine;
        _inGame3D.AddChild(_map);

        _stateLabel = new Label();
        _mainInfoContainer.AddChild(_stateLabel);

        _playerContainers.Add(GetNode<HBoxContainer>("CanvasLayer/GameUIRoot/HBoxContainer/MainInfoContainer/Player1Container"));
        _playerContainers.Add(GetNode<HBoxContainer>("CanvasLayer/GameUIRoot/HBoxContainer/MainInfoContainer/Player2Container"));
        _playerContainers.Add(GetNode<HBoxContainer>("CanvasLayer/GameUIRoot/HBoxContainer/MainInfoContainer/Player3Container"));
        _playerContainers.Add(GetNode<HBoxContainer>("CanvasLayer/GameUIRoot/HBoxContainer/MainInfoContainer/Player4Container"));
        _playerContainers.Add(GetNode<HBoxContainer>("CanvasLayer/GameUIRoot/HBoxContainer/MainInfoContainer/Player5Container"));
        PlayerContainerVisibilityOff();

        _playerPoints.Add(GetNode<Label>("CanvasLayer/GameUIRoot/HBoxContainer/MainInfoContainer/Player1Container/Player1StatusContainer/PointsContainer/PointsPlayer1"));
        _playerPoints.Add(GetNode<Label>("CanvasLayer/GameUIRoot/HBoxContainer/MainInfoContainer/Player2Container/Player2StatusContainer/PointsContainer/PointsPlayer2"));
        _playerPoints.Add(GetNode<Label>("CanvasLayer/GameUIRoot/HBoxContainer/MainInfoContainer/Player3Container/Player3StatusContainer/PointsContainer/PointsPlayer3"));
        _playerPoints.Add(GetNode<Label>("CanvasLayer/GameUIRoot/HBoxContainer/MainInfoContainer/Player4Container/Player4StatusContainer/PointsContainer/PointsPlayer4"));
        _playerPoints.Add(GetNode<Label>("CanvasLayer/GameUIRoot/HBoxContainer/MainInfoContainer/Player5Container/Player4StatusContainer/PointsContainer/PointsPlayer5"));

        _playerStatus.Add(GetNode<Label>("CanvasLayer/GameUIRoot/HBoxContainer/MainInfoContainer/Player1Container/Player1StatusContainer/StatusPlayer1"));
        _playerStatus.Add(GetNode<Label>("CanvasLayer/GameUIRoot/HBoxContainer/MainInfoContainer/Player2Container/Player2StatusContainer/StatusPlayer2"));
        _playerStatus.Add(GetNode<Label>("CanvasLayer/GameUIRoot/HBoxContainer/MainInfoContainer/Player3Container/Player3StatusContainer/StatusPlayer3"));
        _playerStatus.Add(GetNode<Label>("CanvasLayer/GameUIRoot/HBoxContainer/MainInfoContainer/Player4Container/Player4StatusContainer/StatusPlayer4"));
        _playerStatus.Add(GetNode<Label>("CanvasLayer/GameUIRoot/HBoxContainer/MainInfoContainer/Player5Container/Player4StatusContainer/StatusPlayer5"));


        PlayerContainerVisibilityOn();

        RepeatN(_engine.Players.Count, i =>
        {
            var l = new Label();
            var s = new Label();
            _playerLabels.Add(l);
            _playerStatus.Add(s);
            _mainInfoContainer.AddChild(l);
            _mainInfoContainer.AddChild(s);
        });

        Start(_game);

        _gameAudio = GetNode<AudioPlayer>("/root/AudioPlayer");

        _effectsVolumeSlider = GetNode<HSlider>("CanvasLayer/GameUIRoot/HBoxContainer/VBoxContainer/HBoxContainer/AudioMenu/VBoxContainer/HBoxContainer/SoundVolumeSlider");
        _effectsVolumeSlider.MaxValue = 100;
        _effectsVolumeSlider.Value = _effectsVolumeSlider.MaxValue * Globals.Settings.Audio.EffectsVolume;

        _musicVolumeSlider = GetNode<HSlider>("CanvasLayer/GameUIRoot/HBoxContainer/VBoxContainer/HBoxContainer/AudioMenu/VBoxContainer/HBoxContainer2/MusicVolumeSlider");
        _musicVolumeSlider.MaxValue = 100;
        _musicVolumeSlider.Value = _musicVolumeSlider.MaxValue * Globals.Settings.Audio.MusicVolume;

    }

    void PlayerContainerVisibilityOff()
    {
        //_playerContainers.ForEach(this.Visible = false);
        for (int i = 0; i < 5; i++)
        {
            _playerContainers[i].Visible = false;
        }
    }

    void PlayerContainerVisibilityOn()
    {
        //_playerContainers.ForEach(this.Visible = false);
        for (int i = 0; i < _engine.Players.Count; i++)
        {
            _playerContainers[i].Visible = true;
        }
    }


    void OnMusicToggleButtonToggled(bool button_pressed)
    {
        _gameAudio.ToggleAudioBusVolume("Music");
    }

    void OnPlayNextSongButtonPressed()
    {
        _gameAudio.StopIntroMusic();

        _gameAudio.PlayNextSong();
    }

    void OnSoundToggleButtonToggled(bool button_pressed)
    {
        _gameAudio.ToggleAudioBusVolume("Sounds");
    }

    void OnSoundVolumeSliderValueChanged(float value)
    {
        Globals.Settings.Audio.EffectsVolume.Value = value / 100.0f;
    }

    void OnMusicVolumeSliderValueChanged(float value)
    {
        Globals.Settings.Audio.MusicVolume.Value = value / 100.0f;
    }

    public bool AudioMenuToggle()
    {
        Control audioMenu = GetNode<Control>("CanvasLayer/GameUIRoot/HBoxContainer/VBoxContainer/HBoxContainer/AudioMenu");
        if (audioMenu.Visible && Globals.Settings.Audio.Modified)
            GlobalScript.GS.SaveSettingsAsync(true);
        return (audioMenu.Visible = !audioMenu.Visible);
    }

    void OnAudioMenuButtonPressed()
    {
        AudioMenuToggle();
    }

    void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }
}









