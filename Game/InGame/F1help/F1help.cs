using System;
using System.Collections.Generic;
using Godot;

public class F1help : Control
{
    Game _game;
    InGameUI _inGameUI;

    Control _saveLoadRoot;
    Control _mainRoot;

    Button _resumeButton;

    public Action OnResume = () => { };

    void OnResumePressed()
    {
        Visible = false;
        OnResume();
    }
    
    public void Init(Game game, InGameUI gameui)
    {
        this._game = game;
        this._inGameUI = gameui;
    }
    public override void _UnhandledInput(InputEvent @event)
    {
        if (Input.IsActionJustPressed("ui_cancel") || Input.IsActionPressed("ui_cancel"))
        {
            OnResumePressed();
        }
    }
    public override void _Input(InputEvent @event)
    {
        _UnhandledInput(@event);
    }


    public override void _Ready()
    {
        Visible = false;

        _mainRoot = this.GetNodeSafe<Control>("Panel/HBoxContainer");

        _resumeButton = this.GetNodeSafe<Button>
            ("Panel/HBoxContainer/Control2/VBoxContainer/Control2/VBoxContainer/ResumeButton");
        _resumeButton.OnButtonPressed(OnResumePressed);
    }
}
