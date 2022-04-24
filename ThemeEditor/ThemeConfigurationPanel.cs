

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using ExtraMath;
using Godot;
using Newtonsoft.Json;
using static System.Math;
using static Utils;

[Tool]
public class ThemeConfigurationPanel : Control
{
    PersonalTheme _currentTheme;
    public PersonalTheme CurrentTheme
    {
        get => _currentTheme;
        set
        {
            _currentTheme = value;
            CallDeferred("UpdateUI");
        }
    }
    public Action OnChangeHandle = () => { };
    FileDialog _loadFileDialog;
    ThemeSingleColorPicker _primaryPicker;
    ThemeSingleColorPicker _secondaryPicker;
    ThemeSingleColorPicker _tertiaryPicker;
    Button _selectIconButton;
    Button _selectAvatarButton;
    void UpdateUI()
    {
        _primaryPicker.OnColorChangedHandle = col =>
        {
            if (CurrentTheme != null)
            {
                CurrentTheme.PrimaryColor = col;
                OnChangeHandle();
            }
        };
        _secondaryPicker.OnColorChangedHandle = col =>
        {
            if (CurrentTheme != null)
            {
                CurrentTheme.SecondaryColor = col;
                OnChangeHandle();
            }
        };
        _tertiaryPicker.OnColorChangedHandle = col =>
        {
            if (CurrentTheme != null)
            {
                CurrentTheme.TertiaryColor = col;
                OnChangeHandle();
            }
        };
        if (CurrentTheme != null)
        {
            _primaryPicker.PickedColor = CurrentTheme.PrimaryColor;
            _secondaryPicker.PickedColor = CurrentTheme.SecondaryColor;
            _tertiaryPicker.PickedColor = CurrentTheme.TertiaryColor;
            _selectIconButton.Text = CurrentTheme.IconPath;
            _selectAvatarButton.Text = CurrentTheme.AvatarPath;
        }

    }
    Action<string> OnFileSelected = s => { };
    void IconSelected(string dir)
    {
        if (CurrentTheme != null && Utils.FileExists(dir))
        {
            CurrentTheme.IconPath = dir;
            UpdateUI();
            OnChangeHandle();
        }
    }
    void AvatarSelected(string dir)
    {
        if (CurrentTheme != null && Utils.FileExists(dir))
        {
            CurrentTheme.AvatarPath = dir;
            UpdateUI();
            OnChangeHandle();
        }
    }
    void SelectIconPressed()
    {
        OnFileSelected = IconSelected;
        _loadFileDialog.Show();
    }
    void SelectAvatarPressed()
    {
        OnFileSelected = AvatarSelected;
        _loadFileDialog.Show();
    }
    public override void _Ready()
    {
        _loadFileDialog = this.GetNodeSafe<FileDialog>("LoadFileDialog");
        _primaryPicker = this.GetNodeSafe<ThemeSingleColorPicker>("VBoxContainer/PrimaryColorPicker");
        _secondaryPicker = this.GetNodeSafe<ThemeSingleColorPicker>("VBoxContainer/SecondaryColorPicker");
        _tertiaryPicker = this.GetNodeSafe<ThemeSingleColorPicker>("VBoxContainer/TertiaryColorPicker");
        _selectIconButton = this.GetNodeSafe<Button>("VBoxContainer/IconPicker/SelectIconButton");
        _selectAvatarButton = this.GetNodeSafe<Button>("VBoxContainer/AvatarPicker/SelectAvatarButton");

        _selectIconButton.Connect("pressed", this, nameof(SelectIconPressed));
        _selectAvatarButton.Connect("pressed", this, nameof(SelectAvatarPressed));
        _loadFileDialog.OnFileSelected(s =>
        { // workaround for a partially implemented API
            if (s == null || s == "")
                s = _loadFileDialog.CurrentPath;
            this.OnFileSelected(s);
        });
    }
}
