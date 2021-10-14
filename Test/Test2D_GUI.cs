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
    Test2D _test2D;
    TileMap _tileMap;
    GameEngine _game;
    Camera2D _camera;
    PlacedTile _currenttile;
    List<Label> _playerlabels = new List<Label>();
    CanvasLayer _cl;
    Godot.Container _sidecontainer;
    TabContainer _playerTabContainer;
    VBoxContainer _mainButtonContaienr;
    List<VBoxContainer> _playerDataContainers = new List<VBoxContainer>();
    class PlacePawnButton : Button
    {
        GameEngine _game;
        InternalNode _node;
        int _indx;
        PlacedTile _placedTile => _gui._tileMap.tiledisplays.Find(it => it.tile == _node.ParentTile);
        Test2D_GUI _gui;
        void MouseEntered()
        {
            _placedTile.HighlightedNode = _indx;
            _placedTile.CallDeferred("update");
        }
        void MouseExited()
        {
            _placedTile.HighlightedNode = -1;
            _placedTile.CallDeferred("update");
        }
        public override void _Pressed()
        {
            SetProcessInput(false);
            if (_indx == -1)
            {
                _game.SkipPlacingPawn();
            }
            else
            {
                _placedTile.HighlightedNode = -1;
                _game.PlacePawnOnNode(_indx);
            }
            _gui._tileMap.UpdateDisplay();
            _gui.UpdateInterface();
        }
        public PlacePawnButton(Test2D_GUI gui, GameEngine game, InternalNode node, int indx)
        {
            Assert(gui != null);
            Assert(node != null || indx == -1);

            this._gui = gui;
            this._game = game;
            this._node = node;
            this._indx = indx;

            if (indx == -1)
            {
                this.Text = "SKIP";
            }
            else
            {
                this.Text = $"{node.Type} ({node.ParentTile.Nodes.IndexOf(node)})";
                this.Connect("mouse_entered", this, "MouseEntered");
                this.Connect("mouse_exited", this, "MouseExited");
            }
        }
    }
    public void UpdateInterface()
    {
        _currenttile.Visible = _game.CurrentState == GameEngine.State.PLACE_TILE;
        if (_game.GetCurrentTile() != _currenttile.tile)
        {
            _currenttile.tile = _game.GetCurrentTile();
            _currenttile.CallDeferred("update");
        }
        foreach (Node n in _mainButtonContaienr.GetChildren())
        {
            n.GetParent().RemoveChild(n);
            n.QueueFree();
        }
        if (_game.CurrentState == GameEngine.State.PLACE_PAWN)
        {
            int i = 0;
            Tile tile = _game.map[_game.CurrentPawnTarget()];
            _mainButtonContaienr.AddChild(new PlacePawnButton(this, _game, null, -1));
            foreach (var it in _game.PossibleMeepleNodePlacements())
            {
                InternalNode node = tile.Nodes[it];
                PlacePawnButton button = new PlacePawnButton(this, _game, node, it);
                _mainButtonContaienr.AddChild(button);
                i++;
            }
        }

        {
            int i = 0;
            foreach (var it in _game.Players)
            {
                _playerlabels[i].Text = "Player " + i.ToString() + "\n" + "Score: " + it.Score.ToString()
                    + $" ({it.PotentialScore})" + $"\nPawns left: { _game.CountPlayerMeeplesLeft(it) }";
                if (it == _game.CurrentPlayer)
                {
                    _playerlabels[i].Modulate = new Color(1, 1, 1, 1);
                    _playerTabContainer.CurrentTab = i;
                }
                else
                    _playerlabels[i].Modulate = new Color(0.9f, 0.9f, 0.9f, 0.9f);

                foreach (Node n in _playerDataContainers[i].GetChildren())
                {
                    n.GetParent().RemoveChild(n);
                    n.QueueFree();
                }
                foreach ((Map.Graph graph, bool iscontested) in _game.GetGraphsOwnedBy(it))
                {
                    var l = new Label();
                    l.Text = $"ID: {graph.ID}\nType: {graph.Type}\nTiles: {graph.Tiles.Count}\nIsContested: {iscontested}";
                    _playerDataContainers[i].AddChild(l);
                }
                i++;
            }
        }
    }
    public override void _Ready()
    {
        _cl = GetNode<CanvasLayer>("CanvasLayer");
        _test2D = GetNode<Test2D>("Test2D");
        _tileMap = _test2D.GetNode<TileMap>("TileMap");
        _camera = GetNode<Camera2D>("Camera2D");
        _currenttile = _cl.GetNode<PlacedTile>("HBoxContainer/VBoxContainer/PanelContainer/CurrentTile");
        _sidecontainer = _cl.GetNode("HBoxContainer/VBoxContainer/PlayerDataScroll")
            .GetNode<Godot.Container>("PlayerDataContainer");

        _mainButtonContaienr = _cl.GetNode<VBoxContainer>("HBoxContainer/VBoxContainer2/MainButtonContainer");
        _playerTabContainer = _cl.GetNode<TabContainer>("HBoxContainer/VBoxContainer2/PlayerTabs");


        var cc = GetNode<CenterContainer>("CanvasLayer/HBoxContainer/CenterContainer");
        cc.MouseFilter = MouseFilterEnum.Ignore;

        _game = _tileMap.game;

        _currenttile.Position = _currenttile.outersize / 2;

        _test2D.Position = _camera.GetViewport().Size / 2;
        this.MouseFilter = MouseFilterEnum.Ignore;

        int i = 0;
        foreach (var it in _game.Players)
        {
            Label l = new Label();
            l.Align = Label.AlignEnum.Center;
            _sidecontainer.AddChild(l);
            _playerlabels.Add(l);
            ScrollContainer sc = new ScrollContainer();
            sc.Name = $"{i}";
            _playerTabContainer.AddChild(sc);
            VBoxContainer vb = new VBoxContainer();
            sc.AddChild(vb);
            _playerDataContainers.Add(vb);
            i++;
        }
        _camera.Current = true;
        _tileMap.UpdateHandle = () => UpdateInterface();
        UpdateInterface();
    }
    public override void _Process(float delta)
    {
        _camera.Offset = _camera.GetViewport().Size / 2;
    }
    public override void _Input(InputEvent @event)
    {
        switch (@event)
        {
            case InputEventMouseMotion ev:
                {
                    if (Input.IsMouseButtonPressed((int)ButtonList.Middle))
                    {
                        _test2D.Position += ev.Relative * _camera.Zoom;
                    }
                    break;
                }
            case InputEventMouseButton ev:
                {
                    if (ev.ButtonIndex == (int)ButtonList.WheelUp)
                        _camera.Zoom = new Vector2(1, 1) * (float)Max(_camera.Zoom.x * 0.9f, 0.1f);
                    else if (ev.ButtonIndex == (int)ButtonList.WheelDown)
                    {
                        _camera.Zoom = new Vector2(1, 1) * (float)Min(_camera.Zoom.x * 1.1f, 5f);
                    }

                    break;
                }
        }
        _test2D._Input(@event);
    }
}
