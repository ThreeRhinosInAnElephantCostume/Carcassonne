using System;
using System.Collections.Generic;
using System.ComponentModel;
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

public class Test2D_GUI : Control
{
	Test2D test2D;
	TileMap map;
	GameEngine game;
	Camera2D camera;
	PlacedTile currenttile;
	List<Label> playerlabels = new List<Label>();
	List<Label> graphlabels = new List<Label>();
	CanvasLayer cl;
	Godot.Container sidecontainer;
	public override void _Ready()
	{
		cl = GetNode<CanvasLayer>("CanvasLayer");
		test2D = GetNode<Test2D>("Test2D");
		map = test2D.GetNode<TileMap>("TileMap");
		camera = GetNode<Camera2D>("Camera2D");
		currenttile = cl.GetNode<PlacedTile>("CurrentTile");

		game = map.game;

		currenttile.Position = currenttile.outersize / 2;

		test2D.Position = camera.GetViewport().Size / 2;
		this.MouseFilter = MouseFilterEnum.Ignore;

		sidecontainer = cl.GetNode("PlayerDataScroll").GetNode<Godot.Container>("PlayerDataContainer");

		foreach (var it in game.Players)
		{
			Label l = new Label();
			l.Align = Label.AlignEnum.Center;
			sidecontainer.AddChild(l);
			playerlabels.Add(l);
		}
		camera.Current = true;
	}
	public override void _Process(float delta)
	{
		camera.Offset = camera.GetViewport().Size / 2;
		if (game.CurrentState != GameEngine.State.PLACE_TILE)
		{
			currenttile.Visible = false;
		}
		if (game.GetCurrentTile() != currenttile.tile)
		{
			currenttile.tile = game.GetCurrentTile();
			currenttile.Update();
		}
		{
			int i = 0;
			foreach (var it in game.Players)
			{
				playerlabels[i].Text = "Player " + i.ToString() + "\n" + "Score: " + it.Score.ToString();
				if (it == game.CurrentPlayer)
					playerlabels[i].Modulate = new Color(1, 1, 1, 1);
				else
					playerlabels[i].Modulate = new Color(0.9f, 0.9f, 0.9f, 0.9f);
				i++;
			}
		}
		while (game.map.Graphs.Count > graphlabels.Count)
		{
			var l = new Label();
			l.Align = Label.AlignEnum.Center;
			sidecontainer.AddChild(l);
			graphlabels.Add(l);
		}
		while (game.map.Graphs.Count < graphlabels.Count)
		{
			graphlabels.Last().GetParent().RemoveChild(graphlabels.Last());
			graphlabels.Last().QueueFree();
			graphlabels.Remove(graphlabels.Last());
		}
		for (int i = 0; i < game.map.Graphs.Count; i++)
		{
			var g = game.map.Graphs[i];
			graphlabels[i].Text =
			  "Graph " + g.ID
			+ "\nTiles: " + g.Tiles.Count
			+ "\nNodes: " + g.Nodes.Count
			+ "\nConnections: " + g.Connections.Count
			+ "\nOpenconnections: " + g.OpenConnections.Count
			+ "\nIsClosed: " + g.IsClosed;
		}
	}
	public override void _Input(InputEvent @event)
	{
		switch (@event)
		{
			case InputEventMouseMotion ev:
				{
					if (Input.IsMouseButtonPressed((int)ButtonList.Middle))
					{
						test2D.Position += ev.Relative * camera.Zoom;
					}
					break;
				}
			case InputEventMouseButton ev:
				{
					if (ev.ButtonIndex == (int)ButtonList.WheelUp)
						camera.Zoom = new Vector2(1, 1) * (float)Max(camera.Zoom.x * 0.9f, 0.1f);
					else if (ev.ButtonIndex == (int)ButtonList.WheelDown)
					{
						camera.Zoom = new Vector2(1, 1) * (float)Min(camera.Zoom.x * 1.1f, 5f);
					}

					break;
				}
		}
		test2D._Input(@event);
	}
}
