using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
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
using Vector2 = Godot.Vector2;
using Vector3 = Godot.Vector3;


//[Tool]
public class TileGraphicsEditor : VBoxContainer
{

    Container _toolboxContainer;
    Button _saveButton;
    Button _resetButton;
    Button _reloadButton;
    Button _autoAssignButton;
    OptionButton _modelSelector;
    Button _addModelButton;
    Button _delModelButton;
    VBoxContainer _assignmentContainer;
    Label _statusLabel;
    VBoxContainer _toggleNodeContainer;
    Button _rotateTileButton;
    ItemList _assignableList;
    Slider _logicOpacitySlider;
    TilePrototype _tile;
    bool _loaded = false;
    string _path { get; set; }

    TileGraphicsConfig _tileGraphicsConfig;
    TileGraphicsConfig.Config _config;
    Spatial _modelRoot;
    Spatial _3DRoot;
    OrbitCamera _camera3D;
    Node2D _2DRoot;
    Camera2D _camera2D;
    Viewport _viewport2D;
    PlacedTile _placedTile;
    Sprite3D _tileLogicOverlay;
    string _modelPath = "";
    List<string> _modelGroups = new List<string>();
    Dictionary<string, bool> _groupAssigned = new System.Collections.Generic.Dictionary<string, bool>();
    Dictionary<string, Vector3> _groupAveragePosition = new Dictionary<string, Vector3>();

    int _graphicsRotation = 0;
    void AssignableSelected(int indx)
    {
        if (!_loaded)
            return;
        AssignableDeselected();
        if (indx == 0)
        {
            _modelGroups.FindAll(s => !_groupAssigned[s]).ForEach(s => HighlightGroup(s, true));
            return;
        }
        if (indx == 1)
        {
            _modelGroups.FindAll(s => _config.Unassociated.Contains(s)).ForEach(s => HighlightGroup(s, true));
            return;
        }
        indx -= 2;
        if(indx < _tile.TileAttributes.Count)
        {
            _modelGroups.FindAll(s => _config.AttributeAssociations.ContainsKey(s) && _config.AttributeAssociations[s] == indx).ForEach(s => HighlightGroup(s, true));
            return;
        }
        indx -= _tile.TileAttributes.Count;
        _modelGroups.FindAll(s => _config.NodeAssociations.ContainsKey(s) && _config.NodeAssociations[s] == indx).ForEach(s => HighlightGroup(s, true));
        _placedTile.HighlightedNode = indx;
    }
    void AssignableDeselected()
    {
        if (!_loaded)
            return;
        _modelGroups.ForEach(s => HighlightGroup(s, false));
        _placedTile.HighlightedNode = -1;
    }
    void HighlightGroup(string group, bool enabled)
    {
        Spatial root = _modelRoot.GetNode<Spatial>(group);
        Assert(root != null);
        foreach (var it in GetChildrenRecrusively<MeshInstance>(root))
        {
            if (enabled)
            {
                it.MaterialOverride = new SpatialMaterial();
            }
            else
            {
                it.MaterialOverride = null;
            }
        }
        if (_config.NodeAssociations.ContainsKey(group))
        {
            if (!enabled)
            {
                _placedTile.HighlightedNode = -1;
                if (_assignableList.IsAnythingSelected())
                {
                    int indx = _assignableList.GetSelectedItems()[0] - 2;
                    if (indx > 0)
                        _placedTile.HighlightedNode = indx;
                }
            }
            else
                _placedTile.HighlightedNode = _config.NodeAssociations[group];
        }
        UpdateDisplays();
    }
    void UpdateMessages()
    {
        _statusLabel.Text = "";
        int uc = _groupAssigned.Count((kv) => !kv.Value);
        if (uc == 0)
        {
            _statusLabel.Text = "All OK";
        }
        else
            _statusLabel.Text = $"{uc} untracked groups";
    }
    void UnloadLists()
    {
        foreach (var it in _assignmentContainer.GetChildren())
        {
            if (it is Control c)
            {
                _assignmentContainer.RemoveChild(c);
                c.QueueFree();
            }
        }
        _assignableList.Clear();
    }
    void LoadLists()
    {
        UnloadLists();
        foreach (var it in _modelGroups)
        {
            var a = new Assigner(it, _tile, _config,
                (s, b) =>  // visible
                {
                    ((Spatial)_modelRoot.GetNode(s)).Visible = b;
                },
                (s, b) =>  // highlighted
                {
                    HighlightGroup(s, b);
                },
                (s) =>
                {
                    UpdateDisplays();
                    Validate(false);
                    UpdateMessages();
                }
            );
            _assignmentContainer.CallDeferred("add_child", a);
        }
        _assignableList.Visible = true;
        _assignableList.AddItem("<NOTHING>");
        _assignableList.AddItem("Unassigned");
        int i = 0;
        foreach(var it in _tile.TileAttributes)
        {
            _assignableList.AddItem($"{(TileAttributeType)it} ({i})");
            i++;
        }
        i = 0;
        foreach (var it in _tile.NodeTypes)
        {
            _assignableList.AddItem($"{(NodeType)it} ({i})");
            i++;
        }
        UpdateMessages();
    }
    void Validate(bool refreshlists = true)
    {
        Assert(_modelRoot != null);
        Assert(_tile != null);
        Assert(_config != null);

        _groupAssigned.Clear();
        foreach (var it in _modelGroups)
        {
            bool a = false;
            if (_config.Unassociated.Contains(it))
            {
                a = true;
            }
            else if (_config.NodeAssociations.ContainsKey(it))
            {
                int indx = _config.NodeAssociations[it];
                if (indx >= _tile.NodeTypes.Length)
                {
                    _config.NodeAssociations.Remove(it);
                }
                else
                    a = true;
            }
            else if (_config.AttributeAssociations.ContainsKey(it))
            {
                int indx = _config.AttributeAssociations[it];
                if (indx >= _tile.TileAttributes.Count)
                {
                    _config.NodeAssociations.Remove(it);
                }
                else
                    a = true;
            }
            _groupAssigned.Add(it, a);
        }
        if (refreshlists)
            LoadLists();
    }
    void SavePressed()
    {
        Assert(_modelPath != "");
        Assert(_modelRoot != null);
        Assert(_tileGraphicsConfig != null);

        string path = _modelPath.Replace(".tscn", ".json");

        string dt = JsonConvert.SerializeObject(_tileGraphicsConfig);

        var f = new File();

        Assert(f.Open(path, File.ModeFlags.Write));

        f.StoreString(dt);

        f.Close();

        Assert(new Directory().FileExists(path));

        string data = JsonConvert.SerializeObject(_tile);
        var fm = new File();
        fm.Open(_path, File.ModeFlags.Write);
        fm.StoreString(data);
        fm.Close();
    }
    void ResetPressed()
    {
        _config = new TileGraphicsConfig.Config();
        _tileGraphicsConfig.Configs[_path] = _config;
        Validate();
    }
    void ReloadPressed()
    {
        Assert(new Directory().FileExists(_modelPath));

        SetModel(_modelPath);
    }
    void AutoAssignPressed()
    {
        Assert(_modelRoot != null);
        Assert(_config != null);

        List<string> unassigned = _groupAssigned.Keys.ToList().FindAll(s => !_groupAssigned[s]);

        if (unassigned.Count == 0)
            return;

        foreach (var it in unassigned.FindAll(s => s.ToLower().Contains("base")))
        {
            _config.Unassociated.Add(it);
            _groupAssigned[it] = true;

            unassigned.Remove(it);
        }

        Dictionary<string, int> typeassociations = new Dictionary<string, int>()
        {
            {"farm", (int)NodeType.FARM},
            {"field", (int)NodeType.FARM},
            {"grass", (int)NodeType.FARM},
            {"road", (int)NodeType.ROAD},
            {"street", (int)NodeType.ROAD},
            {"city", (int)NodeType.CITY},
        };

        List<int> assignednodes = new List<int>();
        Dictionary<int, (Vector2 p, int n)> nodeposdict = new Dictionary<int, (Vector2, int)>();

        for (int i = 0; i < _tile.Assignments.Length; i++)
        {
            var it = _tile.Assignments[i];
            if (!nodeposdict.ContainsKey(it))
            {
                nodeposdict.Add(it, (new Vector2(0, 0), 0));
            }

            nodeposdict[it] = (nodeposdict[it].p + TilePrototype.RelativeConnectorPositions[i], nodeposdict[it].n + 1);
        }
        var npk = nodeposdict.Keys.ToList();
        for (int i = 0; i < npk.Count; i++)
        {
            var it = npk[i];
            nodeposdict[it] = (nodeposdict[it].p / (float)nodeposdict[it].n, nodeposdict[it].n);
        }
        List<(string name, int node, float dist)> l = new List<(string name, int node, float dist)>();

        foreach (var it in unassigned)
        {
            int utp;
            var a = typeassociations.Keys.ToList().Find(s => it.ToLower().Contains(s));
            if (a == default(string))
                continue;
            utp = typeassociations[a];
            for (int i = 0; i < _tile.NodeTypes.Length; i++)
            {
                var ntp = _tile.NodeTypes[i];
                if (ntp != utp)
                    continue;
                Vector3 p3 = _groupAveragePosition[it];
                Vector2 p2 = new Vector2(p3.x, p3.z);
                float d = nodeposdict[i].p.DistanceSquaredTo(p2);
                l.Add((it, i, d));
            }
        }
        if (l.Count != 0)
        {
            l.Sort
            (
                (p0, p1) =>
                {
                    return p0.dist.CompareTo(p1.dist);
                }
            );
            foreach (var it in l)
            {
                if (_groupAssigned[it.name] || assignednodes.Contains(it.node))
                    continue;
                Assert(it.node != -1);
                assignednodes.Add(it.node);
                _config.SetNodeAssociaiton(it.name, it.node);
                _groupAssigned[it.name] = true;
            }
            foreach (var it in l)
            {
                if (_groupAssigned[it.name])
                    continue;
                Assert(it.node != -1);
                _config.SetNodeAssociaiton(it.name, it.node);
                _groupAssigned[it.name] = true;
            }
        }
        Validate();
    }
    void ModelSelected(int indx)
    {
        var p = _tile.AssociatedModels[indx];
        if (p == _modelPath)
            return;
        SetModel(p);
    }
    void OpacityChanged(float val)
    {
        _tileLogicOverlay.Opacity = val / (float)_logicOpacitySlider.MaxValue;
    }
    void RotateGroupPositions(int rot)
    {
        _graphicsRotation += rot;
        float r = ((float)PI / 2) * (float)rot;
        foreach (var it in _groupAveragePosition.Keys.ToList())
        {
            _groupAveragePosition[it] = _groupAveragePosition[it].Rotated(Vector3.Up, r);
        }
    }
    void SetRotation(int rot)
    {
        RotateGroupPositions(-_graphicsRotation);
        rot = _config.Rotation = (rot) % GameEngine.N_SIDES;
        float r = ((float)PI / 2) * (float)rot;
        _modelRoot.Rotation = new Vector3(0, 0, 0);
        _modelRoot.Rotate(Vector3.Up, r);
        RotateGroupPositions(rot);
    }
    void RotatePressed()
    {
        Assert(_modelRoot != null);
        Assert(_config != null);

        SetRotation(_config.Rotation+1);
    }
    void AddModelPressed()
    {
        FastSearchFilesPopup pop = (FastSearchFilesPopup)GD.Load<PackedScene>("res://TileEditor/FastSearchFilesPopup.tscn").Instance();

        Assert(pop != null);

        pop.FilterHandle = s => s.EndsWith(".tscn") && !_tile.AssociatedModels.Contains(s);
        pop.CompleteHandle = (string s, bool comp) =>
        {
            if (comp)
            {
                if (!_tile.AssociatedModels.Contains(s))
                    _tile.AssociatedModels.Add(s);
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
    void DelModelPressed()
    {
        if (_modelPath == "" || _modelRoot == null)
            return;
        Assert(new Directory().FileExists(_modelPath));
        _tileGraphicsConfig.Configs.Remove(_path);
        if(_tile.AssociatedModels.Contains(_modelPath))
           _tile.AssociatedModels.Remove(_modelPath);
        _config = null;
        _modelSelector.RemoveItem(_modelSelector.Items.IndexOf(_modelPath.Split("/").Last()));
        SetModel("");
    }
    void SetModel(string path)
    {
        Assert(path != null);
        Assert(_tile != null);

        _modelPath = path;

        UnloadDisplays();
        if (path == "")
        {
            SetInterfaceActive(false);
            UpdateOptions();
            return;
        }
        Directory dr = new Directory();
        Assert(dr.FileExists(path));
        string jsonpath = path.Replace(".tscn", ".json");
        if (dr.FileExists(jsonpath))
        {
            var f = new File();
            Assert(f.Open(jsonpath, File.ModeFlags.Read));
            var dt = f.GetAsText();
            _tileGraphicsConfig = JsonConvert.DeserializeObject<TileGraphicsConfig>(dt);
            f.Close();
            Assert(_tileGraphicsConfig != null && _tileGraphicsConfig.Configs != null);
            if (!_tileGraphicsConfig.Configs.ContainsKey(_path))
                _tileGraphicsConfig.Configs.Add(_path, _config = new TileGraphicsConfig.Config());
            else
                _config = _tileGraphicsConfig.Configs[_path];
        }
        else
        {
            _tileGraphicsConfig = new TileGraphicsConfig();
            _config = new TileGraphicsConfig.Config();
            _tileGraphicsConfig.Configs.Add(_path, _config);
        }

        Assert(_modelPath != "");
        Assert(_modelPath.Contains(".tscn"));

        _modelRoot = (Spatial)GD.Load<PackedScene>(_modelPath).Instance();

        Assert(_modelRoot != null);

        _modelGroups.Clear();
        _groupAssigned.Clear();
        _groupAveragePosition.Clear();

        foreach (var it in _modelRoot.GetChildren())
        {
            if (it is Spatial s)
            {
                Assert(!(it is MeshInstance), "Error: Mesh instance at depth 1");
                _modelGroups.Add(s.Name);
                _groupAveragePosition.Add(s.Name, new Vector3(0, 0, 0));
            }
        }

        Assert(_modelGroups.Count > 0, "Error: The tile model has no groups");



        // Note: Change if meshes get proper transforms. TAGS: translations, transforms, meshinstances
        _modelGroups.ForEach(s =>
            {
                int n = 0;
                GetChildrenRecrusively<MeshInstance>(_modelRoot.GetNode<Spatial>(s)).ForEach(
                mi =>
                {
                    n++;
                    Vector3 av = new Vector3(0, 0, 0);
                    for (int i = 0; i < mi.Mesh.GetSurfaceCount(); i++)
                    {
                        var arr = mi.Mesh.SurfaceGetArrays(i);
                        Vector3[] a = (Vector3[])arr[(int)ArrayMesh.ArrayType.Vertex];
                        foreach (var it in a)
                        {
                            av += it / a.Length;
                        }
                    }
                    _groupAveragePosition[s] = _groupAveragePosition[s] + av;
                });
                _groupAveragePosition[s] = _groupAveragePosition[s] / n;
            }
        );
        SetRotation(_config.Rotation);
        LoadDisplays();
        Validate();
        UpdateOptions();

    }
    void UpdateOptions()
    {
        if (_tile == null)
        {
            _modelSelector.Clear();
            return;
        }
        Assert(_tile != null);
        _modelSelector.Clear();
        int i = 0;
        foreach (var it in _tile.AssociatedModels)
        {
            _modelSelector.AddItem(it.Split("/").Last());
            if (it == _modelPath)
            {
                _modelSelector.CallDeferred("select", i);
            }
            i++;
        }
        if (_tile.AssociatedModels.Count > 0 && _modelRoot == null)
        {
            _modelSelector.Select(0);
            SetModel(_tile.AssociatedModels[0]);
        }
    }
    void MaybeLoad()
    {
        if (_tile != null && _modelRoot != null)
        {
            Validate();
            LoadDisplays();
            UpdateDisplays();
        }
        SetInterfaceActive(false);
        if (_tile != null)
        {
            _addModelButton.Disabled = false;
            _modelSelector.Disabled = false;
        }
    }
    public void SetTile(TilePrototype tile, string path)
    {
        _path = path;
        if (_tile == tile)
        {
            if (_tile != null && _modelRoot != null)
                UpdateDisplays();
            return;
        }
        _tile = tile;
        if (_tile == null)
        {
            _assignableList.Clear();
            UnloadDisplays();
            return;
        }
        UnloadDisplays();
        MaybeLoad();
        UpdateOptions();
    }
    void LoadDisplays()
    {
        if (_loaded)
            return;
        _loaded = true;

        Assert(_modelRoot != null);

        _3DRoot.AddChild(_modelRoot);

        _placedTile = new PlacedTile();
        _placedTile.RenderedTile = _tile.Convert();
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

        _statusLabel.Text = "";

        _modelRoot.GetParent().RemoveChild(_modelRoot);
        _modelRoot.QueueFree();
        UnloadLists();

        _placedTile.GetParent().RemoveChild(_placedTile);
        _placedTile.QueueFree();
        _placedTile = null;
        _modelRoot = null;
        _config = null;
        _tileGraphicsConfig = null;
    }
    void UpdateDisplays()
    {
        CallDeferred("ActuallyUpdateDisplays");
    }
    void ActuallyUpdateDisplays()
    {
        _placedTile.RenderedTile = _tile.Convert();
        _placedTile.Update();
        SetInterfaceActive(true);
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
        if (_tile != null)
        {
            _addModelButton.Disabled = false;
            _modelSelector.Disabled = false;
        }
    }

    public override void _Ready()
    {
        _toolboxContainer = (Container)GetNode("ToolsContainer");

        _saveButton = (Button)GetNode("ToolsContainer/SaveButton");
        _resetButton = (Button)GetNode("ToolsContainer/ResetButton");
        _reloadButton = (Button)GetNode("ToolsContainer/ReloadButton");
        _autoAssignButton = (Button)GetNode("ToolsContainer/AutoAssignButton");

        _saveButton.Connect("pressed", this, "SavePressed");
        _resetButton.Connect("pressed", this, "ResetPressed");
        _reloadButton.Connect("pressed", this, "ReloadPressed");
        _autoAssignButton.Connect("pressed", this, "AutoAssignPressed");

        _modelSelector = (OptionButton)GetNode("ToolsContainer/OptionButton");
        _addModelButton = (Button)GetNode("ToolsContainer/AddAssociationsButton");
        _delModelButton = (Button)GetNode("ToolsContainer/RemoveAssociationButton");

        _modelSelector.Connect("item_selected", this, "ModelSelected");
        _addModelButton.Connect("pressed", this, "AddModelPressed");
        _delModelButton.Connect("pressed", this, "DelModelPressed");

        _toggleNodeContainer = (VBoxContainer)GetNode("HBoxContainer/VBoxContainer/HBoxContainer/ToggleNodeContainer");
        _statusLabel = (Label)GetNode("HBoxContainer/VBoxContainer2/StatusLabel");
        _assignmentContainer = (VBoxContainer)GetNode("HBoxContainer/VBoxContainer2/AssignmentContainer");


        _2DRoot = (Node2D)GetNode("HBoxContainer/VBoxContainer/HBoxContainer/2DViewport/Viewport/2DRoot");
        _camera2D = (Camera2D)GetNode("HBoxContainer/VBoxContainer/HBoxContainer/2DViewport/Viewport/2DRoot/Camera2D");
        _viewport2D = (Viewport)GetNode("HBoxContainer/VBoxContainer/HBoxContainer/2DViewport/Viewport");

        _3DRoot = (Spatial)GetNode("HBoxContainer/VBoxContainer/3DViewport/Viewport/3DRoot");
        _camera3D = (OrbitCamera)GetNode("HBoxContainer/VBoxContainer/3DViewport/Viewport/3DRoot/CameraHolder/Camera3D");
        _camera3D.Distance = Constants.TILE_SIDE_LENGTH;

        _tileLogicOverlay = (Sprite3D)GetNode("HBoxContainer/VBoxContainer/3DViewport/Viewport/3DRoot/TileLogicOverlay");

        _tileLogicOverlay.Scale = new Vector3(1f / 3f, 1f / 3f, 1f / 3f);
        _tileLogicOverlay.Translation = new Vector3(0, Constants.TILE_HEIGHT * 2f, 0);

        _logicOpacitySlider = (Slider)GetNode("HBoxContainer/VBoxContainer/3DViewport/Viewport/CanvasLayer/VBoxContainer/TileLogicOpacitySlider");
        _logicOpacitySlider.Value = _logicOpacitySlider.MaxValue / 2;

        _logicOpacitySlider.Connect("value_changed", this, "OpacityChanged");

        _tileLogicOverlay.Opacity = 0.5f;

        _rotateTileButton = (Button)GetNode("HBoxContainer/VBoxContainer/3DViewport/Viewport/CanvasLayer/VBoxContainer/RotateTileButton");

        _rotateTileButton.Connect("pressed", this, "RotatePressed");

        _logicOpacitySlider.SetProcessInput(false);
        _rotateTileButton.Disabled = true;

        _assignableList = GetNode<ItemList>("HBoxContainer/VBoxContainer/HBoxContainer/ToggleNodeContainer/AssignlableList");
        _assignableList.Connect("item_selected", this, "AssignableSelected");

        SetInterfaceActive(false);

    }
    public override void _Process(float delta)
    {
        if (_loaded)
        {
            _tileLogicOverlay.Texture = _viewport2D.GetTexture();
        }
        _rotateTileButton.Disabled = !_loaded;
    }
}
