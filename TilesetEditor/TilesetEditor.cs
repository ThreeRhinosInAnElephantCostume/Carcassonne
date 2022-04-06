// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
using Newtonsoft.Json;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;


public class TilesetEditor : Control
{
    // Main state
    string _currentPath = "";
    public string CurrentPath { get => _currentPath; set { _currentPath = value; SetPath(value); } }
    public EditableTileset Tileset { get; protected set; }
    PlacedTile _placedTile;
    TilePrototype _tilePrototype;
    bool _loaded = false;
    // Data
    enum ListMode
    {
        NOTHING,
        POSSIBLE,
        CURRENT
    }
    ListMode _listMode = ListMode.NOTHING;
    int _listIndex = 0;
    readonly List<string> _possibleTilePaths = new List<string>();
    readonly List<(string path, int n, bool isstarter)> _currentTileData = new List<(string, int, bool)>();
    // Controls
    GroupedFileManager _fileManager;
    Container _mainContainer;
    LineEdit _possibleSearchEdit;
    ItemList _possibleTileList;
    Container _listEditButtonContainer;
    Button _addButton;
    Button _add5Button;
    Button _removeButton;
    Button _remove5Button;
    Button _addStarterButton;
    LineEdit _currentSearchEdit;
    ItemList _currentTileList;
    Button _saveButton;
    Button _resetButton;
    Button _reloadButton;
    CheckBox _partialOutputCheck;
    LineEdit _partialOutputNEdit;
    Label _nTilesLabel;
    Label _nStarterLabel;
    Label _statusLabel;
    Node2D _root2D;
    Camera2D _camera;
    ItemList _nodeList;
    ItemList _attributeList;

    // Interface callbacks

    void PossibleSearchChanged(string text)
    {
        UnloadTile();
        LoadPossibleList(text);
        UpdateInterface();
    }
    void PossibleListSelected(int indx)
    {
        if (_currentTileList.IsAnythingSelected())
            _currentTileList.Unselect(_currentTileList.GetSelectedItems()[0]);
        _listMode = ListMode.POSSIBLE;
        _listIndex = indx;
        LoadTile(_possibleTilePaths[indx]);
        UpdateInterface();
    }
    void PossibleListDeselected()
    {
        _listMode = ListMode.NOTHING;
        _listIndex = -1;
        UpdateInterface();
    }
    void PossibleListActivated(int indx)
    {
        PossibleListSelected(indx);
        AddButtonPressed();
    }
    void CurrentSearchChanged(string text)
    {
        UnloadTile();
        LoadCurrentList(text);
        UpdateInterface();
    }
    void CurrentListSelected(int indx)
    {
        if (_possibleTileList.IsAnythingSelected())
            _possibleTileList.Unselect(_possibleTileList.GetSelectedItems()[0]);
        _listMode = ListMode.CURRENT;
        _listIndex = indx;
        LoadTile(_currentTileData[indx].path);
        UpdateInterface();
    }
    void CurrentListDeselected()
    {
        _listMode = ListMode.NOTHING;
        _listIndex = -1;
        UpdateInterface();
    }
    void CurrentListActivated(int indx)
    {
        CurrentListSelected(indx);
        RemoveButtonPressed();
    }
    void AddButtonPressed()
    {
        Assert(_listMode != ListMode.NOTHING);

        var p = GetSelectedTile();
        AddTileToList(p, 1, false);
        Tileset.Tiles.Add(p);
        UpdateInterface();
    }
    void Add5ButtonPressed()
    {
        Assert(_listMode != ListMode.NOTHING);

        var p = GetSelectedTile();
        for (int i = 0; i < 5; i++)
            Tileset.Tiles.Add(p);
        AddTileToList(p, 5, false);
        UpdateInterface();
    }
    void RemoveButtonPressed()
    {
        Assert(_listMode == ListMode.CURRENT);

        RemoveNTiles(1);
        UpdateInterface();
    }
    void Remove5ButtonPressed()
    {
        Assert(_listMode == ListMode.CURRENT);

        RemoveNTiles(5);
        UpdateInterface();
    }
    void AddStarterButtonPressed()
    {
        Assert(_listMode != ListMode.NOTHING);

        var p = GetSelectedTile();
        AddTileToList(p, 1, true);
        Tileset.StarterTiles.Add(p);
        UpdateInterface();
    }
    void SaveButtonPressed()
    {
        Assert(Tileset != null && CurrentPath != "");

        Tileset.NPossibleTiles = Tileset.Tiles.Count;
        Tileset.NOutputTiles = Tileset.Tiles.Count; // TODO: support partial outputs
        Tileset.SingleStarter = true; // TODO: support multiple starters
        Tileset.NStarterTiles = Tileset.NStarterTiles;

        SerializeToFile(CurrentPath, Tileset);
    }
    void ResetButtonPressed()
    {
        Tileset.Tiles.Clear();
        Tileset.StarterTiles.Clear();
        UnloadTile();
        LoadInterface();
    }
    void ReloadButonPressed()
    {
        string path = CurrentPath;
        SetPath("");
        SetPath(path);
    }
    void PartialOutputToggled(bool b)
    {

    }
    void PartialOutputNChanged(string text)
    {

    }
    void NodeListSelected(int indx)
    {
        Assert(_tilePrototype != null);
        Assert(_placedTile != null);

        _attributeList.Clear();
        _placedTile.HighlightedNode = -1;
        _placedTile.CallDeferred("update");
        if (indx == 0)
        {
            if (_tilePrototype.TileAttributes.Count == 0)
                _attributeList.AddItem("<<NO ATTRIBUTES>>");
            else
            {
                foreach (var it in _tilePrototype.TileAttributes)
                {
                    _attributeList.AddItem(((TileAttributeType)it).ToString());
                }
            }
            return;
        }
        indx -= 1;
        if (indx >= _tilePrototype.NodeAttributes.Count)
            return;
        _tilePrototype.NodeAttributes[indx].ForEach(it => _attributeList.AddItem(((NodeAttributeType)it).ToString()));
        _placedTile.HighlightedNode = indx;
    }
    void AttributeListSelected(int indx)
    {

    }

    // Internal logic
    string CreateTileText(string path, int n, bool starter)
    {
        string visstr = path.Replace(Constants.DataPaths.TILE_DIRECTORY, "");
        if (visstr[0] == '/')
            visstr = visstr.Substring(1);
        return ($"{visstr} " + ((starter) ? ("STARTER") : "") + ((n > 1) ? ("(" + n.ToString() + ")") : ""));
    }
    void RemoveNTiles(int n)
    {

        Assert(_listMode == ListMode.CURRENT);

        var p = GetSelectedTile();
        var isstarter = IsStarterSelected();
        var indx = _currentTileData.FindIndex(it => it.isstarter == isstarter && it.path == p);

        Assert(indx != -1);


        var dt = _currentTileData[indx];
        int trmv = Min(dt.n, n);
        Assert(trmv > 0);

        for (int i = 0; i < trmv; i++)
        {
            if (isstarter)
                Tileset.StarterTiles.Remove(p);
            else
                Tileset.Tiles.Remove(p);
        }

        RemoveTileFromList(p, trmv, isstarter);
    }
    void AddTileToList(string path, int n, bool starter)
    {
        int i = 0;
        foreach (var it in _currentTileData)
        {
            if (it.path == path && it.isstarter == starter)
            {
                (string path, int n, bool isstarter) nit = (it.path, it.n + n, it.isstarter);
                _currentTileData[i] = nit;
                _currentTileList.SetItemText(i, CreateTileText(nit.path, nit.n, nit.isstarter));
                return;
            }
            i++;
        }
        _currentTileData.Add((path, n, starter));
        _currentTileList.AddItem(CreateTileText(path, n, starter));
    }
    void RemoveTileFromList(string path, int n, bool starter)
    {
        int i = 0;
        foreach (var it in _currentTileData)
        {
            if (it.path == path && it.isstarter == starter)
            {
                if (n >= it.n)
                {
                    _currentTileData.RemoveAt(i);
                    _currentTileList.RemoveItem(i);
                }
                else
                {
                    var nit = (it.path, it.n - n, it.isstarter);
                    _currentTileData[i] = nit;
                    _currentTileList.SetItemText(i, CreateTileText(nit.path, it.n - n, it.isstarter));
                }
                return;
            }
            i++;
        }
        throw new Exception("Attempted to remove a tile that does not exist!");
    }
    bool IsStarterSelected()
    {
        Assert(_listMode == ListMode.CURRENT);
        return _currentTileData[_listIndex].isstarter;
    }
    string GetSelectedTile()
    {
        Assert(_listMode != ListMode.NOTHING);

        if (_listMode == ListMode.CURRENT)
        {
            return _currentTileData[_listIndex].path;
        }
        else if (_listMode == ListMode.POSSIBLE)
        {
            return _possibleTilePaths[_listIndex];
        }
        throw new Exception();
    }
    void LoadPossibleList(string filter)
    {
        filter = filter.ToLower();
        _possibleTilePaths.Clear();
        _possibleTileList.Clear();
        var files = ListDirectoryFilesRecursively(Constants.DataPaths.TILE_DIRECTORY, s => s.EndsWith(".json"));
        files.Sort();
        foreach (var it in files)
        {
            string visstr = it.Replace(Constants.DataPaths.TILE_DIRECTORY, "");
            if (visstr[0] == '/')
                visstr = visstr.Substring(1);
            if (filter != "" && !visstr.ToLower().Contains(filter))
            {
                continue;
            }

            var p = TileDataLoader.LoadTilePrototype(it);

            if (p == null || !p.IsValid)
                continue;

            _possibleTilePaths.Add(it);
            _possibleTileList.AddItem(visstr);
        }
    }
    void RemoveInvalidTiles()
    {
        Assert(Tileset != null);
        foreach (var it in Tileset.Tiles.ToList())
        {
            if (!FileExists(it))
            {
                if (Tileset.Tiles.Contains(it))
                {
                    GD.PrintErr("Removed missing tile: " + it);
                    Tileset.Tiles.Remove(it);
                }
            }
        }
    }
    void LoadCurrentList(string filter)
    {
        filter = filter.ToLower();
        _currentTileList.Clear();
        _currentTileData.Clear();
        Dictionary<string, int> dt = new Dictionary<string, int>();
        foreach (var it in Tileset.Tiles)
        {
            if (dt.ContainsKey(it))
                dt[it] = dt[it] + 1;
            else
                dt.Add(it, 1);
        }
        var kvs = dt.ToList();
        kvs.Sort((kv0, kv1) => kv0.Key.CompareTo(kv1.Key));
        foreach (var it in kvs)
        {
            string visstr = it.Key;
            if (visstr[0] == '/')
                visstr = visstr.Substring(1);
            if (filter != "" && !visstr.ToLower().Contains(filter))
            {
                continue;
            }
            AddTileToList(it.Key, it.Value, false);
        }
        foreach (var it in Tileset.StarterTiles)
        {
            string visstr = it;
            if (visstr[0] == '/')
                visstr = visstr.Substring(1);
            if (filter != "" && !visstr.ToLower().Contains(filter))
            {
                continue;
            }

            AddTileToList(it, 1, true);
        }

    }
    void LoadNodeList()
    {
        Assert(_placedTile != null);
        _nodeList.Clear();
        _nodeList.AddItem("<<TILE>>");
        int i = 0;
        foreach (var it in _tilePrototype.NodeTypes)
        {
            _nodeList.AddItem($"{(NodeType)it} ({i})");
            i++;
        }
    }
    void LoadInterface()
    {
        if (!_loaded)
        {
            foreach (var it in GetChildrenRecrusively<Control>(_mainContainer))
            {
                if (it is Button b)
                {
                    b.Disabled = true;
                }
                else if (it is LineEdit e)
                {
                    e.Editable = false;
                    e.Text = "";
                }
                else if (it is ItemList l)
                {
                    l.Clear();
                }
            }
            _listMode = ListMode.NOTHING;
            return;
        }
        foreach (var it in GetChildrenRecrusively<Control>(_mainContainer))
        {
            if (it is Button b)
            {
                b.Disabled = false;
            }
            else if (it is LineEdit e)
            {
                e.Editable = true;
            }
        }

        LoadPossibleList(_possibleSearchEdit.Text);
        LoadCurrentList(_currentSearchEdit.Text);

        if (_tilePrototype != null && _nodeList.GetItemCount() == 0)
        {
            LoadNodeList();
        }
        UpdateInterface();
    }
    void UpdateInterface()
    {
        _partialOutputCheck.Pressed = false;
        _partialOutputCheck.Disabled = true;
        _partialOutputNEdit.Editable = _partialOutputCheck.Pressed;

        bool cantadd = !_loaded || !(_listMode == ListMode.CURRENT || _listMode == ListMode.POSSIBLE);
        bool cantdel = !_loaded || !(_listMode == ListMode.CURRENT);

        _addButton.Disabled = cantadd;
        _add5Button.Disabled = cantadd;
        _removeButton.Disabled = cantdel;
        _remove5Button.Disabled = cantdel;
        _addStarterButton.Disabled = cantadd || Tileset.StarterTiles.Count > 0; // TODO: Support multiple starter tiles

        if (!_loaded)
        {
            _nTilesLabel.Text = "NTiles: 0";
            _nStarterLabel.Text = "NStarter: 0";
            _statusLabel.Text = "NO TILESET LOADED";
        }
        else
        {
            _nTilesLabel.Text = $"NTiles: {Tileset.Tiles.Count}";
            _nStarterLabel.Text = $"NStarters: {Tileset.StarterTiles.Count}";
            _statusLabel.Text = (Tileset.StarterTiles.Count == 0) ? "NO STARTER SELECTED" : "OK";
        }

    }
    void LoadTile(string path)
    {
        if (_placedTile != null)
            UnloadTile();
        if (path == "")
            return;
        Assert(FileExists(path));

        _tilePrototype = TileDataLoader.LoadTilePrototype(path);

        _placedTile = new PlacedTile();
        _placedTile.RenderedTile = _tilePrototype.Convert();
        var vsize = _camera.GetViewport().Size;

        _placedTile.size = Min(vsize.x, vsize.y);
        _placedTile.outersize = new Vector2(_placedTile.size, _placedTile.size);
        _placedTile.Position = vsize / 2;

        _root2D.AddChild(_placedTile);
        LoadInterface();
    }
    void UnloadTile()
    {
        if (_placedTile != null)
        {
            _placedTile.GetParent().RemoveChild(_placedTile);
            _placedTile.QueueFree();
            _placedTile = null;
        }
        _nodeList.Clear();
        _attributeList.Clear();
    }
    void Load()
    {
        if (_loaded)
            return;
        _loaded = true;
        if (FileExists(CurrentPath))
        {
            Tileset = (EditableTileset)TileDataLoader.LoadTileset(CurrentPath);
        }
        else
        {
            Tileset = new EditableTileset(false);
        }
        RemoveInvalidTiles();
        LoadInterface();
    }
    void Unload()
    {
        if (!_loaded)
            return;
        _loaded = false;
        LoadInterface();
    }

    void SetPath(string path)
    {
        this._currentPath = path;
        Unload();
        if (path != "")
            Load();
    }


    public override void _Ready()
    {
        {
            _fileManager = GetNode<GroupedFileManager>("HBoxContainer/FileSelector");
            _fileManager.FilterHandle = (string path) =>
            {
                Assert(FileExists(path));
                if (!path.EndsWith(".json"))
                    return false;

                return DeserializeFromFile<EditableTileset>(path, true) != null;
            };
            _fileManager.FileCloseHandle = (string s) =>
            {
                this.CurrentPath = "";
            };
            _fileManager.FileOpenHandle = (string s) =>
            {
                this.CurrentPath = s;
            };
            _fileManager.CreateFileHandle = (string s) =>
            {
                Assert(!FileExists(s));
                SerializeToFile<EditableTileset>(s, new EditableTileset(true));
            };
            _fileManager.Extension = ".json";
            _fileManager.Path = Constants.DataPaths.TILESET_DIRECTORY;
        }

        _mainContainer = GetNode<Container>("HBoxContainer/MainContainer");

        {
            _possibleSearchEdit = GetNode<LineEdit>("HBoxContainer/MainContainer/VBoxContainer/PotentialSearchBar");
            _possibleSearchEdit.Connect("text_changed", this, "PossibleSearchChanged");
            _possibleTileList = GetNode<ItemList>("HBoxContainer/MainContainer/VBoxContainer/PotentialTiles");
            _possibleTileList.Connect("item_selected", this, "PossibleListSelected");
            _possibleTileList.Connect("nothing_selected", this, "PossibleListDeselected");
            _possibleTileList.Connect("item_activated", this, "PossibleListActivated");

        }
        {

            _listEditButtonContainer = GetNode<Container>("HBoxContainer/MainContainer/VBoxContainer3");
            _addButton = GetNode<Button>("HBoxContainer/MainContainer/VBoxContainer3/AddButton");
            _addButton.Connect("pressed", this, "AddButtonPressed");
            _add5Button = GetNode<Button>("HBoxContainer/MainContainer/VBoxContainer3/Add5Button");
            _add5Button.Connect("pressed", this, "Add5ButtonPressed");
            _removeButton = GetNode<Button>("HBoxContainer/MainContainer/VBoxContainer3/RemoveButton");
            _removeButton.Connect("pressed", this, "RemoveButtonPressed");
            _remove5Button = GetNode<Button>("HBoxContainer/MainContainer/VBoxContainer3/Remove5Button");
            _remove5Button.Connect("pressed", this, "Remove5ButtonPressed");
            _addStarterButton = GetNode<Button>("HBoxContainer/MainContainer/VBoxContainer3/AddStarter");
            _addStarterButton.Connect("pressed", this, "AddStarterButtonPressed");
        }
        {
            _currentSearchEdit = GetNode<LineEdit>("HBoxContainer/MainContainer/VBoxContainer2/CurrentSearchBar");
            _currentSearchEdit.Connect("text_changed", this, "CurrentSearchChanged");
            _currentTileList = GetNode<ItemList>("HBoxContainer/MainContainer/VBoxContainer2/CurrentTiles");
            _currentTileList.Connect("item_selected", this, "CurrentListSelected");
            _currentTileList.Connect("nothing_selected", this, "CurrentListDeselected");
            _currentTileList.Connect("item_activated", this, "CurrentListActivated");
        }

        {
            _saveButton = GetNode<Button>("HBoxContainer/MainContainer/VBoxContainer2/VBoxContainer/HBoxContainer4/SaveButton");
            _saveButton.Connect("pressed", this, "SaveButtonPressed");
            _resetButton = GetNode<Button>("HBoxContainer/MainContainer/VBoxContainer2/VBoxContainer/HBoxContainer4/ResetButton");
            _resetButton.Connect("pressed", this, "ResetButtonPressed");
            _reloadButton = GetNode<Button>("HBoxContainer/MainContainer/VBoxContainer2/VBoxContainer/HBoxContainer4/ReloadButton");
            _reloadButton.Connect("pressed", this, "ReloadButtonPressed");
        }
        {
            _partialOutputCheck = GetNode<CheckBox>("HBoxContainer/MainContainer/VBoxContainer2/VBoxContainer/HBoxContainer3/PartialOutputCheck");
            _partialOutputCheck.Connect("toggled", this, "PartialOutputToggled");
            _partialOutputNEdit = GetNode<LineEdit>("HBoxContainer/MainContainer/VBoxContainer2/VBoxContainer/HBoxContainer3/NOutputTilesEdit");
            _partialOutputCheck.Connect("text_changed", this, "PartialOutputNChanged");
        }
        _nTilesLabel = GetNode<Label>("HBoxContainer/MainContainer/VBoxContainer2/VBoxContainer/HBoxContainer2/NTilesLabel");
        _nStarterLabel = GetNode<Label>("HBoxContainer/MainContainer/VBoxContainer2/VBoxContainer/HBoxContainer2/NStarterLabel");
        _statusLabel = GetNode<Label>("HBoxContainer/MainContainer/VBoxContainer2/VBoxContainer/StatusLabel");
        _root2D = GetNode<Node2D>("HBoxContainer/MainContainer/VisualisationBox/ViewportContainer/Viewport/Root2D");
        _camera = GetNode<Camera2D>("HBoxContainer/MainContainer/VisualisationBox/ViewportContainer/Viewport/Camera2D");
        {
            _nodeList = GetNode<ItemList>("HBoxContainer/MainContainer/VisualisationBox/HBoxContainer/NodeList");
            _nodeList.Connect("item_selected", this, "NodeListSelected");
            _attributeList = GetNode<ItemList>("HBoxContainer/MainContainer/VisualisationBox/HBoxContainer/AttributeList");
            _attributeList.Connect("item_selected", this, "AttributeListSelected");
        }
        LoadInterface();
    }
    public override void _Process(float delta)
    {

    }
}
