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
    const string CAMERA_ROTATE_LEFT = "camera_rotate_left";
    const string CAMERA_ROTATE_RIGHT = "camera_rotate_right";
    const string CAMERA_ZOOM_IN = "camera_zoom_in";
    const string CAMERA_ZOOM_OUT = "camera_zoom_out";
    const string CAMERA_CYCLE_VIEW_ACTION = "camera_cycle_view";
    const string CAMERA_RESET_VIEW_ACTION = "camera_reset_view";
    static readonly string[] ACTIONS = new string[]
    {
        CAMERA_FORWARD_ACTION, CAMERA_RIGHT_ACTION, CAMERA_LEFT_ACTION, CAMERA_BACK_ACTION,
        CAMERA_UP_ACTION, CAMERA_DOWN_ACTION, CAMERA_ROTATE_LEFT, CAMERA_ROTATE_RIGHT,
        CAMERA_ZOOM_IN, CAMERA_ZOOM_OUT, CAMERA_CYCLE_VIEW_ACTION, CAMERA_RESET_VIEW_ACTION,
    };

    [Export]
    public float MomentumGainMultpilier = 0.2f;
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
    public float KeyboardRotateSpeedMultip = 1f;
    [Export]
    public float ZoomSpeedMultip = 0.15f;
    [Export]
    public float ZoomUpFactor = 0.4f;

    [Export]
    public float CameraUpStep = 1.8f;

    [Export]
    public float TiltMax = -0.05f * (float)PI;
    [Export]
    public float TiltMin = -0.3f * (float)PI;
    Vector2 _defaultRotation = new Vector2(-0.2f * (float)PI, 0);
    [Export]
    public Vector2 DefaultRotation
    {
        get => _defaultRotation;
        set
        {
            _defaultRotation = value;
            Rotation = new Vector3(_defaultRotation.x, _defaultRotation.y, 0);
        }
    }

    [Export]
    public float MinHeight = 0.5f;
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
    float _defaultHeight = 3.0f;
    [Export]
    public float DefaultHeight
    {
        get => _defaultHeight;
        set
        {
            _defaultHeight = value;
            Height = value;
        }
    }


    public enum Mode
    {
        NORMAL = 0,
        TOP = 1
    }
    [Export(PropertyHint.Enum, "Normal,Top")]
    int CameraViewMode
    {
        get => (int)ViewMode;
        set => ViewMode = (Mode)value;
    }
    Mode _viewMode = Mode.NORMAL;
    Mode ViewMode
    {
        get => _viewMode;
        set
        {
            if (value == _viewMode)
                return;
            _viewMode = value;
            UpdateViewMode();
        }
    }


    Vector2 _velocity = new Vector2();
    Vector2 _momentum = new Vector2();
    Vector2 _lastDisplacement = new Vector2();
    Vector2 _lastRotation = new Vector2();
    DateTime _lastTime;

    bool _moveDown = false;
    bool _rotateDown = false;
    readonly object _physMX = new object(); // will be nessessary if we ever decide to use multithreaded physics

    void Rotate(Vector2 rot)
    {
        var r = Rotation;
        r.x += rot.y;
        r.y += rot.x;
        r.x = Clamp(r.x, TiltMin, TiltMax);
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
    Vector2 RotatedAbsoluteMove(Vector2 v)
    {
        return v.Rotated(-Rotation.y);
    }
    public void ResetView()
    {

    }
    void UpdateViewMode()
    {

    }
    void StepZoom(float mousevec, float delta)
    {
        bool zi = Input.IsActionPressed(CAMERA_ZOOM_IN);
        bool zo = Input.IsActionPressed(CAMERA_ZOOM_OUT);
        float dir = 0;
        if (zi == zo)
            return;
        else if (zi)
            dir = 1.0f;
        else
            dir = -1.0f;
        float forward = dir * ZoomSpeedMultip * delta;

        //Vector2 upfactor = new Vector2(mousevec, 0).Rotated(Rotation.x);

        //float upward = upfactor.x * ZoomUpFactor * delta;
        float upward = mousevec * ZoomUpFactor * delta;


        Position += RotatedAbsoluteMove(new Vector2(0, forward));
        Height += upward;

    }
    public override void _UnhandledInput(InputEvent @event)
    {

        lock (_physMX)
        {
            _lastTime = DateTime.Now;
            if (@event is InputEventMouseMotion mmev)
            {
                if (_moveDown)
                {
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
                    _velocity = _momentum;
                    _momentum = new Vector2(0, 0);
                    _moveDown = false;
                }
                else if (!_moveDown && IsMoveTrigger(mbev))
                {
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
                if (Input.IsActionJustPressed(CAMERA_ZOOM_IN) || Input.IsActionJustPressed(CAMERA_ZOOM_OUT))
                {
                    float mousevec = -((mbev.GlobalPosition - GetViewport().Size * 0.5f) / (GetViewport().Size)).y;
                    StepZoom(mousevec, 1.0f);
                }
            }
        }

    }
    public override void _PhysicsProcess(float delta)
    {
        lock (_physMX)
        {
            if (_velocity.Length() > MaxSpeed)
                _velocity = _velocity.Normalized() * MaxSpeed;
            _velocity = FadeVector(_velocity, VelocityLossMultiplier * delta);
            _momentum = FadeVector(_momentum, MomentumLossMultiplier * delta);
            _momentum *= 0.99f;
            Position -= _velocity * delta * MouseMoveSpeedMultip;
            Position -= _lastDisplacement * MouseMoveSpeedMultip;
            Position += MoveDirection() * KeyboardMoveSpeedMultip;

            Rotate(_lastRotation * MouseRotateSpeedMultip);
            _lastDisplacement = new Vector2(0, 0);
            _lastRotation = new Vector2(0, 0);

            Vector2 keyboardrot = new Vector2();
            keyboardrot.x -= (Input.IsActionPressed(CAMERA_ROTATE_LEFT) ? 1.0f : 0.0f);
            keyboardrot.x += (Input.IsActionPressed(CAMERA_ROTATE_RIGHT) ? 1.0f : 0.0f);
            Rotate(keyboardrot * KeyboardRotateSpeedMultip * delta);

            if (Input.IsActionPressed(CAMERA_UP_ACTION) || Input.IsActionJustPressed(CAMERA_UP_ACTION))
                Height += delta * CameraUpStep;
            else if (Input.IsActionPressed(CAMERA_DOWN_ACTION) || Input.IsActionJustPressed(CAMERA_DOWN_ACTION))
                Height -= delta * CameraUpStep;
            StepZoom(0.0f, delta);
        }
    }

    public override void _Ready()
    {
        _lastTime = DateTime.Now;
        foreach (var it in ACTIONS)
        {
            Assert(InputMap.HasAction(it), $"Undefined camera action: {it}");
        }
        DefaultRotation = DefaultRotation;
        DefaultHeight = DefaultHeight;
    }
}
