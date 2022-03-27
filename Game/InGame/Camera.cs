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


public class Camera : Godot.Camera
{

    const string CAMERA_FORWARD_ACTION = "camera_forward";
    const string CAMERA_RIGHT_ACTION = "camera_right";
    const string CAMERA_LEFT_ACTION = "camera_left";
    const string CAMERA_BACK_ACTION = "camera_back";
    const string CAMERA_UP_ACTION = "camera_up";
    const string CAMERA_DOWN_ACTION = "camera_down";
    static readonly string[] ACTIONS
        = new string[] { CAMERA_FORWARD_ACTION, CAMERA_RIGHT_ACTION, CAMERA_LEFT_ACTION, CAMERA_BACK_ACTION, CAMERA_UP_ACTION, CAMERA_DOWN_ACTION };

    [Export]
    public bool MovementMomentum = true;
    [Export]
    public float MomentumGainMultpilier = 0.07f;
    [Export]
    public float MomentumLossMultiplier = 400.0f;
    [Export]
    public float VelocityLossMultiplier = 100.0f;
    [Export]
    public float MaxSpeed = 10.0f;
    [Export]
    public float MouseMoveSpeedMultip = 0.025f;
    [Export]
    public float MouseRotateSpeedMultip = 0.005f;
    [Export]
    public float KeyboardMoveSpeedMultip = 0.1f;

    [Export]
    public float CameraUpStep = 0.1f;

    [Export]
    public float VMax = 0f * (float)PI;
    [Export]
    public float VMin = -0.25f * (float)PI;

    [Export]
    public float MinHeight = 1;
    [Export]
    public float MaxHeight = 3;


    [Export]
    public Vector2 Position
    {
        get => new Vector2(Translation.x, Translation.z);
        protected set => Translation = new Vector3(value.x, Translation.y, value.y);
    }

    [Export]
    public float Height
    {
        get => Translation.y;
        protected set
        {
            Translation = new Vector3(Translation.x, Math.Max(Math.Min(value, MaxHeight), MinHeight), Translation.z);
        }
    }


    Vector2 _velocity = new Vector2();
    Vector2 _momentum = new Vector2();
    Vector2 _lastDisplacement = new Vector2();
    Vector2 _lastRotation = new Vector2();
    DateTime _lastTime;

    bool _moveDown = false;
    bool _rotateDown = false;

    object _physMX = new object(); // will be nessessary if we ever decide to use multithreaded physics

    void Rotate(Vector2 rot)
    {
        var r = Rotation;
        r.x += rot.y;
        r.y += rot.x;
        r.x = Clamp(r.x, VMin, VMax);
        Rotation = r;
    }

    bool IsMoveTrigger(InputEventMouseButton btn)
    {
        return btn.ButtonIndex == ((int)ButtonList.Left) && btn.Pressed;
    }
    bool IsMoveRelease(InputEventMouseButton btn)
    {
        return btn.ButtonIndex == ((int)ButtonList.Left) && !btn.Pressed;
    }
    bool IsRotateTrigger(InputEventMouseButton btn)
    {
        return btn.ButtonIndex == ((int)ButtonList.Middle) && btn.Pressed;
    }
    bool IsRotateRelease(InputEventMouseButton btn)
    {
        return btn.ButtonIndex == ((int)ButtonList.Middle) && !btn.Pressed;
    }
    int GetZoomStep(InputEventMouseButton btn)
    {
        if (!btn.Pressed)
            return 0;
        int r = 0;
        if (btn.ButtonIndex == (int)ButtonList.WheelUp)
            r++;
        if (btn.ButtonIndex == (int)ButtonList.WheelDown)
            r--;
        return r;

    }
    Vector2 MoveDirection()
    {
        var dir = new Vector2();
        dir += new Vector2(0, -1) * (Input.IsActionPressed(CAMERA_FORWARD_ACTION) ? 1.0f : 0f);
        dir += new Vector2(0, 1) * (Input.IsActionPressed(CAMERA_BACK_ACTION) ? 1.0f : 0f);
        dir += new Vector2(1, 0) * (Input.IsActionPressed(CAMERA_RIGHT_ACTION) ? 1.0f : 0f);
        dir += new Vector2(-1, 0) * (Input.IsActionPressed(CAMERA_LEFT_ACTION) ? 1.0f : 0f);
        dir = dir.Rotated(-Rotation.y);
        return dir;
    }
    public override void _UnhandledInput(InputEvent @event)
    {

        lock (_physMX)
        {
            float delta = (float)(DateTime.Now - _lastTime).TotalSeconds;
            _lastTime = DateTime.Now;
            if (@event is InputEventMouseMotion mmev)
            {
                if (_moveDown)
                {
                    if (MovementMomentum)
                        _momentum += (mmev.Speed * MomentumGainMultpilier).Rotated(-Rotation.y);
                    _lastDisplacement += (mmev.Relative).Rotated(-Rotation.y);
                }
                if (_rotateDown)
                {
                    _lastRotation += (mmev.Relative);
                }
            }
            else if (@event is InputEventMouseButton mbev)
            {
                if (_moveDown && IsMoveRelease(mbev))
                {
                    if (MovementMomentum)
                    {
                        _velocity = _momentum;
                        _momentum = new Vector2(0, 0);
                    }
                    _moveDown = false;
                }
                else if (!_moveDown && IsMoveTrigger(mbev))
                {
                    if (MovementMomentum)
                        _momentum = new Vector2(0, 0);
                    _velocity = new Vector2(0, 0);
                    _moveDown = true;
                }
                if (_rotateDown && IsRotateRelease(mbev))
                {
                    _rotateDown = false;
                }
                else if (!_rotateDown && IsRotateTrigger(mbev))
                {
                    _rotateDown = true;
                }
                Height += ((float)GetZoomStep(mbev)) * CameraUpStep;
            }
        }

    }
    public override void _PhysicsProcess(float delta)
    {
        lock (_physMX)
        {
            //GD.Print(Position, _momentum, _velocity);
            if (MovementMomentum)
            {
                if (_velocity.Length() > MaxSpeed)
                    _velocity = _velocity.Normalized() * MaxSpeed;
                _velocity = FadeVector(_velocity, VelocityLossMultiplier * delta);
                _momentum = FadeVector(_momentum, MomentumLossMultiplier * delta);
                _momentum *= 0.99f;
            }
            Position -= _velocity * delta * MouseMoveSpeedMultip;
            Position -= _lastDisplacement * MouseMoveSpeedMultip;
            Position += MoveDirection() * KeyboardMoveSpeedMultip;

            Rotate(_lastRotation * MouseRotateSpeedMultip);
            _lastDisplacement = new Vector2(0, 0);
            _lastRotation = new Vector2(0, 0);

            if (Input.IsActionPressed(CAMERA_UP_ACTION))
                Height += delta * CameraUpStep;
            else if (Input.IsActionPressed(CAMERA_DOWN_ACTION))
                Height -= delta * CameraUpStep;

        }
    }

    public override void _Ready()
    {
        _lastTime = DateTime.Now;
        foreach (var it in ACTIONS)
        {
            Assert(InputMap.HasAction(it), $"Undefined camera action: {it}");
        }
    }
}
