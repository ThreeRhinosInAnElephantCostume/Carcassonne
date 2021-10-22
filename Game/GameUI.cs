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

public class GameUI : Control
{
    Camera _camera;
    TileMap3D _tileMap;
    float _speed = 1.0f;
    float _rotspeed = 0.01f * (float)PI;
    public override void _Ready()
    {
        _camera = GetNode<Camera>("Camera");
        _tileMap = GetNode<TileMap3D>("TileMap3D");
    }
    public override void _PhysicsProcess(float delta)
    {
        Vector3 pos = _camera.Translation;
        if (Input.IsKeyPressed((int)KeyList.W))
            pos.z -= _speed * delta;
        if (Input.IsKeyPressed((int)KeyList.S))
            pos.z += _speed * delta;
        if (Input.IsKeyPressed((int)KeyList.A))
            pos.x -= _speed * delta;
        if (Input.IsKeyPressed((int)KeyList.D))
            pos.x += _speed * delta;
        if (Input.IsKeyPressed((int)KeyList.Shift))
            pos.y += _speed * delta;
        if (Input.IsKeyPressed((int)KeyList.Control))
            pos.y -= _speed * delta;
        if (Input.IsKeyPressed((int)KeyList.Q))
            _camera.RotateY(_rotspeed * _speed);
        if (Input.IsKeyPressed((int)KeyList.E))
            _camera.RotateY(-_rotspeed * _speed);
        _camera.Translation = pos;
    }
}
