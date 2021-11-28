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
    [Export]
    public bool MovementMomentum = true;
    [Export]
    public float MomentumGainMultpilier = 1.0f;
    [Export]
    public float MomentumLossMultiplier = 1.0f;
    [Export]
    public float VelocityLossMultiplier = 1.0f;
    [Export]
    public float MaxSpeed = 100.0f;
    Vector2 _velocity = new Vector2();
    Vector2 _momentum = new Vector2();
    DateTime _lasttime;

    public bool IsMoveOn()
    {
        return Input.IsMouseButtonPressed((int)ButtonList.Left);
    }
    public bool IsRotateOn()
    {
        return Input.IsMouseButtonPressed((int)ButtonList.Middle);
    }
    public override void _Input(InputEvent @event)
    {
        float delta = (float)(DateTime.Now - _lasttime).TotalSeconds;
        _lasttime = DateTime.Now;
        if(@event is InputEventMouseMotion mmev)
        {
            if(IsMoveOn())
            {
                
            }
            else
            {

            }
        }
        else if(@event is InputEventMouseButton mbev)
        { 

        }   
        else if(@event is InputEventKey kev)
        {

        }
        
    }
    public override void _UnhandledInput(InputEvent @event)
    {
        this._Input(@event);
    }
    public override void _PhysicsProcess(float delta)
    {

        if(MovementMomentum)
        {
            _velocity = FadeVector(_velocity, VelocityLossMultiplier);
        }
        
    }

    public override void _Ready()
    {
        _lasttime = DateTime.Now;   
    }
}
