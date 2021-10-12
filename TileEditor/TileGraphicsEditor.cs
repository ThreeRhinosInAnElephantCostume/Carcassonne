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
using Newtonsoft.Json;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;


//[Tool]
public class TileGraphicsEditor : VBoxContainer
{

    Container _toolboxContainer;
    Button _saveButton;
    Button _resetButton;
    Button _reloadButton;
    OptionButton _modelSelector;
    Button _addModelButton;
    TilePrototype _tile;
    bool _loaded = false;
    public TilePrototype Tile { get => _tile; set => SetTile(value); }
    public string Path { get; set; }

    TileGraphicsConfig _tileGraphicsConfig;
    TileGraphicsConfig.Config _config;
    Spatial _modelRoot;
    Spatial _3DRoot;
    Camera _camera3D;
    Node2D _2DRoot;
    Camera2D _camera2D;
    PlacedTile _placedTile;
    string _modelPath = "";
    void SavePressed()
    {

    }
    void ResetPressed()
    {

    }
    void ReloadPressed()
    {

    }
    void ModelSelected(int indx)
    {
        SetModel(Tile.AssociatedModels[indx]);
    }
    void AddModelPressed()
    {
        FastSearchFilesPopup pop = (FastSearchFilesPopup)GD.Load<PackedScene>("res://TileEditor/FastSearchFilesPopup.tscn").Instance();

        Assert(pop != null);

        pop.FilterHandle = s => s.EndsWith(".tscn") && !Tile.AssociatedModels.Contains(s);
        pop.CompleteHandle = (string s, bool comp) =>
        {
            if (comp)
            {
                if (!Tile.AssociatedModels.Contains(s))
                    Tile.AssociatedModels.Add(s);
            }
            UpdateOptions();
            if (comp && _modelPath != s)
            {
                SetModel(s);
            }
        };

        pop.Path = Constants.TILE_MODEL_DIRECTORY;

        AddChild(pop);

        pop.Popup_();

        SetInterfaceActive(false);
    }
    void SetModel(string path)
    {
        Assert(path != null);
        Assert(Tile != null);

        _modelPath = path;

        UnloadDisplays();
        if (path == "")
        {
            SetInterfaceActive(false);
            return;
        }
        Directory dr = new Directory();
        Assert(dr.FileExists(path));
        if (dr.FileExists(path.Replace(".tscn", ".json")))
        {
            var f = new File();
            Assert(f.Open(path, File.ModeFlags.Write));
            _tileGraphicsConfig = JsonConvert.DeserializeObject<TileGraphicsConfig>(f.GetAsText());
            f.Close();
            Assert(_tileGraphicsConfig != null && _tileGraphicsConfig.Configs != null);
            if (!_tileGraphicsConfig.Configs.ContainsKey(Path))
                _tileGraphicsConfig.Configs.Add(Path, _config = new TileGraphicsConfig.Config());
            _tileGraphicsConfig.Configs[Path] = _config;
        }
        else
        {
            _tileGraphicsConfig = new TileGraphicsConfig();
            _config = new TileGraphicsConfig.Config();
            _tileGraphicsConfig.Configs.Add(Path, _config);
        }


        LoadDisplays();
    }
    void UpdateOptions()
    {
        Assert(Tile != null);
        _modelSelector.Clear();
        foreach (var it in Tile.AssociatedModels)
        {
            _modelSelector.AddItem(it.Split("/").Last());
        }
        if (Tile.AssociatedModels.Count > 0 && _modelRoot == null)
            _modelSelector.Select(0);
    }
    void MaybeLoad()
    {
        if (Tile != null && _modelRoot != null)
        {
            LoadDisplays();
            UpdateDisplays();
        }
        SetInterfaceActive(false);
        if (Tile != null)
        {
            _addModelButton.Disabled = false;
            _modelSelector.Disabled = false;
        }
    }
    public void SetTile(TilePrototype tile)
    {
        if (_tile == tile)
        {
            if (_tile != null && _modelRoot != null)
                UpdateDisplays();
            return;
        }
        _tile = tile;
        if (Tile == null)
        {
            UnloadDisplays();
            return;
        }
        UnloadDisplays();
        MaybeLoad();
    }
    void LoadDisplays()
    {
        if (_loaded)
            return;
        _loaded = true;

        Assert(_modelPath != "");
        Assert(_modelPath.Contains(".tscn"));

        _modelRoot = (Spatial)GD.Load<PackedScene>(_modelPath).Instance();

        Assert(_modelRoot != null);

        _3DRoot.AddChild(_modelRoot);

        _placedTile = new PlacedTile();
        _placedTile.tile = _tile.Convert();
        _placedTile.size = 300.0f;
        _placedTile.outersize = new Vector2(_placedTile.size, _placedTile.size);

        _2DRoot.AddChild(_placedTile);

        _placedTile.Position = _camera2D.GetViewport().Size / 2;

        _placedTile.Update();
    }
    void UnloadDisplays()
    {
        if (!_loaded)
            return;
        _loaded = false;
    }
    void UpdateDisplays()
    {
        CallDeferred("ActuallyUpdateDisplays");
    }
    void ActuallyUpdateDisplays()
    {
        _placedTile.tile = _tile.Convert();
        _placedTile.Update();

    }
    void SetInterfaceActive(bool b)
    {
        foreach (var it in _toolboxContainer.GetChildren())
        {
            Button button = it as Button;
            if (button == null)
                continue;
            button.Disabled = !b;
        }
    }

    public override void _Ready()
    {
        _toolboxContainer = (Container)GetNode("ToolsContainer");

        _saveButton = (Button)GetNode("ToolsContainer/SaveButton");
        _resetButton = (Button)GetNode("ToolsContainer/ResetButton");
        _reloadButton = (Button)GetNode("ToolsContainer/ReloadButton");
        _modelSelector = (OptionButton)GetNode("ToolsContainer/OptionButton");
        _addModelButton = (Button)GetNode("ToolsContainer/AddAssociationsButton");

        _saveButton.Connect("pressed", this, "SavePressed");
        _resetButton.Connect("pressed", this, "ResetPressed");
        _reloadButton.Connect("pressed", this, "ReloadPressed");
        _modelSelector.Connect("item_selected", this, "ModelSelected");
        _addModelButton.Connect("pressed", this, "AddModelPressed");

        _2DRoot = (Node2D)GetNode("HBoxContainer/VBoxContainer/2DViewport/Viewport/2DRoot");
        _3DRoot = (Spatial)GetNode("HBoxContainer/VBoxContainer/3DViewport/Viewport/3DRoot");
        _camera2D = (Camera2D)GetNode("HBoxContainer/VBoxContainer/2DViewport/Viewport/2DRoot/Camera2D");
        _camera3D = (Camera)GetNode("HBoxContainer/VBoxContainer/3DViewport/Viewport/3DRoot/Camera3D");

        SetInterfaceActive(false);
    }
}
