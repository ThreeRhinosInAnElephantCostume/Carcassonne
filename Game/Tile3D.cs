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
using Carcassonne;
using ExtraMath;
using Godot;
using Newtonsoft.Json;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

public class Tile3D : Spatial
{
    Vector2I _gridPosition = new Vector2I();
    public Vector2I GridPosition
    {
        get => _gridPosition; set
        {
            _gridPosition = value;
            Translation = GridPosTo3D(_gridPosition);
        }
    }
    int _rotation = 0;
    public int TileRotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            this.Rotation = new Vector3(0, -AbsMod(_rotation + _config.Rotation, GameEngine.N_SIDES) * (float)PI / 2, 0);
        }
    }
    Spatial _modelRoot;
    Tile _tile;
    TileMap3D.TileModel _model;
    TileGraphicsConfig.Config _config;
    public Tile3D(Tile tile, TileMap3D.TileModel model)
    {
        Assert(tile != null);
        Assert(model != null);
        this._tile = tile;
        this._model = model;
        this._config = model.config;
        var inst = model.scene.Instance();
        Assert(inst != null);
        _modelRoot = (Spatial)inst;
        this.AddChild(_modelRoot);
    }
}
