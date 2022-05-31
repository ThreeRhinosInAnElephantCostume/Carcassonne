using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Threading;
using ExtraMath;
using Godot;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using static System.Math;
using static Utils;
using Expression = System.Linq.Expressions.Expression;
using Thread = System.Threading.Thread;

public class InGameMenu : Control, SaveLoadGame.ISaveLoadHandler
{
    Game _game;
    InGameUI _inGameUI;

    Control _saveLoadRoot;
    Control _mainRoot;
    SaveLoadGame _saveLoadGame;
    Button _resumeButton;
    Button _saveButton;
    Button _loadButton;
    Button _settingsButton;
    Button _exitButton;
    List<Control> Children = new List<Control>();
    List<BaseButton> Buttons = new List<BaseButton>();
    public Action OnResume = () => { };
    void OnHide()
    {
        if (Visible)
        {
            Visible = false;
            return;
        }
        Buttons.ForEach(it => it.Disabled = true);
    }
    void OnShow()
    {
        if (!Visible)
        {
            Visible = true;
            return;
        }
        Buttons.ForEach(it => it.Disabled = false);
    }
    public override void _Notification(int what)
    {
        if (what == NotificationVisibilityChanged)
        {
            if (Visible)
                OnShow();
            else
                OnHide();
        }
    }
    void OnResumePressed()
    {
        Visible = false;
        OnResume();
    }
    void OnSavePressed()
    {
        string basepath = (_game.IsMultiplayer) ? Constants.DataPaths.SAVES_MULTIPLAYER_PATH : Constants.DataPaths.SAVES_LOCAL_PATH;
        string subpath = ConcatPaths(basepath, _game.GameName);
        string name = $"TURN_{_game.Engine.Turn}_{GetFormattedTimeNow()}";
        EnsurePathExists(subpath);
        _saveLoadRoot.Visible = true;
        _saveLoadGame.StartSave(this, subpath, subpath, name);
    }
    void OnLoadPressed()
    {
        string basepath = (_game.IsMultiplayer) ? Constants.DataPaths.SAVES_MULTIPLAYER_PATH : Constants.DataPaths.SAVES_LOCAL_PATH;
        string subpath = ConcatPaths(basepath, _game.GameName);
        EnsurePathExists(subpath);
        _saveLoadRoot.Visible = true;
        _saveLoadGame.StartLoad(this, basepath, subpath);
    }
    void OnSettingsPressed()
    {

    }
    void OnExitPressed()
    {

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
    bool SaveLoadGame.ISaveLoadHandler.CanDelete(string path)
    {
        return (IsFile(path));
    }

    void SaveLoadGame.ISaveLoadHandler.OnSelected(string path)
    {
        if (_saveLoadGame.Mode == SaveLoadGame.SLMode.Save)
        {
            Assert(!FileExists(path) || !IsDirectory(path), "Attempting to write over a directory");
            EnsurePathExists(path.GetBaseDir());
            _game.SaveToFile(path);
        }
        else
        {
            Assert(FileExists(path));
            _inGameUI.LoadGameFromFile(path);
        }
        _saveLoadRoot.Visible = false;
        OnResumePressed();
    }

    void SaveLoadGame.ISaveLoadHandler.OnDelete(string path)
    {
        throw new NotImplementedException();
    }

    void SaveLoadGame.ISaveLoadHandler.OnCancelled()
    {
        _saveLoadRoot.Visible = false;
    }
    public override void _Ready()
    {
        Visible = false;

        _mainRoot = this.GetNodeSafe<Control>("Panel/HBoxContainer");

        _resumeButton = this.GetNodeSafe<Button>
            ("Panel/HBoxContainer/Control2/VBoxContainer/Control2/VBoxContainer/ResumeButton");
        _resumeButton.OnButtonPressed(OnResumePressed);

        _saveButton = this.GetNodeSafe<Button>
            ("Panel/HBoxContainer/Control2/VBoxContainer/Control2/VBoxContainer/SaveButton");
        _saveButton.OnButtonPressed(OnSavePressed);

        _loadButton = this.GetNodeSafe<Button>
            ("Panel/HBoxContainer/Control2/VBoxContainer/Control2/VBoxContainer/LoadButton");
        _loadButton.OnButtonPressed(OnLoadPressed);

        _settingsButton = this.GetNodeSafe<Button>
            ("Panel/HBoxContainer/Control2/VBoxContainer/Control2/VBoxContainer/SettingsButton");
        _settingsButton.OnButtonPressed(OnSettingsPressed);

        _exitButton = this.GetNodeSafe<Button>
            ("Panel/HBoxContainer/Control2/VBoxContainer/Control2/VBoxContainer/ExitButton");
        _exitButton.OnButtonPressed(OnExitPressed);

        _saveLoadRoot = this.GetNodeSafe<Control>("Panel/SaveLoadRoot");
        _saveLoadRoot.Visible = false;

        _saveLoadGame = this.GetNodeSafe<SaveLoadGame>("Panel/SaveLoadRoot/SaveLoadGame");


        Children = _mainRoot.GetChildrenRecrusively<Control>();
        Buttons = _mainRoot.GetChildrenRecrusively<BaseButton>();
    }

}
