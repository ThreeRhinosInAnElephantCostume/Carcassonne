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
class Assigner : HBoxContainer
{
    string _group;
    TilePrototype _tile;
    TileGraphicsConfig.Config _config;

    public Action<string, bool> SetVisibleHandle;
    public System.Action<string, bool> SetHighlightedHandle;
    public System.Action<string> UpdatedHandle;

    CheckBox _visibleCheck;
    Label _label;
    OptionButton _options;

    public void OptionSelected(int indx)
    {
        Assert(indx >= 0);
        if (indx == 0)
        {
            _config.RemoveAssociations(_group);
            UpdatedHandle(_group);
            return;
        }
        else if (indx == 1)
        {
            _config.RemoveAssociations(_group);
            _config.SetUnassociated(_group);
            UpdatedHandle(_group);
            return;
        }
        indx -= 2;
        if (indx >= _tile.TileAttributes.Count)
        {
            indx -= _tile.NodeAttributes.Count;
            Assert(indx < _tile.NodeTypes.Length);
            _config.RemoveAssociations(_group);
            _config.SetNodeAssociaiton(_group, indx);
            UpdatedHandle(_group);
            return;
        }
        Assert(indx < _tile.TileAttributes.Count);
        _config.RemoveAssociations(_group);
        _config.SetAttributeAssociation(_group, indx);
        UpdatedHandle(_group);
        return;
    }
    public void VisibilityToggled(bool b)
    {
        SetVisibleHandle(_group, b);
    }

    public void MouseEntered()
    {
        SetHighlightedHandle(_group, true);
    }
    public void MouseExited()
    {
        SetHighlightedHandle(_group, false);
    }

    public override void _Ready()
    {
        SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
        SizeFlagsVertical = (int)SizeFlags.Fill;

        _visibleCheck = new CheckBox();
        _label = new Label();
        _options = new OptionButton();
        _options.Clear();

        _options.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
        _options.SizeFlagsVertical = (int)SizeFlags.Fill;

        _label.Text = _group;

        _visibleCheck.Pressed = true;

        _visibleCheck.Connect("toggled", this, "VisibilityToggled");
        int tindx = -1;
        if (_config.AttributeAssociations.ContainsKey(_group))
            tindx = _tile.TileAttributes.IndexOf(_config.AttributeAssociations[_group]) + 2;
        _options.AddItem("<NOTHING>");
        _options.AddItem("Unassociated");
        int i = 0;
        foreach (var it in _tile.TileAttributes)
        {
            _options.AddItem($"{(TileAttributeType)it} ({i})");
            i++;
        }
        if (_config.NodeAssociations.ContainsKey(_group))
            tindx = _config.NodeAssociations[_group] + i + 2;
        i = 0;
        foreach (var it in _tile.NodeTypes)
        {
            _options.AddItem($"{(NodeType)it} ({i})");
            i++;
        }
        if (tindx == -1)
        {
            if (_config.Unassociated.Contains(_group))
                tindx = 1;
            else
                tindx = 0;
        }

        _options.Select(tindx);

        _options.Connect("item_selected", this, "OptionSelected");

        this.Connect("mouse_entered", this, "MouseEntered");
        this.Connect("mouse_exited", this, "MouseExited");
        _options.Connect("mouse_entered", this, "MouseEntered");
        _options.Connect("mouse_exited", this, "MouseExited");

        AddChild(_visibleCheck);
        AddChild(_label);
        AddChild(_options);
        SetVisibleHandle(_group, true);
        SetHighlightedHandle(_group, false);
    }
    public Assigner(string group, TilePrototype tile, TileGraphicsConfig.Config config,
        Action<string, bool> SetVisibleHandle, System.Action<string, bool> SetHighlightedHandle,
        System.Action<string> UpdatedHandle)
    {
        this._group = group;
        this._tile = tile;
        this._config = config;
        this.SetVisibleHandle = SetVisibleHandle;
        this.SetHighlightedHandle = SetHighlightedHandle;
        this.UpdatedHandle = UpdatedHandle;
    }
}
