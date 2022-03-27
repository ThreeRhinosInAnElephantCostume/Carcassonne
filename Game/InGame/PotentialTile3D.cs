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
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;
public class PotentialTile3D : Spatial
{
    const string PLACE_ACTION = "map_tile_place";
    const string ROTATE_ACTION = "map_tile_rotate";
    static string[] ACTIONS = new string[] { PLACE_ACTION, ROTATE_ACTION }; public Action<Vector2I, int> OnPlaceHandle;
    public Tile3D PTile { get; set; } = null;
    Spatial _visRoot;
    Spatial _center;
    Spatial _front, _back, _right, _left;
    Spatial[] dirs;
    CollisionShape _colshape;
    Area _area;
    List<int> _rotations = new List<int>();
    int _currotpos = 0;
    static bool _sharedInputLockout = false;
    Vector2I _position;
    AudioPlayer _gameAudio;
    public Vector2I PotentialPosition
    {
        get => _position;
        set
        {
            _position = value;
            this.Translation = GridPosTo3D(_position);
        }
    }
    public void AddRotation(int rot, bool deferred = false)
    {
        if (!deferred)
        {
            Assert(!_rotations.Contains(rot));
            _rotations.Add(rot);
        }
        if (this.dirs == null || this.dirs.Length == 0)
        {
            CallDeferred("AddRotation", rot, true);
            return;
        }
        Assert(rot >= 0 && rot <= 3);
        this.dirs[rot].Visible = true;
    }
    void MouseEntered()
    {
        if (_sharedInputLockout)
            return;
        _visRoot.Visible = false;
        _gameAudio.PlaySound("TileOverSpotSound");
        if (PTile.GetParent() != null)
            PTile.GetParent().RemoveChild(PTile);
        this.AddChild((PTile));
        PTile.Rot = _rotations[_currotpos];
    }
    void MouseExited()
    {
        if (_sharedInputLockout)
            return;
        _visRoot.Visible = true;
        if (PTile.GetParent() == this)
            PTile.GetParent().RemoveChild(PTile);
        PTile.Rot = 0;
    }
    void AreaInputEvent(Camera camera, InputEvent @event, Vector3 clickpos, Vector3 clicknormal, int shapeindx)
    {
        if (_sharedInputLockout)
            return;
        if (InputMap.EventIsAction(@event, PLACE_ACTION) && Input.IsActionJustPressed(PLACE_ACTION))
        {
            _sharedInputLockout = true;
            PTile.Rot = _rotations[_currotpos];
            OnPlaceHandle(PotentialPosition, _rotations[_currotpos]);
            _area.InputRayPickable = false;
            DestroyNode(_area);
        }
        else if (InputMap.EventIsAction(@event, ROTATE_ACTION) && Input.IsActionJustPressed(ROTATE_ACTION))
        {
            if (_rotations.Count > 1)
                _gameAudio.PlaySound("TileRotationAvailableSound");
            else
                _gameAudio.PlaySound("TileRotationDisabledSound");
            _currotpos = (_currotpos + 1) % (_rotations.Count);
            PTile.Rot = _rotations[_currotpos];
        }
    }
    public override void _Ready()
    {
        _gameAudio = GetNode<AudioPlayer>("/root/AudioPlayer");
        foreach (var it in ACTIONS)
            Assert(InputMap.HasAction(it));
        _sharedInputLockout = false;
        _visRoot = GetNode<Spatial>("VisRoot");
        _center = GetNode<Spatial>("VisRoot/CenterPiece");
        _front = GetNode<Spatial>("VisRoot/Front");
        _back = GetNode<Spatial>("VisRoot/Back");
        _right = GetNode<Spatial>("VisRoot/Right");
        _left = GetNode<Spatial>("VisRoot/Left");
        _area = GetNode<Area>("Area");
        _colshape = GetNode<CollisionShape>("Area/CollisionShape");
        _area.Connect("mouse_entered", this, "MouseEntered");
        _area.Connect("mouse_exited", this, "MouseExited");
        _area.Connect("input_event", this, "AreaInputEvent");

        this.dirs = new Spatial[] { _front, _right, _back, _left };

        _area.InputRayPickable = true;
        foreach (var it in this.dirs)
        {
            it.Visible = false;
        }
    }

    private void _OnPotentialTileTreeExiting()
    {
        _gameAudio.PlaySound("TilePlacedSound");
    }
}





