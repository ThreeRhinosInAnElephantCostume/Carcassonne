// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using static Constants.ShaderParams;
using static Constants.ShaderParams.ThemableShader;
using static Utils;
using Expression = System.Linq.Expressions.Expression;

[Tool]
public class TextureButtonProp : TextureButton, IProp, IExtendedProperties
{
    bool _dirty = true;

    PersonalTheme IProp._theme { get; set; } = null;
    string IProp._examplePlayerTheme { get; set; } = "";
    List<IProp> IProp._children { get; set; } = new List<IProp>();
    IProp IProp._parent { get; set; }
    Dictionary<string, (Func<object> getter, Action<object> setter, Func<bool> predicate, GDProperty prop)> IExtendedProperties.ExtendedProperties { get; set; }
    List<string> IExtendedProperties.ExtendedPropertiesOrdered { get; set; }
    Texture _mask = null;
    [Export]
    public Texture Mask
    {
        get => _mask; set
        {
            _mask = value;
            RealRefresh();
        }
    }
    bool _primaryEnabled = true;
    [Export]
    public bool PrimaryEnabled
    {
        get => _primaryEnabled;
        set
        {
            if (_primaryEnabled != value)
            {
                _primaryEnabled = value;
                ShaderUpdate();
            }
        }
    }
    bool _secondaryEnabled = true;
    [Export]
    public bool SecondaryEnabled
    {
        get => _secondaryEnabled;
        set
        {
            if (_secondaryEnabled != value)
            {
                _secondaryEnabled = value;
                ShaderUpdate();
            }
        }
    }
    bool _tertiaryEnabled = true;
    [Export]
    public bool TertiaryEnabled
    {
        get => _tertiaryEnabled;
        set
        {
            if (_tertiaryEnabled != value)
            {
                _tertiaryEnabled = value;
                ShaderUpdate();
            }
        }
    }
    int _backgroundColor = 0;
    [Export(PropertyHint.Enum, "None,Primary,Secondary,Tertiary")]
    public int BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            if (value != 0 && _backgroundColor == 0 && _backgroundOpacity == 0.0f)
                _backgroundOpacity = 1.0f;
            _backgroundColor = value;
            ShaderUpdate();
            Refresh();
        }
    }
    public float _backgroundOpacity = 1.0f;
    public float BackgroundOpacity
    {
        get => _backgroundOpacity;
        set
        {
            _backgroundOpacity = value;
            ShaderUpdate();
        }
    }
    bool _showIcon = false;
    public bool ShowIcon { get => _showIcon; set { if (ShowIcon != value) { _showIcon = value; Refresh(); } } }
    bool _centerIcon = true;
    public bool CenterIcon { get => _centerIcon; set { if (_centerIcon != value) { _centerIcon = value; ShaderUpdate(); } } }
    Vector2 _iconOffset = new Vector2(0, 0);
    public Vector2 IconOffset
    {
        get => _iconOffset;
        set
        {
            if (IconOffset != value)
            {
                _iconOffset = new Vector2(Clamp(value.x, -1, 1), Clamp(value.y, -1, 1));
                ShaderUpdate();
            }
        }
    }
    Vector2 _iconScale = new Vector2(1, 1);
    public Vector2 IconScale
    {
        get => _iconScale;
        set
        {
            if (_iconScale != value)
            {
                _iconScale = new Vector2(Clamp(value.x, -1, 1), Clamp(value.y, -1, 1));
                ShaderUpdate();
            }
        }
    }
    void IProp.UpdateTheme()
    {
        var prop = (IProp)this;
        if (prop.CurrentTheme == null)
        {
            if (Engine.EditorHint && Globals.DefaultTheme != null)
            {
                prop.CurrentTheme = Globals.DefaultTheme;
            }
            return;
        }
        ShaderUpdate();
    }
    ShaderMaterial GetShaderMaterial()
    {
        if (this.Material == null || !(this.Material is ShaderMaterial))
        {
            var shad = new ShaderMaterial();
            shad.Shader = ResourceLoader.Load<Shader>(Constants.AssetPaths.SPRITE2D_PROP_SHADER); ;
            this.Material = shad;
        }
        this.Material.ResourceLocalToScene = true;
        return (ShaderMaterial)this.Material;
    }
    public void ShaderUpdate()
    {
        var prop = (IProp)this;
        var mat = GetShaderMaterial();
        if (prop.CurrentTheme != null)
        {
            if (ShowIcon)
                prop.CurrentTheme.SetFullShader(mat, true, IconScale, IconOffset);
            else
                prop.CurrentTheme.SetFullShader(mat, false);
        }
        if (BackgroundColor == 0 || prop.CurrentTheme == null) // None
        {
            mat.SetShaderParam(SHADER_BACKGROUND_OPACITY, 0.0f);
        }
        else
        {
            mat.SetShaderParam(SHADER_BACKGROUND_OPACITY, BackgroundOpacity);
            var col = (BackgroundColor) switch
            {
                1 => prop.CurrentTheme.PrimaryColor, // primary
                2 => prop.CurrentTheme.SecondaryColor, // secondary
                3 => prop.CurrentTheme.TertiaryColor, // tertiary
                _ => throw new Exception("Impossible color selection")
            };
            mat.SetShaderParam(SHADER_BACKGROUND_COLOR, col);
        }
        mat.SetShaderParam(SHADER_MASK_ENABLED_THEME_SETTER, Mask != null);
        mat.SetShaderParam(SHADER_MASK_TEXTURE_THEME_SETTER, Mask);
        mat.SetShaderParam(SHADER_PRIMARY_ENABLED, PrimaryEnabled);
        mat.SetShaderParam(SHADER_SECONDARY_ENABLED, SecondaryEnabled);
        mat.SetShaderParam(SHADER_TERTIARY_ENABLED, TertiaryEnabled);
        mat.SetShaderParam(SHADER_ICON_CENTERED, CenterIcon);
    }
    void RealRefresh()
    {
        PropertyListChangedNotify();
        (this as IProp).UpdateTheme();
        (this as IExtendedProperties).ReloadProperties();
    }
    public void Refresh()
    {
        Defer(RealRefresh);
    }

    public override void _Ready()
    {
        (this as IProp).InitHierarchy();
    }
    public override void _Process(float delta)
    {
        if (_dirty)
        {
            _dirty = false;
            (this as IProp).UpdateProp();
        }
    }
    public override object _Get(string property)
    {
        object ret = null;
        if (!(this as IExtendedProperties).OnGet(property, out ret))
            return base._Get(property);
        return ret;
    }
    public override bool _Set(string property, object value)
    {
        if (!(this as IExtendedProperties).OnSet(property, value))
            return base._Set(property, value);
        return true;
    }
    public override Godot.Collections.Array _GetPropertyList()
    {
        var ep = (this as IExtendedProperties);
        if (!ep.IsValid() || ep.ExtendedProperties.Count == 0)
            RealRefresh();
        return ep.OnGetPropertyList();
    }

    void IExtendedProperties.LoadProperties()
    {
        var ep = (IExtendedProperties)this;

        if (BackgroundColor != 0)
        {
            ep.AddProperty(new GDProperty("BackgroundTransparency", Variant.Type.Real, PropertyHint.Range, "0,1"),
                () => BackgroundOpacity, o => BackgroundOpacity = (float)o);
        }
        ep.AddProperty(new GDProperty("ShowIcon", Variant.Type.Bool, PropertyHint.None),
            () => ShowIcon, o => ShowIcon = (bool)o);
        if (ShowIcon)
        {
            ep.AddProperty(new GDProperty("IconCentered", Variant.Type.Bool, PropertyHint.None),
                () => CenterIcon, o => CenterIcon = (bool)o);
            ep.AddProperty(new GDProperty("IconOffset", Variant.Type.Vector2, PropertyHint.None),
                () => IconOffset, o => IconOffset = (Vector2)o);
            ep.AddProperty(new GDProperty("IconScale", Variant.Type.Vector2, PropertyHint.None),
                () => IconScale, o => IconScale = (Vector2)o);
        }
        PropertyListChangedNotify();
    }
}
