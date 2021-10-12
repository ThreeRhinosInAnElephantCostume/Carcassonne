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
using System.Threading.Tasks.Dataflow;
using Carcassonne;
using ExtraMath;
using Godot;
using Newtonsoft.Json;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

//[Tool]
public class OrbitCamera : Camera
{
    float _maxDistance = 10.0f;
    public float MaxDistance { get => _maxDistance; set { _maxDistance = value; Distance = Distance; } }
    float _minDistance = 1.0f;
    public float MinDistance { get => _minDistance; set { _minDistance = value; Distance = Distance; } }
    public float DistanceStep { get; set; } = 0.1f;
    public float Sensitivity { get; set; } = 1.0f;
    float _distance;
    public float Distance { get => _distance; set { _distance = Clamp(value, MinDistance, MaxDistance); UpdatePosition(); } }
    Vector3 _direction = Vector3.Down;
    public Vector3 Direction { get => _direction; set { _direction = value.Normalized(); UpdatePosition(); } }
    void UpdatePosition()
    {
        Translation = new Vector3(0, 0, 0);
        var pos = -Direction * Distance;
        LookAtFromPosition(pos, new Vector3(0, 0, 0), Vector3.Forward);
        GD.Print("tra: ", Translation);
        GD.Print("rot: ", RotationDegrees);
    }
    public void Rotate(Vector3 rot)
    {
        rot = new Vector3(rot.x, rot.z, rot.y);
        var q = new Quat(rot);
        Direction = q.Xform(Direction);
    }
    // void SetDirection(Vector3 dir)
    // {
    //     this.Translation = new Vector3(0,0,0);
    //     LookAt( (dir), Vector3.Up);
    //     UpdatePosition();
    // }
    public override void _Ready()
    {
        Distance = MinDistance + (MaxDistance - MinDistance) / 2;
        UpdatePosition();
    }
    public override void _Process(float delta)
    {

    }
    public override void _Input(InputEvent @event)
    {
        switch (@event)
        {
            case InputEventMouseMotion ev:
                {
                    if (Input.IsMouseButtonPressed((int)ButtonList.Middle))
                    {
                        Vector2 dif = ev.Relative * Sensitivity * (float)PI * 0.01f;
                        Rotate(new Vector3(dif.y, dif.x, 0));
                    }
                    break;
                }
            case InputEventMouseButton ev:
                {
                    if (ev.ButtonIndex == (int)ButtonList.WheelUp)
                    {
                        Distance -= DistanceStep;
                    }
                    else if (ev.ButtonIndex == (int)ButtonList.WheelDown)
                    {
                        Distance += DistanceStep;
                    }
                    break;
                }
        }
    }
    // public override void _UnhandledInput(InputEvent @event)
    // {
    //     this._Input(@event);
    // }
}
