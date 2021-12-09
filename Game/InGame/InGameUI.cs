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
		_stateLabel.Text = $"STATE: {_engine.CurrentPlayer}";
		for (int i = 0; i < _engine.Players.Count; i++)
		{
			var p = _engine.Players[i];
			var l = _playerLabels[i];
			l.Text = $"Player {p.ID}: {p.Score} (+{p.PotentialScore})\n";
			if (p == _engine.CurrentPlayer)
			{
				l.Text += " <<<<<";
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

		RepeatN(_engine.Players.Count, i =>
		{
			var l = new Label();
			_playerLabels.Add(l);
			_mainInfoContainer.AddChild(l);
		});

		Start(_game);
	}

}
