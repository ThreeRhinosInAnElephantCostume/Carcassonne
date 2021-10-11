using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Threading;
using Carcassonne;
using ExtraMath;
using Godot;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

//[Tool]
public class TileLogicEditor : Control
{
    string _path;
    public string Path
    {
        get => _path;
        set
        {
            _path = value;
            SetPath(_path);
        }
    }
    TilePrototype _tile;
    Camera2D camera;
    Node2D root;
    bool loaded = false;
    List<Button> _connectorButtons = new List<Button>();
    Node2D nd;

    Button _saveButton;
    Button _resetButton;
    Button _fillButton;
    Container _toolboxContainer;
    void UnloadView()
    {
        if (!loaded)
            return;
        loaded = false;
        _connectorButtons.ForEach(b => b.QueueFree());
        nd.GetParent().RemoveChild(nd);
        nd.QueueFree();
    }
    void LoadView()
    {
        if (loaded)
            return;
        loaded = true;
        nd = new Node2D();
        root.AddChild(nd);
        Vector2[] Corners = new Vector2[]
        {
            new Vector2(-1, -1),
            new Vector2(1, -1),
            new Vector2(1, 1),
            new Vector2(-1, 1),
        };
        float sz = 100f;
        float third = (2f * sz) / 3f;
        for (int i = 0; i < GameEngine.N_SIDES; i++)
        {
            for (int ii = 0; ii < GameEngine.N_CONNECTORS; ii++)
            {
                Button b = new Button();
                nd.AddChild(b);
                b.RectPosition = Corners[i] * sz + ii * (third * Corners[AbsMod(i - 1, GameEngine.N_SIDES)]);
                b.RectSize = new Vector2(1, 1) * third;
                b.RectRotation = (90.0f * (i - 1));

                _connectorButtons.Add(b);
            }
        }
    }
    void SetTile(TilePrototype tile)
    {
        UnloadView();
        this._tile = tile;
        if (_tile == null)
            return;
        LoadView();
        int tc = GameEngine.N_CONNECTORS * GameEngine.N_SIDES;
        if (tile.Assignments == null || tile.Assignments.Length == 0)
        {
            tile.NodeTypes = new int[1];
            tile.Assignments = new int[tc];
            for (int i = 0; i < tc; i++)
            {
                tile.Assignments[i] = 0;
            }
            tile.NodeTypes[0] = (int)(NodeType.ERR);
        }


        UpdateTileDisplay();

    }
    void UpdateTileDisplay()
    {
        if (!loaded)
            return;

        int tc = GameEngine.N_CONNECTORS * GameEngine.N_SIDES;

        Assert(_tile.NodeTypes != null && _tile.NodeTypes.Length > 0);
        Assert(_tile.Assignments.Length == tc);
        Assert(_connectorButtons.Count == tc);

        for (int i = 0; i < tc; i++)
        {
            _connectorButtons[i].Text = _tile.NodeTypes[_tile.Assignments[i]].ToString();
        }
    }
    void SetPath(string path)
    {
        _path = path;
        if (path == "")
            SetTile(null);
        else
        {
            Assert(ResourceLoader.Exists(path));
            Resource resource = ResourceLoader.Load(path);
            Assert(resource != null);
            Assert((resource as TilePrototype) != null);
            SetTile((TilePrototype)resource);
        }
        UpdateTileDisplay();
    }
    public override void _Ready()
    {
        this.camera = (Camera2D)FindChild<ViewportContainer>(this).GetNode("Viewport/Camera2D");
        this.root = (Node2D)camera.GetNode("TileDisplayRoot");

        this._toolboxContainer = (Container)GetNode("ToolsContainer");
        this._saveButton = (Button)GetNode("ToolsContainer/SaveButton");
        this._fillButton = (Button)GetNode("ToolsContainer/FillButton");
        this._resetButton = (Button)GetNode("ToolsContainer/ResetButton");


    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
