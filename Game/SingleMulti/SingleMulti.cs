using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Net.Sockets;
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

public class SingleMulti : Control
{
    Control _singleImage;
    Control _singleBanner;
    Control _multiImage;
    Control _multiBanner;
    Control _frameSelector;

    bool _isSingleplayer = true;
    bool _mouseTimeOut = false;

    void SelectSingle(bool frommouse)
    {
        if (frommouse && _mouseTimeOut)
            return;
        _frameSelector.Visible = true;
        _isSingleplayer = true;
        _frameSelector.LoseParent();
        _singleImage.AddChild(_frameSelector);
    }
    void SelectMulti(bool frommouse)
    {
        if (frommouse && _mouseTimeOut)
            return;
        _frameSelector.Visible = true;
        _isSingleplayer = false;
        _frameSelector.LoseParent();
        _multiImage.AddChild(_frameSelector);
    }
    void SelectNext()
    {
        if (_isSingleplayer)
            SelectMulti(false);
        else
            SelectSingle(false);
    }
    void ToLobby()
    {
        SetMainScene((_isSingleplayer) ? Globals.Scenes.LobbySingleplayerPacked : Globals.Scenes.LobbyMultiplayerPacked);
    }

    public override void _Ready()
    {
        _singleImage = GetNode<Control>("VBoxContainer2/HBoxContainer/CenterContainer/VBoxContainer/Control/Single");
        _singleImage.OnMouseEntered(() => SelectSingle(true));
        _singleBanner = GetNode<Control>("VBoxContainer2/HBoxContainer/CenterContainer/VBoxContainer/SingleBaner");
        _singleBanner.OnMouseEntered(() => SelectSingle(true));

        _multiImage = GetNode<Control>("VBoxContainer2/HBoxContainer/CenterContainer2/VBoxContainer/Control/Multi");
        _multiImage.OnMouseEntered(() => SelectMulti(true));
        _multiBanner = GetNode<Control>("VBoxContainer2/HBoxContainer/CenterContainer2/VBoxContainer/MultiBanner");
        _multiBanner.OnMouseEntered(() => SelectMulti(true));

        _frameSelector = GetNode<Control>("FrameSelector");
        SelectSingle(false);
    }
    public override void _UnhandledKeyInput(InputEventKey @event)
    {
        _Input(@event);
    }
    public override void _UnhandledInput(InputEvent @event)
    {
        _Input(@event);
    }
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey IEK && !IEK.IsPressed())
        {
            if (IEK.IsAction("ui_accept"))
            {
                ToLobby();
            }
            else if (IEK.IsAction("ui_cancel"))
            {
                SetMainScene(Globals.Scenes.MainMenuPacked);
            }
            else if (IEK.IsAction("ui_focus_next") || IEK.IsAction("ui_focus_prev")
                || IEK.IsAction("ui_up") || IEK.IsAction("ui_down")
                || IEK.IsAction("ui_left") || IEK.IsAction("ui_right"))
            {
                SelectNext();
                _mouseTimeOut = true;
            }
        }
        else if (@event is InputEventMouseButton IEMB && !IEMB.Pressed)
        {
            if (_mouseTimeOut)
                _mouseTimeOut = false;
            else
                ToLobby();
        }
        else if (@event is InputEventMouseMotion)
        {
            _mouseTimeOut = false;
        }
    }
}
