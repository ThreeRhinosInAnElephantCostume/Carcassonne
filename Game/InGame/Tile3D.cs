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

public class Tile3D : Spatial
{
    public Tile AssociatedTile { get; protected set; }
    TileDataLoader.TileModel _model;
    Spatial root;

    Vector2I _position = new Vector2I();
    public Vector2I Pos
    {
        get => _position;
        set
        {
            _position = value;
            this.Translation = GridPosTo3D(_position);
        }
    }

    int _rotation = 0;
    public int Rot
    {
        get => _rotation;
        set
        {
            _rotation = value;
            this.Rotation = new Vector3(this.Rotation.x, (float)_rotation * (float)PI / 2, this.Rotation.z);
        }
    }

    public Tile3D(Tile tile, int rot, RNG rng)
    {
        this.AssociatedTile = tile;
        Assert(tile.MetaData is string);
        string path = (string)tile.MetaData;
        var models = TileDataLoader.LoadPrototypeModels(path);
        Assert(models.Count > 0);
        _model = models[(int)rng.NextLong() % models.Count];
        root = (Spatial)_model.Scene.Instance();
        this.AddChild(root);

        root.RotateY((float)(_model.Config.Rotation * PI / 2));
        Pos = tile.Position;
        Rot = rot;
    }
}
