using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Threading;
using System.Xml;
using Carcassonne;
using ExtraMath;
using Godot;
using Newtonsoft.Json;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

public class PlaceableTile : Spatial
{
    public Vector2I GridPosition { get; set; }
    public Tile3D tile3d;
    public bool Enabled { get; set; }
    public int TileRotation { get => tile3d.TileRotation; set => tile3d.TileRotation = value; }
    Spatial _visualisation;
    List<int> _rotations = new List<int>();
    Area _area;
    Action<PlaceableTile> PlaceTileHandle;
    public void MouseEntered()
    {
        if (!Enabled)
            return;
        _visualisation.Visible = false;
        if (tile3d.GetParent() != this)
        {
            TileRotation = _rotations[0];
            if (tile3d.GetParent() != null)
                tile3d.GetParent().RemoveChild(tile3d);
        }
        this.AddChild(tile3d);
        tile3d.Visible = true;
    }
    public void MouseExited()
    {
        if (!Enabled)
            return;
        tile3d.Visible = false;
        _visualisation.Visible = true;
    }
    public void MouseInput(Node node, InputEvent @event, Vector3 clickposition, Vector3 clicknormal, int shapeindx)
    {
        if (!Enabled)
            return;
        if (@event is InputEventMouseButton ev && ev.Pressed == false)
        {
            if (ev.ButtonIndex == (int)ButtonList.Left)
            {
                PlaceTileHandle(this);
            }
            else if (ev.ButtonIndex == (int)ButtonList.Right)
            {
                for (int i = 0; i < _rotations.Count; i++)
                {
                    if (_rotations[i] == TileRotation)
                    {
                        if (i == _rotations.Count - 1)
                            TileRotation = _rotations[0];
                        else
                            TileRotation = _rotations[i + 1];
                        break;
                    }
                }
            }

        }
    }
    public void Init(Tile3D tile3d, Vector2I pos, List<int> rotations, Action<PlaceableTile> PlaceTileHandle)
    {
        Assert(tile3d != null);
        Assert(rotations.Count > 0);

        this.tile3d = tile3d;
        this._rotations = rotations;
        this.PlaceTileHandle = PlaceTileHandle;
        this.GridPosition = pos;
        Enabled = true;

        TileRotation = rotations[0];
    }
    public override void _Ready()
    {
        _visualisation = GetNode<Spatial>("vis");
        _area = GetNode<Area>("Area");
        _area.Connect("mouse_entered", this, "MouseEntered");
        _area.Connect("mouse_exited", this, "MouseExited");
        _area.Connect("input_event", this, "MouseInput");
    }
}
