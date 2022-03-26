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
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using ExtraMath;
using Godot;
using Newtonsoft.Json;
using static System.Math;
using static Utils;
using Expression = System.Linq.Expressions.Expression;

[Tool]
public class PropBanner : Spatial, IPropElement, IExtendedProperties
{
    MeshInstance _bannerMeshInstance;

    Action IPropElement.OnChangeHandle { get; set; }
    Dictionary<string, (Func<object> getter, Action<object> setter, Func<bool> predicate, GDProperty prop)>
        IExtendedProperties.Actions
    { get; set; } =
            new Dictionary<string, (Func<object> getter, Action<object> setter, Func<bool> predicate, GDProperty prop)>();
    bool _billboard = true;
    [Export]
    public bool Billboard { get => _billboard; set { if (_billboard != value) { _billboard = value; Refresh(); } } }

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
                Refresh();
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
                Refresh();
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
                Refresh();
            }
        }
    }
    bool _showIcon = false;
    [Export]
    public bool ShowIcon { get => _showIcon; set { if (ShowIcon != value) { _showIcon = value; Refresh(); } } }
    Vector2 _iconOffset = new Vector2(0, 0);
    public Vector2 IconOffset
    {
        get => _iconOffset;
        set
        {
            if (IconOffset != value)
            {
                _iconOffset = new Vector2(Clamp(value.x, -1, 1), Clamp(value.y, -1, 1));
                Refresh();
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
                Refresh();
            }
        }
    }
    int _background = 0;
    [Export(PropertyHint.Enum, "None,Primary,Secondary,Tertiary")]
    public int Background
    {
        get => _background;
        set
        {
            if (_background != value)
            {
                if (value != 0 && _background == 0)
                    _backgroundOpacity = 1.0f;
                _background = value;

                Refresh();
            }
        }
    }
    float _backgroundOpacity = 0;
    public float BackgroundOpacity
    {
        get => _backgroundOpacity;
        set
        {
            if (_backgroundOpacity != value)
            {
                _backgroundOpacity = value;
                Refresh();
            }
        }
    }
    int _content = 0;
    [Export(PropertyHint.Enum, "Nothing,Image,Animation")]
    public int Content
    {
        get => _content;
        set
        {
            if (_content != value)
            {
                _content = value;
                Refresh();
            }
        }
    }
    public class ImageContent
    {
        public Texture _texture;
        public bool _maskEnabled = false;
        public Texture _mask;
    }
    public class AnimatedContent
    {
        public SpriteFrames _frames;
        public bool _looping = true;
        public bool _maskEnabled = false;
        public bool _animatedMask = false;
        public object _mask;
    }
    object _contentData;
    bool refreshing = false;
    void StartAnimation()
    {

    }
    void StopAnimation()
    {

    }
    public void Refresh()
    {
        if (refreshing)
            return;
        refreshing = true;
        Defer(() => RealRefresh());
    }
    void SetTheme(PersonalTheme t)
    {
        var mat = (ShaderMaterial)_bannerMeshInstance.GetActiveMaterial(0);
        if (ShowIcon)
            t.SetFullShader(mat, true, IconScale, IconOffset);
        else
            t.SetFullShader(mat, false);
        mat.SetShaderParam(Constants.SHADER_PRIMARY_ENABLED, PrimaryEnabled);
        mat.SetShaderParam(Constants.SHADER_SECONDARY_ENABLED, SecondaryEnabled);
        mat.SetShaderParam(Constants.SHADER_TERTIARY_ENABLED, TertiaryEnabled);
        mat.SetShaderParam(Constants.SHADER_BILLBOARD_ENABLED, Billboard);
        mat.SetShaderParam("albedo", Background switch
        {
            0 => Colors.Transparent,
            1 => t.PrimaryColor,
            2 => t.SecondaryColor,
            3 => t.TertiaryColor,
            _ => throw new InvalidOperationException("This should not happen"),
        });
        var ep = (IExtendedProperties)this;
        ep.RemoveProperty("Texture", true);
        ep.RemoveProperty("MaskEnabled", true);
        ep.RemoveProperty("Mask", true);
        ep.RemoveProperty("Frames", true);
        ep.RemoveProperty("AnimatedMask", true);
        ep.RemoveProperty("Loop", true);
        ep.RemoveProperty("BackgroundOpacity", true);
        if (Background != 0)
        {
            ep.AddProperty(new GDProperty("BackgroundOpacity", Variant.Type.Real, PropertyHint.ExpRange, "0.0,1.0"),
                () => BackgroundOpacity, o => BackgroundOpacity = (float)o);
        }
        else
            _backgroundOpacity = 0.0f;

        mat.SetShaderParam(Constants.SHADER_BACKGROUND_OPACITY, BackgroundOpacity);

        if (Content == 0)
        {
            _contentData = null;
            mat.SetShaderParam(Constants.SHADER_TEXTURE_ENABLED, false);
        }
        else if (Content == 1) // Sprite
        {
            ImageContent ic;
            if (!(_contentData is ImageContent))
            {
                ic = new ImageContent();
                if (_contentData is AnimatedContent ac)
                {
                    if (ac._maskEnabled && !ac._animatedMask)
                    {
                        ic._maskEnabled = true;
                        ic._mask = ac._mask as Texture;
                    }
                }
                _contentData = ic;
            }
            ic = (ImageContent)_contentData;
            mat.SetShaderParam(Constants.SHADER_TEXTURE_ENABLED, true);
            mat.SetShaderParam("texture_albedo", ic._texture);
            mat.SetShaderParam(Constants.SHADER_MASK_ENABLED_THEME_SETTER, ic._maskEnabled);
            ep.AddProperty(new GDProperty("Texture", Variant.Type.Object, PropertyHint.ResourceType, "Texture"),
                () => (_contentData as ImageContent)._texture, o => (_contentData as ImageContent)._texture = (Texture)o);
            ep.AddProperty(new GDProperty("MaskEnabled", Variant.Type.Bool, PropertyHint.None),
                () => (_contentData as ImageContent)._maskEnabled, o => (_contentData as ImageContent)._maskEnabled = (bool)o);
            if (ic._maskEnabled)
            {
                mat.SetShaderParam(Constants.SHADER_MASK_TEXTURE_THEME_SETTER, ic._mask);
                ep.AddProperty(new GDProperty("Mask", Variant.Type.Object, PropertyHint.ResourceType, "Texture"),
                    () => (_contentData as ImageContent)._mask, o => (_contentData as ImageContent)._mask = (Texture)o);
            }
        }
        else if (Content == 2) // AnimatedSprite
        {
            if (!(_contentData is AnimatedContent))
            {
                var ac = new AnimatedContent();
                if (_contentData is ImageContent ic)
                {
                    ac._maskEnabled = ic._maskEnabled;
                    if (ic._maskEnabled)
                    {
                        ac._animatedMask = false;
                        ac._mask = ic._mask;
                    }
                }
                _contentData = ac;
            }
            ep.AddProperty(new GDProperty("Frames", Variant.Type.Object, PropertyHint.ResourceType, "SpriteFrames"),
                () => (_contentData as AnimatedContent)._frames, o => (_contentData as AnimatedContent)._frames = (SpriteFrames)o);
            ep.AddProperty(new GDProperty("MaskEnabled", Variant.Type.Bool, PropertyHint.None),
                () => (_contentData as AnimatedContent)._maskEnabled, o => (_contentData as AnimatedContent)._maskEnabled = (bool)o);
            if ((_contentData as AnimatedContent)._maskEnabled)
            {
                ep.AddProperty(new GDProperty("AnimatedMask", Variant.Type.Bool, PropertyHint.None),
                    () => (_contentData as AnimatedContent)._animatedMask, o => (_contentData as AnimatedContent)._animatedMask = (bool)o);
                ep.AddProperty
                (
                    new GDProperty("Mask", Variant.Type.Object, PropertyHint.ResourceType,
                    ((_contentData as AnimatedContent)._animatedMask) ? "SpriteFrames" : "Texture"),
                    () =>
                    {
                        return (_contentData as AnimatedContent)._mask;
                    },
                    o =>
                    {
                        (_contentData as AnimatedContent)._mask = o;
                    }
                );
            }
            ep.AddProperty(new GDProperty("Loop", Variant.Type.Bool, PropertyHint.None),
                () => (_contentData as AnimatedContent)._looping, o => (_contentData as AnimatedContent)._looping = (bool)o);
            StartAnimation();
        }

        PropertyListChangedNotify();
    }
    void RealRefresh()
    {
        refreshing = false;
        if ((this as IPropElement).OnChangeHandle != null)
            (this as IPropElement).OnChangeHandle();
        else if (Engine.EditorHint)
        {
            if (Globals.DefaultTheme == null)
                Globals.DefaultTheme = DeserializeFromFile<PersonalTheme>(Constants.DEFAULT_PLAYER_THEME_PATH);
            SetTheme(Globals.DefaultTheme);
        }
    }
    public override void _Notification(int what)
    {
        if (what == NotificationVisibilityChanged)
        {
            if (Content == 2)
            {
                if (Visible)
                    StartAnimation();
                else
                    StopAnimation();
            }
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
        return (this as IExtendedProperties).OnGetPropertyList();
    }
    List<Action<PersonalTheme>> IPropElement.GetThemeSetters()
    {
        return new List<Action<PersonalTheme>>() { SetTheme };
    }
    public override void _Ready()
    {
        _bannerMeshInstance = this.GetNodeSafe<MeshInstance>("BannerMesh");
        Refresh();
    }


    public PropBanner()
    {
        var iep = (this as IExtendedProperties);
        iep.AddProperty(new GDProperty("IconScale", Variant.Type.Vector2, PropertyHint.None),
            () => IconScale, o => IconScale = (Vector2)o, () => ShowIcon);
        iep.AddProperty(new GDProperty("IconOffset", Variant.Type.Vector2, PropertyHint.None),
            () => IconOffset, o => IconOffset = (Vector2)o, () => ShowIcon);
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
