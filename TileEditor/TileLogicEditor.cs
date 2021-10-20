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
public class TileLogicEditor : Control
{
    class ButtonHandleObject : Godot.Object
    {
        int indx;
        Action<int> action;
        public void Pressed()
        {
            action(indx);
        }
        public ButtonHandleObject(int indx, Action<int> action)
        {
            this.indx = indx;
            this.action = action;
        }
    }
    bool AutoTrackCityConnections { get; set; } = true;
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
    public Action<TilePrototype, string> TileChangedHandle;
    TilePrototype _tile;
    public TilePrototype Tile
    {
        get => _tile; protected set
        {
            _tile = value;
            TileChanged();
        }
    }
    Camera2D camera;
    Node2D root;
    bool loaded = false;
    List<Button> _connectorButtons = new List<Button>();
    Node2D nd;

    Button _saveButton;
    Button _resetButton;
    Button _fillButton;
    Button _reloadButton;
    Container _toolboxContainer;

    ItemList _attributableList;
    ItemList _possibleAttributeList;
    ItemList _currentAttributeList;

    Container _attributeButtonContainer;
    Button _addAttributeButton;
    Button _removeAttributeButton;
    Button _resetAttributeButton;
    CheckBox _updateCityTrackCheck;
    CheckBox _userEditableCheck;

    PlacedTile _placedTile;
    void TileChanged()
    {
        TileChangedHandle(Tile, Path);
    }
    void UpdateNearCityTrack(bool force = false)
    {
        if (!AutoTrackCityConnections && !force)
            return;
        bool changed = false;

        for (int i = 0; i < Tile.NodeTypes.Length; i++)
        {
            if (!Tile.NodeAttributes.ContainsKey(i))
            {
                Tile.NodeAttributes.Add(i, new List<int>());
            }
            var tp = Tile.NodeTypes[i];
            if (tp != (int)NodeType.FARM)
                continue;
            bool b = false;
            for (int ii = 0; ii < Tile.Assignments.Length; ii++)
            {
                if (Tile.Assignments[ii] != i)
                    continue;
                var p = Tile.NodeTypes[Tile.Assignments[AbsMod(ii - 1, Tile.Assignments.Length)]];
                var n = Tile.NodeTypes[Tile.Assignments[AbsMod(ii + 1, Tile.Assignments.Length)]];
                if (p == (int)NodeType.CITY || n == (int)NodeType.CITY)
                {
                    b = true;
                    break;
                }
            }
            if (b)
            {
                if (!Tile.NodeAttributes[i].Contains((int)NodeAttributeType.NEAR_CITY))
                {
                    Tile.NodeAttributes[i].Add((int)NodeAttributeType.NEAR_CITY);
                    changed = true;
                }
            }
            else
            {
                if (Tile.NodeAttributes[i].Contains((int)NodeAttributeType.NEAR_CITY))
                {
                    Tile.NodeAttributes[i].Remove((int)NodeAttributeType.NEAR_CITY);
                    changed = true;
                }
            }
        }
        if (changed)
        {
            ResetLists();
            UpdateTileDisplay();
        }
    }
    void CityTrackToggled(bool b)
    {
        AutoTrackCityConnections = b;
        if (b)
            UpdateNearCityTrack();
    }
    void AutoEditableToggled(bool b)
    {
        Tile.UserEditable = b;
        TileChanged();
    }
    void CurrentNodeSelected(int indx)
    {
        _possibleAttributeList.Clear();
        _currentAttributeList.Clear();
        _placedTile.HighlightedNode = indx - 1;
        if (indx == 0)
        {
            foreach (TileAttributeType it in Enum.GetValues(typeof(TileAttributeType)))
            {
                if (it == TileAttributeType.ERR)
                    continue;
                string s = it.ToString();
                if (Tile.TileAttributes.Contains((int)it))
                    _currentAttributeList.AddItem(s);
                else
                    _possibleAttributeList.AddItem(s);
            }
            UpdateTileDisplay();
            return;
        }
        indx -= 1;
        foreach (NodeAttributeType it in Enum.GetValues(typeof(NodeAttributeType)))
        {
            if (it == NodeAttributeType.ERR)
                continue;
            if (!Tile.NodeAttributes.ContainsKey(indx))
                Tile.NodeAttributes.Add(indx, new List<int>());
            var l = Tile.NodeAttributes[indx];
            string s = it.ToString();
            if (l.Contains((int)it))
                _currentAttributeList.AddItem(s);
            else
                _possibleAttributeList.AddItem(s);
        }
        _resetAttributeButton.Disabled = false;
        UpdateTileDisplay();
    }
    void CurrentNodeDeselected()
    {
        if (!loaded)
            return;
        _placedTile.HighlightedNode = -1;
        ResetLists();
        _resetAttributeButton.Disabled = true;
        UpdateTileDisplay();
    }
    void PossibleAttributeSelected(int indx)
    {
        _addAttributeButton.Disabled = false;
    }
    void PossibleAttributeDeselected()
    {
        if (!loaded)
            return;
        _addAttributeButton.Disabled = true;
    }
    void CurrentAttributeSelect(int indx)
    {
        _removeAttributeButton.Disabled = false;
    }
    void CurrentAttributeDeselect(int indx)
    {
        _removeAttributeButton.Disabled = true;
    }
    void AddAttributePressed(int indx)
    {
        Assert(_attributableList.IsAnythingSelected());
        Assert(_possibleAttributeList.IsAnythingSelected());

        string name = (string)_possibleAttributeList.GetItemText(indx);
        int sel = _attributableList.GetSelectedItems()[0];
        if (sel == 0)
        {
            var ta = Enum.Parse<TileAttributeType>(name, true);
            _currentAttributeList.AddItem(ta.ToString());
            _possibleAttributeList.RemoveItem(indx);
            Assert(!Tile.TileAttributes.Contains((int)ta));
            Tile.TileAttributes.Add((int)ta);
            return;
        }
        sel -= 1;
        var na = Enum.Parse<NodeAttributeType>(name, true);
        _currentAttributeList.AddItem(na.ToString());
        _possibleAttributeList.RemoveItem(indx);
        Assert(!Tile.NodeAttributes[sel].Contains((int)na));
        Tile.NodeAttributes[sel].Add((int)na);
        if (!_currentAttributeList.IsAnythingSelected())
            _removeAttributeButton.Disabled = true;
        if (!_possibleAttributeList.IsAnythingSelected())
            _addAttributeButton.Disabled = true;
        return;
    }
    void RemoveAttributePressed(int indx)
    {
        Assert(_attributableList.IsAnythingSelected());

        string name = (string)_currentAttributeList.GetItemText(indx);
        int sel = _attributableList.GetSelectedItems()[0];
        if (sel == 0)
        {
            var ta = Enum.Parse<TileAttributeType>(name, true);
            _currentAttributeList.RemoveItem(indx);
            _possibleAttributeList.AddItem(ta.ToString());
            Assert(Tile.TileAttributes.Contains((int)ta));
            Tile.TileAttributes.Remove((int)ta);
            return;
        }
        sel -= 1;
        var na = Enum.Parse<NodeAttributeType>(name, true);
        _currentAttributeList.RemoveItem(indx);
        _possibleAttributeList.AddItem(na.ToString());
        Assert(Tile.NodeAttributes[sel].Contains((int)na));
        Tile.NodeAttributes[sel].Remove((int)na);
        if (!_currentAttributeList.IsAnythingSelected())
            _removeAttributeButton.Disabled = true;
        if (!_possibleAttributeList.IsAnythingSelected())
            _addAttributeButton.Disabled = true;
        return;

    }
    void ResetAttributePressed()
    {
        while (_currentAttributeList.GetItemCount() > 0)
        {
            RemoveAttributePressed(0);
        }
    }
    void CurAddAttributePressed()
    {
        AddAttributePressed(_possibleAttributeList.GetSelectedItems()[0]);
    }
    void CurRemoveAttributePressed()
    {
        RemoveAttributePressed(_currentAttributeList.GetSelectedItems()[0]);
    }
    void ResetLists()
    {
        _currentAttributeList.Clear();
        _possibleAttributeList.Clear();
        _attributableList.Clear();
        _addAttributeButton.Disabled = true;
        _removeAttributeButton.Disabled = true;
        _resetAttributeButton.Disabled = true;
        if (Tile == null)
        {
            return;
        }
        _attributableList.AddItem("<<TILE>>");
        int i = 0;
        foreach (var it in Tile.NodeTypes)
        {
            if (it == (int)NodeType.ERR)
                continue;
            _attributableList.AddItem(((NodeType)it).ToString() + $"({i})");
            i++;
        }

    }
    void SetInterfaceActive(bool b)
    {
        void togglebuttons(Container con)
        {
            GetChildrenRecrusively<Button>(con).ForEach(bt => bt.Disabled = !b);
        }
        togglebuttons(_toolboxContainer);
        togglebuttons(_attributeButtonContainer);
    }
    void ButtonPressed(int indx)
    {
        Button b = _connectorButtons[indx];
        NodeType ctype = (NodeType)Tile.NodeTypes[Tile.Assignments[indx]];
        int cass = Tile.Assignments[indx];
        if (Input.IsMouseButtonPressed((int)ButtonList.Left))
        {
            NodeType next = EnumNext(ctype);
            if (next == NodeType.ERR)
                next = EnumNext(next);
            SetTileType(indx, next);
        }
        else if (Input.IsMouseButtonPressed((int)ButtonList.Right) && ctype != NodeType.ERR)
        {
            var ntl = Tile.NodeTypes.ToList();
            var samenodes = ntl.FindAll(t => t == (int)ctype);
            var samecons = Tile.Assignments.ToList().FindAll(a => a == cass);
            if (samecons.Count == 1 && samenodes.Count == 1)
                return;
            int start = (samecons.Count == 1) ? 0 : cass + 1;
            int next = -1;
            for (int i = start; i < ntl.Count; i++)
            {
                if (ntl[i] == (int)ctype)
                {
                    next = i;
                    break;
                }
            }
            if (next == -1)
            {
                ntl.Add((int)ctype);
                Tile.Assignments[indx] = ntl.Count - 1;
                Tile.NodeTypes = ntl.ToArray();
            }
            else
            {
                Tile.Assignments[indx] = next;
            }
            EnsureNoEmptyNodes();
        }
        UpdateTileDisplay();
        ResetLists();
        UpdateNearCityTrack();
    }
    void RemoveNode(int indx)
    {
        if (Tile.NodeAttributes.ContainsKey(indx))
            Tile.NodeAttributes.Remove(indx);
        var l = Tile.NodeTypes.ToList();
        for (int i = indx; i < Tile.NodeTypes.Length; i++)
        {
            if (Tile.NodeAttributes.ContainsKey(i))
            {
                Tile.NodeAttributes[i - 1] = Tile.NodeAttributes[i];
                Tile.NodeAttributes.Remove(i);
            }
        }
        l.RemoveAt(indx);
        Tile.NodeTypes = l.ToArray();
        for (int i = 0; i < Tile.Assignments.Length; i++)
        {
            if (Tile.Assignments[i] >= indx)
            {
                Tile.Assignments[i] -= 1;
            }
        }
        UpdateTileDisplay();
        ResetLists();
        UpdateNearCityTrack();
    }
    void EnsureNoEmptyNodes()
    {
        for (int i = 0; i < Tile.NodeTypes.Length; i++)
        {
            if (!Tile.Assignments.Contains(i))
            {
                RemoveNode(i);
                i--;
            }
        }
    }
    void SetTileType(int indx, NodeType nt)
    {
        if (Tile.NodeTypes.Length == 1 && (NodeType)Tile.NodeTypes[0] == NodeType.ERR)
        {
            Tile.NodeTypes[0] = (int)nt;
            Tile.Assignments[indx] = 0;
            UpdateTileDisplay();
            EnsureNoEmptyNodes();
            UpdateNearCityTrack();
            return;
        }
        int nindx = -1;
        for (int i = 0; i < Tile.NodeTypes.Length; i++)
        {
            if (nt == (NodeType)Tile.NodeTypes[i])
            {
                nindx = i;
                break;
            }
        }
        if (nindx == -1)
        {
            var l = Tile.NodeTypes.ToList();
            l.Add((int)nt);
            Tile.Assignments[indx] = l.Count - 1;
            Tile.NodeTypes = l.ToArray();
            _attributableList.AddItem(((NodeType)nt).ToString() + $"({l.Count - 1})");
        }
        else
        {
            Tile.Assignments[indx] = nindx;
        }
        EnsureNoEmptyNodes();
        UpdateNearCityTrack();
        UpdateTileDisplay();
    }
    void UnloadView()
    {
        SetInterfaceActive(false);
        if (!loaded)
            return;
        loaded = false;
        _connectorButtons.ForEach(b => b.QueueFree());
        _connectorButtons = new List<Button>(12);
        nd.GetParent().RemoveChild(nd);
        nd.QueueFree();
        foreach (var it in root.GetChildren())
        {
            if (it as Control != null)
            {
                ((Control)it).GetParent().RemoveChild((Control)it);
                ((Control)it).QueueFree();
            }
        }
        if (_placedTile != null)
        {
            _placedTile.GetParent().RemoveChild(_placedTile);
            _placedTile.QueueFree();
        }
    }
    void LoadView()
    {
        SetInterfaceActive(true);
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
        Vector2[] Dirs = new Vector2[]
        {
            new Vector2(0, -1),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(-1, 0),
        };
        float sz = 100f;
        float third = (2 * sz) / 3f;
        for (int i = 0; i < GameEngine.N_SIDES; i++)
        {
            for (int ii = 0; ii < GameEngine.N_CONNECTORS; ii++)
            {
                Button b = new Button();

                var edge = sz * Corners[i] + (ii) * (third * Dirs[AbsMod(i + 1, GameEngine.N_SIDES)]);

                var center = edge + (third / 2 * Dirs[i]);

                b.RectPosition = center - new Vector2(third / 2, third / 2) + (0.5f * third * Dirs[AbsMod(i + 1, GameEngine.N_SIDES)]);
                b.RectSize = new Vector2(third, third);


                b.ButtonMask = (int)ButtonList.MaskLeft | (int)ButtonList.MaskRight;
                b.Connect("button_down", new ButtonHandleObject(i * GameEngine.N_CONNECTORS + ii, (_i) => ButtonPressed(_i)), "Pressed");
                _connectorButtons.Add(b);
                nd.AddChild(b);
            }
        }
        _placedTile = new PlacedTile();
        _placedTile.Name = "TileView";
        Node n = root.GetNodeOrNull("TileView");
        if (n != null)
        {
            root.RemoveChild(n);
            n.QueueFree();
        }
        root.AddChild(_placedTile);
        _placedTile.size = 200f;
        _placedTile.outersize = new Vector2(200f, 200f);
        UpdateTileDisplay();
    }
    void FillPressed()
    {
        for (int i = 0; i < Tile.NodeTypes.Length; i++)
        {
            if (Tile.NodeTypes[i] == (int)NodeType.ERR)
                Tile.NodeTypes[i] = (int)NodeType.FARM;
        }
        UpdateTileDisplay();
        UpdateNearCityTrack();
    }
    void ResetPressed()
    {
        SetTile(new TilePrototype());
        UpdateTileDisplay();
    }
    void ReloadPressed()
    {
        Path = Path;
        UpdateTileDisplay();
    }
    void SavePressed()
    {
        string data = JsonConvert.SerializeObject(Tile);
        var fm = new File();
        fm.Open(Path, File.ModeFlags.Write);
        fm.StoreString(data);
        fm.Close();
        UpdateTileDisplay();
    }
    void SetTile(TilePrototype tile)
    {
        UnloadView();
        this.Tile = tile;
        _userEditableCheck.Pressed = false;

        if (Tile == null)
        {
            ResetLists();
            TileChanged();
            return;
        }
        _userEditableCheck.Pressed = tile.UserEditable;
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


        LoadView();
        ResetLists();
        UpdateTileDisplay();


    }
    void UpdateTileDisplay()
    {
        CallDeferred("ActuallyUpdateTileDisplay");
    }
    void ActuallyUpdateTileDisplay()
    {
        if (!loaded)
            return;
        int tc = GameEngine.N_CONNECTORS * GameEngine.N_SIDES;

        Assert(Tile.NodeTypes != null && Tile.NodeTypes.Length > 0);
        Assert(Tile.Assignments.Length == tc);
        Assert(_connectorButtons.Count == tc);

        for (int i = 0; i < tc; i++)
        {
            int indx = Tile.Assignments[i];
            _connectorButtons[i].Text = ((NodeType)Tile.NodeTypes[indx]).ToString();
            _connectorButtons[i].Text += $"({indx})";
        }
        if (Tile.IsValid)
        {
            _placedTile.RenderedTile = (Tile.Convert());
        }
        else
            _placedTile.RenderedTile = null;
        _placedTile.Update();
        TileChanged();
    }
    void SetPath(string path)
    {
        _path = path;
        if (path == "")
            SetTile(null);
        else
        {
            //Assert(ResourceLoader.Exists(path));
            //TilePrototype resource = ResourceLoader.Load<TilePrototype>(path, "res://TileEditor/TilePrototype.cs", true);
            //Assert(resource != null);
            //SetTile(resource);


            var dm = new Directory();
            Assert(dm.FileExists(path));
            var fm = new File();
            Assert(fm.Open(path, File.ModeFlags.Read));
            var dt = fm.GetAsText();
            fm.Close();
            TilePrototype tp;
            tp = JsonConvert.DeserializeObject<TilePrototype>(dt);
            Assert(tp != null);
            SetTile(tp);
        }
        UpdateTileDisplay();
    }
    public override void _Ready()
    {
        HBoxContainer upperhalf = (HBoxContainer)GetNode("VBoxContainer/UpperHalfContainer");
        HBoxContainer bottomhalf = (HBoxContainer)GetNode("VBoxContainer/BottomHalfContainer");

        _attributableList = (ItemList)upperhalf.GetNode("AttributableList");
        _possibleAttributeList = (ItemList)bottomhalf.GetNode("PossibleAttributeList");
        _currentAttributeList = (ItemList)bottomhalf.GetNode("CurrentAttributeList");

        _attributableList.Connect("item_selected", this, "CurrentNodeSelected");
        _attributableList.Connect("nothing_selected", this, "CurrentNodeDeselected");
        _possibleAttributeList.Connect("item_selected", this, "PossibleAttributeSelected");
        _possibleAttributeList.Connect("nothing_selected", this, "PossibleAttributeDeselected");
        _possibleAttributeList.Connect("item_activated", this, "AddAttributePressed");
        _currentAttributeList.Connect("item_selected", this, "CurrentAttributeSelect");
        _currentAttributeList.Connect("nothing_selected", this, "CurrentAttributeDeselect");
        _currentAttributeList.Connect("item_activated", this, "RemoveAttributePressed");

        _attributeButtonContainer = (Container)GetNode("VBoxContainer/BottomHalfContainer/AttributeControlBox");
        _addAttributeButton = (Button)GetNode("VBoxContainer/BottomHalfContainer/AttributeControlBox/AddAttributeButton");
        _removeAttributeButton = (Button)GetNode("VBoxContainer/BottomHalfContainer/AttributeControlBox/RemoveAttributeButton");
        _resetAttributeButton = (Button)GetNode("VBoxContainer/BottomHalfContainer/AttributeControlBox/ResetAttributesbutton");

        _addAttributeButton.Connect("pressed", this, "CurAddAttributePressed");
        _removeAttributeButton.Connect("pressed", this, "CurRemoveAttributePressed");
        _resetAttributeButton.Connect("pressed", this, "ResetAttributePressed");

        camera = (Camera2D)FindChild<ViewportContainer>(upperhalf).GetNode("Viewport/Camera2D");
        root = (Node2D)camera.GetNode("TileDisplayRoot");

        _toolboxContainer = (Container)GetNode("ToolsContainer");

        _saveButton = (Button)GetNode("ToolsContainer/SaveButton");
        _fillButton = (Button)GetNode("ToolsContainer/FillButton");
        _resetButton = (Button)GetNode("ToolsContainer/ResetButton");
        _reloadButton = (Button)GetNode("ToolsContainer/ReloadButton");
        _updateCityTrackCheck = (CheckBox)GetNode("ToolsContainer/AutoAttrNearCities");
        _userEditableCheck = (CheckBox)GetNode("ToolsContainer/UserEditableCheck");

        _updateCityTrackCheck.Pressed = AutoTrackCityConnections;
        _userEditableCheck.Pressed = false;

        _saveButton.Connect("pressed", this, "SavePressed");
        _fillButton.Connect("pressed", this, "FillPressed");
        _resetButton.Connect("pressed", this, "ResetPressed");
        _reloadButton.Connect("pressed", this, "ReloadPressed");
        _updateCityTrackCheck.Connect("toggled", this, "CityTrackToggled");
        _userEditableCheck.Connect("toggled", this, "AutoEditableToggled");

        SetInterfaceActive(false);
    }
    public override void _Process(float delta)
    {
        root.Position = camera.GetViewportRect().Size / 2;
    }
}
