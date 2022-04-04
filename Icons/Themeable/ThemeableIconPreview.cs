// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using ExtraMath;
using Godot;
using Newtonsoft.Json;
using static System.Math;
using static Utils;
using Expression = System.Linq.Expressions.Expression;

public class ThemeableIconPreview : Control
{
    Node2D _rawViewRoot;
    Sprite _rawSprite;
    Node2D _effectViewRoot;
    Sprite _effectSprite;
    Label _nameLabel;
    Label _sizeLabel;
    Button _loadButton;
    Button _loadThemeButton;
    ThemeSingleColorPicker _primaryColorPicker;
    ThemeSingleColorPicker _secondaryColorPicker;
    ThemeSingleColorPicker _tertiaryColorPicker;
    Control _mainControlContainer;
    Texture _currentTexture;
    FileDialog _dialog;
    readonly PackedScene _packedFileDialog = ResourceLoader.Load<PackedScene>("res://UI/FileDialog.tscn");
    System.Threading.Thread _transformThread;
    readonly AutoResetEvent _ev = new AutoResetEvent(false);
    PersonalTheme _lastTheme;
    Texture _lastTexture;
    // While I am fairly certain  that this is thread-safe due to handles being atomic, I am not 100% on this.
    void TransformTextureThread()
    {
        while (true)
        {
            _ev.WaitOne();
            Texture tex = _lastTheme.TransformTexture(_lastTexture);
            Defer(() => _effectSprite.Texture = tex);
        }
    }
    void TransformTexutre(Texture tex, PersonalTheme theme)
    {
        if (_transformThread == null)
        {
            _transformThread = new System.Threading.Thread(TransformTextureThread);
            _transformThread.Start();
        }
        _lastTheme = theme;
        _lastTexture = tex;
        _ev.Set();
    }
    void SetInput(bool enabled)
    {
        GetChildrenRecrusively<Button>(_mainControlContainer).ForEach(it => it.Disabled = !enabled);
    }
    void DestroyIcons()
    {
        if (_effectSprite != null)
        {
            DestroyNode(_effectSprite);
            _effectSprite = null;
        }
        if (_rawSprite != null)
        {
            DestroyNode(_rawSprite);
            _rawSprite = null;
        }
    }
    void UpdateSize()
    {
        if (_currentTexture == null)
            return;
        Camera2D[] cams = new Camera2D[] { _rawViewRoot.GetNode<Camera2D>("Camera2D"), _effectViewRoot.GetNode<Camera2D>("Camera2D") };
        foreach (var it in cams)
        {
            //it.Position = _currentTexture.GetSize() * new Vector2(-0.5f, 0.5f);
            var zv = _currentTexture.GetSize() / it.GetViewport().Size;
            float z = Max(zv.x, zv.y);
            it.Zoom = new Vector2(z, z);
        }
    }
    void CreateIcons()
    {
        _rawSprite = new Sprite();
        _rawSprite.Texture = _currentTexture;
        _rawViewRoot.AddChild(_rawSprite);
        _effectSprite = new Sprite();
        _effectViewRoot.AddChild(_effectSprite);

        UpdateSize();
    }
    void LoadIcon(string path)
    {
        OnPopupClose();
        _currentTexture = null;
        DestroyIcons();
        if (!FileExists(path))
        {
            GD.PrintErr("File does not exist: " + path);
            return;
        }
        try
        {
            _currentTexture = ResourceLoader.Load<Texture>(path);
        }
        catch (Exception ex)
        {
            GD.PrintErr("Could not load texture from: " + path + " Error: " + ex.Message);
            return;
        }
        if (_currentTexture == null)
        {
            GD.PrintErr("Could not load texture from: " + path);
            return;
        }
        _nameLabel.Text = path.BaseName();
        _sizeLabel.Text = $"{_currentTexture.GetSize().x}x{_currentTexture.GetSize().y}";
        CreateIcons();
        UpdateColors();
    }
    void LoadTheme(string path)
    {
        OnPopupClose();

        PersonalTheme theme;

        if (!FileExists(path))
        {
            GD.PrintErr("File does not exist: " + path);
            return;
        }
        try
        {
            theme = DeserializeFromFile<PersonalTheme>(path);
        }
        catch (Exception ex)
        {
            GD.PrintErr("Error deserializing file: " + path + " ERROR: " + ex.Message);
            return;
        }
        if (theme == null)
        {
            GD.PrintErr("Error deserializing file: " + path);
            return;
        }

        _primaryColorPicker.PickedColor = theme.PrimaryColor;
        _secondaryColorPicker.PickedColor = theme.SecondaryColor;
        _tertiaryColorPicker.PickedColor = theme.TertiaryColor;

    }
    void OnPopupClose()
    {
        SetInput(true);
        if (_dialog != null)
            DestroyNode(_dialog);
    }
    void OnLoadPressed()
    {
        SetInput(false);
        if (_dialog != null)
            DestroyNode(_dialog);
        _dialog = _packedFileDialog.Instance<FileDialog>();
        _dialog.CurrentDir = Constants.THEMEABLE_ICONS_DIRECTORY;
        _dialog.Filters = new string[]
        {
            "*.png ; PNG images",
            "*.bmp ; BMP images"
        };

        _dialog.Mode = FileDialog.ModeEnum.OpenFile;
        _dialog.Connect("file_selected", this, nameof(LoadIcon));
        _dialog.Connect("popup_hide", this, nameof(OnPopupClose));
        GetTree().Root.AddChild(_dialog);
        _dialog.CallDeferred("show");
    }
    void OnLoadThemePressed()
    {
        SetInput(false);
        var diag = _packedFileDialog.Instance<FileDialog>();
        diag.CurrentDir = Constants.PLAYER_THEMES_DIRECTORY;
        diag.Filters = new string[]
        {
            "*.json ; THEMES"
        };
        diag.Mode = FileDialog.ModeEnum.OpenFile;

        _dialog.Connect("file_selected", this, nameof(LoadTheme));
        _dialog.Connect("popup_hide", this, nameof(OnPopupClose));
        AddChild(diag);
        diag.Show();
    }
    void UpdateColors()
    {
        if (_currentTexture == null)
            return;
        var theme = new PersonalTheme();
        theme.PrimaryColor = _primaryColorPicker.PickedColor;
        theme.SecondaryColor = _secondaryColorPicker.PickedColor;
        theme.TertiaryColor = _tertiaryColorPicker.PickedColor;
        TransformTexutre(_currentTexture, theme);
    }
    public override void _Ready()
    {
        _rawViewRoot = GetNode<Node2D>("HBoxContainer/HSplitContainer/VSplitContainer/ViewportContainer/RawViewViewport/RawViewRoot");

        _effectViewRoot = GetNode<Node2D>("HBoxContainer/HSplitContainer/VSplitContainer/ViewportContainer2/EffectViewViewport/EffectViewRoot");

        _mainControlContainer = GetNode<Control>("HBoxContainer/HSplitContainer/MainControlContainer");

        _nameLabel = GetNode<Label>("HBoxContainer/HSplitContainer/MainControlContainer/NameLabel");
        _sizeLabel = GetNode<Label>("HBoxContainer/HSplitContainer/MainControlContainer/SizeLabel");

        _loadButton = GetNode<Button>("HBoxContainer/HSplitContainer/MainControlContainer/LoadButton");
        _loadButton.Connect("pressed", this, nameof(OnLoadPressed));

        _loadThemeButton = GetNode<Button>("HBoxContainer/HSplitContainer/MainControlContainer/LoadThemeButton");
        _loadThemeButton.Connect("pressed", this, nameof(OnLoadThemePressed));

        _primaryColorPicker = GetNode<ThemeSingleColorPicker>("HBoxContainer/HSplitContainer/MainControlContainer/PrimaryContainer");
        _primaryColorPicker.DisplayedName = "Primary";
        _primaryColorPicker.OnColorChangedHandle = c => UpdateColors();

        _secondaryColorPicker = GetNode<ThemeSingleColorPicker>("HBoxContainer/HSplitContainer/MainControlContainer/SecondaryContainer");
        _secondaryColorPicker.DisplayedName = "Secondary";
        _secondaryColorPicker.OnColorChangedHandle = c => UpdateColors();

        _tertiaryColorPicker = GetNode<ThemeSingleColorPicker>("HBoxContainer/HSplitContainer/MainControlContainer/TertiaryContainer");
        _tertiaryColorPicker.DisplayedName = "Tertiary";
        _tertiaryColorPicker.OnColorChangedHandle = c => UpdateColors();

        LoadIcon(ConcatPaths(Constants.THEMEABLE_ICONS_DIRECTORY, "ThemableTest.png"));
    }

}
