using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
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
    VBoxContainer _mainButtonContainer;
    List<VBoxContainer> _playerDataContainers = new List<VBoxContainer>();
    List<Button> _pawnPlaceButtons = new List<Button>();
    Button _saveButton;
    Button _undoButton;
    Button _redoButton;
    Label _hashLabel;
    class GraphLabel : Label
    {
        TileMap _tilemap;
        Map.Graph _graph;
        void MouseEntered()
        {
            _tilemap.tiledisplays.ForEach(td =>
                RepeatN(td.RenderedTile.Nodes.Count, i =>
                    { if (td.RenderedTile.Nodes[i].Graph == _graph) td.HighlightedNodes.Add(i); td.CallDeferred("update"); }));
        }
        void MouseExited()
        {
            _tilemap.tiledisplays.ForEach(td =>
                RepeatN(td.RenderedTile.Nodes.Count, i =>
                    { if (td.RenderedTile.Nodes[i].Graph == _graph) td.HighlightedNode = -1; td.CallDeferred("update"); }));
        }
        public GraphLabel(TileMap _tilemap, Map.Graph _graph, bool iscontested)
        {
            this._tilemap = _tilemap;
            this._graph = _graph;
            Text = $"ID: {_graph.ID}\nType: {_graph.Type}\nTiles: {_graph.Tiles.Count}\nIsContested: {iscontested}\n";
            this.Connect("mouse_entered", this, "MouseEntered");
            this.Connect("mouse_exited", this, "MouseExited");
            this.MouseFilter = MouseFilterEnum.Stop;
            this.SetProcessInput(true);
        }
    }
    class PlacePawnButton : Button
    {
        GameEngine _game;
        object o;
        int _indx;
        PlacedTile _placedTile => _gui._tileMap.tiledisplays.Find(it => it.RenderedTile == ((InternalNode)o).ParentTile);
        Test2D_GUI _gui;
        void MouseEntered()
        {
            if (o is InternalNode)
            {
                _placedTile.HighlightedNode = _indx;
                _placedTile.CallDeferred("update");
            }
        }
        void MouseExited()
        {
            if (o is InternalNode)
            {
                _placedTile.HighlightedNode = -1;
                _placedTile.CallDeferred("update");
            }
        }
        public override void _Pressed()
        {
            SetProcessInput(false);
            if (_indx == -1)
            {
                _game.SkipPlacingPawn();
            }
            else if (o is InternalNode n)
            {
                _placedTile.HighlightedNode = -1;
                _game.PlacePawnOnNode(_indx);
            }
            else if (o is Tile.TileAttribute a)
            {
                _game.PlacePawnOnAttribute(_indx);
            }
            _gui._tileMap.UpdateDisplay();
            _gui.UpdateInterface();
        }
        public PlacePawnButton(Test2D_GUI gui, GameEngine game, object o, int indx, int hotkey)
        {
            Assert(gui != null);
            Assert(o != null || indx == -1);

            this._gui = gui;
            this._game = game;
            this.o = o;
            this._indx = indx;

            if (indx == -1)
            {
                this.Text = $"{hotkey}. SKIP";
            }
            else
            {
                if (o is InternalNode node)
                {
                    this.Connect("mouse_entered", this, "MouseEntered");
                    this.Connect("mouse_exited", this, "MouseExited");
                    this.Text = $"{hotkey}. {node.Type} ({node.ParentTile.Nodes.IndexOf(node)})";
                }
                else if (o is Tile.TileAttribute attr)
                {
                    this.Text = $"{hotkey}. {attr.Type}";
                }
            }
        }
    }
    public void UndoPressed()
    {

    }
    public void RedoPressed()
    {

    }
    public void UpdateInterface()
    {
        _currenttile.Visible = _game.CurrentState == GameEngine.State.PLACE_TILE;
        if (_game.CurrentTile != _currenttile.RenderedTile)
        {
            _currenttile.RenderedTile = _game.CurrentTile;
            _currenttile.CallDeferred("update");
        }
        foreach (Node n in _mainButtonContainer.GetChildren())
        {
            n.GetParent().RemoveChild(n);
            n.QueueFree();
        }
        _pawnPlaceButtons.Clear();
        if (_game.CurrentState == GameEngine.State.PLACE_PAWN)
        {
            int i = 1;
            Tile tile = _game.map[_game.CurrentTile.Position];
            PlacePawnButton button;
            _mainButtonContainer.AddChild(button = new PlacePawnButton(this, _game, null, -1, i++));
            _pawnPlaceButtons.Add(button);
            foreach (var it in _game.PossibleMeepleAttributePlacements())
            {
                button = new PlacePawnButton(this, _game, tile.Attributes[it], it, i);
                _mainButtonContainer.AddChild(button);
                _pawnPlaceButtons.Add(button);
                i++;
            }
            foreach (var it in _game.PossibleMeepleNodePlacements())
            {
                InternalNode node = tile.Nodes[it];
                button = new PlacePawnButton(this, _game, node, it, i);
                _mainButtonContainer.AddChild(button);
                _pawnPlaceButtons.Add(button);
                i++;
            }
        }

        {
            _hashLabel.Text = _game.GetHashBase16();
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
                _tileMap.tiledisplays.ForEach(td => td.HighlightedNode = -1);
                foreach ((Map.Graph graph, bool iscontested) in _game.GetGraphsOwnedBy(it))
                {
                    var l = new GraphLabel(_tileMap, graph, iscontested);
                    _playerDataContainers[i].AddChild(l);
                }
                i++;
            }
        }
    }
    void SavePressed()
    {
        File f = new File();
        Assert(f.Open("res://Test/Saves/save.carcassonne", File.ModeFlags.Write));
        var dt = _game.Serialize();
        f.StoreBuffer(dt);
        f.Close();
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

        _mainButtonContainer = _cl.GetNode<VBoxContainer>("HBoxContainer/VBoxContainer2/MainButtonContainer");
        _playerTabContainer = _cl.GetNode<TabContainer>("HBoxContainer/VBoxContainer2/PlayerTabs");

        _saveButton = GetNode<Button>("CanvasLayer/HBoxContainer/VBoxContainer/HBoxContainer/SaveButton");
        _saveButton.Connect("pressed", this, "SavePressed");
        _undoButton = GetNode<Button>("CanvasLayer/HBoxContainer/VBoxContainer/HBoxContainer/UndoButton");
        _undoButton.Connect("pressed", this, "UndoPressed");
        _redoButton = GetNode<Button>("CanvasLayer/HBoxContainer/VBoxContainer/HBoxContainer/RedoButton");
        _redoButton.Connect("pressed", this, "RedoPressed");
        _hashLabel = GetNode<Label>("CanvasLayer/HashLabel");

        if (_tileMap.game == null)
        {
            if (Test2D_MM.GlobalGame == null)
                _tileMap.SetGame(GameEngine.CreateBaseGame(new GameExternalDataLoader(), 666, 5, "BaseGame/BaseTileset.json"));
            else
                _tileMap.SetGame(Test2D_MM.GlobalGame);
        }

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
            case InputEventKey ev:
                {
                    uint codestart = (uint)KeyList.Key1;
                    uint codesend = (uint)KeyList.Key9;
                    if (ev.Scancode >= codestart && ev.Scancode <= codesend)
                    {
                        uint v = ev.Scancode - codestart;
                        if (_game.CurrentState == GameEngine.State.PLACE_PAWN)
                        {
                            if (v < _pawnPlaceButtons.Count)
                            {
                                _pawnPlaceButtons[(int)v]._Pressed();
                            }
                        }
                    }
                    break;
                }
        }
        _test2D._Input(@event);
    }
}
