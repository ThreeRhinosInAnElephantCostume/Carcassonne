using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Threading;
using Carcassonne;
using ExtraMath;
using Godot;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

[Tool]
public class NameDialog : WindowDialog
{
    public Func<string, string> PostProcessHandle = s => s;
    public Action<string> CompleteHandle = null;
    Func<string, (bool res, string msg)> _isValid = s => (s != null && s.Length > 0, "Input at least one character.");
    public Func<string, (bool res, string msg)> ChangedHandle { get => _isValid; set { _isValid = value; if (_nameEdit != null) CheckValidity(_nameEdit.Text); } }
    [Export]
    public Color ValidColor = new Color(0.3f, 1.0f, 0.3f);
    [Export]
    public Color InvalidColor = new Color(1.0f, 0.3f, 0.3f);
    void CheckValidity(string text)
    {
        if (text == null)
            text = "";
        (bool res, string msg) = ChangedHandle(text);
        _confirmButton.Disabled = !res;
        if (msg != null)
        {
            _stateLabel.Text = msg;
            _stateLabel.AddColorOverride("font_color", (res) ? ValidColor : InvalidColor);
        }
        else
            _stateLabel.Text = "";
    }
    void ButtonPressed()
    {
        var text = _nameEdit.Text;
        CheckValidity(text);
        if (_confirmButton.Disabled)
            return;
        Reset();
        Hide();
        if (CompleteHandle != null)
        {
            CompleteHandle(PostProcessHandle(text));
        }
        else
            GD.PrintErr("NameDialog has no Complete handle");
    }
    void EnterPressed(string text)
    {
        if (!_confirmButton.Disabled)
            ButtonPressed();
    }
    public void Reset()
    {
        var cont = this.FindChild<VBoxContainer>();
        _nameEdit = cont.FindChild<LineEdit>();
        _nameEdit.Text = "";
        _confirmButton = cont.FindChild<Button>();
        _stateLabel = cont.FindChild<Label>();
        _stateLabel.Text = "";
        if (!_nameEdit.IsConnected("text_changed", this, "CheckValidity"))
            _nameEdit.Connect("text_changed", this, "CheckValidity");
        if (!_confirmButton.IsConnected("pressed", this, "ButtonPressed"))
            _confirmButton.Connect("pressed", this, "ButtonPressed");
        if (!_nameEdit.IsConnected("text_entered", this, "EnterPressed"))
            _nameEdit.Connect("text_entered", this, "EnterPressed");

        CheckValidity(_nameEdit.Text);

    }
    public void AboutToShow()
    {
        _nameEdit.GrabFocus();
        _nameEdit.GrabClickFocus();
        _nameEdit.CallDeferred("grab_focus");
    }
    LineEdit _nameEdit;
    Button _confirmButton;
    Label _stateLabel;

    public override void _Ready()
    {
        this.GetCloseButton().Disabled = true;
        this.PopupExclusive = true;
        Reset();
        Connect("about_to_show", this, "AboutToShow");
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
