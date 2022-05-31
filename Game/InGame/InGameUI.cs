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
    const string SKIP_MEEPLE_PLACEMENT_ACTION = "ui_skip_meeple";
    Game _game = null;
    TileMap3D _map;
    GameEngine _engine => _game.Engine;
    Spatial _inGame3D;
    Spatial _previewRoot;
    Camera _mainCamera;
    VBoxContainer _mainInfoContainer;

    TextureButton _skipPlacementButton;

    HSlider _musicVolumeSlider;
    HSlider _effectsVolumeSlider;

    TileEdgeIndicators _previewEdgeIndicator;

    AudioPlayer _gameAudio;
    readonly List<PlayerInfoContainer> _playerInfoContainers = new List<PlayerInfoContainer>();
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

        _skipPlacementButton.Disabled = !(_game.Engine.CurrentState == State.PLACE_PAWN);
        _playerInfoContainers.ForEach(pic => pic.UpdatePlayer());
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
    public void SetGame(Game game)
    {
        Assert(_game == null);
        _game = game;
    }
    public override void _Process(float delta)
    {
        if (_map == null)
            return;
        if (_map.NextTile == null)
        {
            _previewEdgeIndicator.LoseParent();
        }
        else if (_map.NextTile.GetParent() == null)
        {
            _previewRoot.AddChild(_map.NextTile);
            if (_previewEdgeIndicator.GetParent() != _map.NextTile)
            {
                _map.NextTile.StealChild(_previewEdgeIndicator);
            }
            _previewEdgeIndicator.SetUpFromTile(_map.NextTile);
            _previewEdgeIndicator.Translation = new Vector3(0, Constants.TILE_HEIGHT, 0);
            _previewEdgeIndicator.Visible = true;
        }
        else if (_map.NextTile.GetParent() != _previewRoot)
        {
            _previewEdgeIndicator.Visible = false;
        }
        _previewRoot.Rotation = new Vector3(-_mainCamera.Rotation.x, _mainCamera.Rotation.y, _mainCamera.Rotation.z);
        if (!_skipPlacementButton.Disabled && Input.IsActionJustPressed(SKIP_MEEPLE_PLACEMENT_ACTION))
            OnSkipMepleButtonPressed();
    }

    public override void _Ready()
    {
        _previewEdgeIndicator = Globals.Scenes.TileEdgeIndicatorsPacked.Instance<TileEdgeIndicators>();
        _previewEdgeIndicator.Name = "PEI";

        Assert(_game != null, "_game is null. Remember to call SetGame before initialization!");
        _inGame3D = GetNode<Spatial>("InGame3D");

        _mainInfoContainer = GetNode<VBoxContainer>("CanvasLayer/GameUIRoot/HBoxContainer/MainInfoContainer");

        _previewRoot = GetNode<Spatial>("CanvasLayer/GameUIRoot/HBoxContainer/VBoxContainer/AspectRatioContainer/ViewportContainer/Viewport/PreviewRoot");

        _mainCamera = GetNode<Camera>("InGame3D/Camera");

        _map = new TileMap3D(_game);
        _map.Engine = _game.Engine;
        _inGame3D.AddChild(_map);

        _skipPlacementButton = this.GetNodeSafe<TextureButton>("CanvasLayer/GameUIRoot/HBoxContainer/VBoxContainer/HBoxContainer2/SkipMepleButton");

        RepeatN(_engine.Players.Count, i =>
        {
            var pic = (PlayerInfoContainer)Globals.Scenes.PlayerInfoContainerPacked.Instance();
            pic.SetPlayer(_game.Agents[i]);
            _playerInfoContainers.Add(pic);
            _mainInfoContainer.AddChild(pic);
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

    public void AudioMenuToggle()
    {
        Control audioMenu = GetNode<Control>("CanvasLayer/GameUIRoot/HBoxContainer/VBoxContainer/HBoxContainer/AudioMenu");
        if (audioMenu.Visible && Globals.Settings.Audio.Modified)
            GlobalScript.GS.SaveSettingsAsync(true);
        audioMenu.Visible = !audioMenu.Visible;
    }

    void OnAudioMenuButtonPressed()
    {
        AudioMenuToggle();
    }

    void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }

    void OnSkipMepleButtonPressed()
    {
        _game.Engine.SkipPlacingPawn();
        _game.AgentExecuteImplied(_game.CurrentAgent);
    }
}









