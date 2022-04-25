

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
using static Constants.ShaderParams.ThemableShader;
using static Utils;
using Expression = System.Linq.Expressions.Expression;

[Tool]
public class BannerProp : Spatial, IProp, IExtendedProperties
{
    MeshInstance _bannerMeshInstance = null;
    MeshInstance BannerMeshInstance
    {
        get
        {
            if (_bannerMeshInstance == null)
                UpdateBannerMesh();
            return _bannerMeshInstance;
        }
        set => _bannerMeshInstance = value;
    }

    Dictionary<string, (Func<object> getter, Action<object> setter, Func<bool> predicate, GDProperty prop)>
        IExtendedProperties.ExtendedProperties
    { get; set; }
    List<string> IExtendedProperties.ExtendedPropertiesOrdered { get; set; }
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
    bool _showIcon = false;
    public bool ShowIcon { get => _showIcon; set { if (ShowIcon != value) { _showIcon = value; Refresh(); } } }
    bool _transformIcon = true;
    public bool TransformIcon
    {
        get => _transformIcon;
        set
        {
            _transformIcon = value;
            ShaderUpdate();
        }
    }
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
                ShaderUpdate();
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

    PersonalTheme IProp._theme { get; set; }
    string IProp._examplePlayerTheme { get; set; }
    List<IProp> IProp._children { get; set; } = new List<IProp>();
    IProp IProp._parent { get; set; }

    public class ImageContent
    {
        public Texture _texture;
        public bool _maskEnabled = false;
        public Texture _mask;
    }
    public class AnimatedContent
    {
        public SpriteFrames _frames;
        public bool _maskEnabled = false;
        public bool _animatedMask = false;
        public object _mask;
        public string _currentAnimation = "default";
        public int _currentFrame = 0;
        public float _frameTime = 0.0f;
        public bool _running = false;
        public Texture[] textures;
    }
    object _contentData;
    bool refreshing = false;
    public void StartAnimation(string name = null)
    {
        Assert(_contentData is AnimatedContent, "Not set-up for animation");
        var ic = (AnimatedContent)_contentData;
        if (name == null)
        {
            if (ic._currentAnimation == null || ic._currentAnimation == "")
                ic._currentAnimation = "default";
            name = ic._currentAnimation;
            ic.textures = null;
        }
        else if (name != ic._currentAnimation || Engine.EditorHint) // invalidate textures if necessary
            ic.textures = null;
        if (ic._frames == null)
        {
            var mat = GetShaderMaterial();
            mat.SetShaderParam(SHADER_TEXTURE_ENABLED, false);
            return;
        }
        Assert<KeyNotFoundException>(ic._frames.GetAnimationNames().Contains(name), "Animation not found!");
        ic._currentAnimation = name;
        ic._currentFrame = 0;
        ic._frameTime = 0.0f;
        ic._running = true;
        if (ic._frames.GetFrameCount(name) > 0)
        {
            if ((ic.textures == null || ic.textures.Length != ic._frames.GetFrameCount(name)))
            {
                var mat = GetShaderMaterial();
                var textures = new Texture[ic._frames.GetFrameCount(name)];
                for (int i = 0; i < ic._frames.GetFrameCount(name); i++)
                {
                    textures[i] = ic._frames.GetFrame(name, i);
                }
                ic.textures = textures;
                var ta = new TextureArray();
                ta.Create((uint)textures[0].GetWidth(), (uint)textures[0].GetHeight(), (uint)textures.Length,
                    textures[0].GetData().GetFormat(), textures[0].Flags);
                for (int i = 0; i < textures.Length; i++)
                {
                    ta.SetLayerData(textures[i].GetData(), i);
                }
                mat.SetShaderParam(SHADER_SPRITE_ARRAY, ta);
            }
            TickAnimation(0.0f);
        }
    }
    void StopAnimation()
    {
        var ic = (AnimatedContent)_contentData;
        ic._running = false;
        TickAnimation(0.0f);
    }
    public void TickAnimation(float delta)
    {
        var ic = (AnimatedContent)_contentData;
        var mat = GetShaderMaterial();
        var frames = ic._frames;
        string anim = ic._currentAnimation;
        if (frames == null || !ic._running)
        {
            mat.SetShaderParam(SHADER_TEXTURE_ENABLED, false);
            return;
        }
        Assert(frames.HasAnimation(anim), "Animation " + anim + " not found");
        float speed = 1.0f / frames.GetAnimationSpeed(anim);
        ic._frameTime += delta;
        if (ic._frameTime > speed)
        {
            ic._frameTime = 0.0f;
            ic._currentFrame++;
        }
        if (ic._currentFrame >= frames.GetFrameCount(anim))
        {
            if (frames.GetAnimationLoop(anim))
                StartAnimation(anim);
            else
                StopAnimation();
        }
        mat.SetShaderParam(SHADER_TEXTURE_ENABLED, true);
        mat.SetShaderParam("texture_albedo", null);
        mat.SetShaderParam(SHADER_SPRITE_INDEX, ic._currentFrame);
        mat.SetShaderParam(SHADER_MASK_ENABLED_THEME_SETTER, ic._maskEnabled && ic._mask != null);
        if (ic._maskEnabled)
        {
            Texture mask = null;
            if (ic._animatedMask)
            {
                var mframes = ic._mask as SpriteFrames;
                if (mframes != null)
                {
                    if (mframes.HasAnimation(anim) && mframes.GetFrameCount(anim) < ic._currentFrame)
                    {
                        mask = mframes.GetFrame(anim, ic._currentFrame);
                    }
                }
            }
            else
            {
                mask = ic._mask as Texture;
            }
            if (mask == null)
                mat.SetShaderParam(SHADER_MASK_ENABLED_THEME_SETTER, false);
            else
            {
                mat.SetShaderParam(SHADER_MASK_TEXTURE_THEME_SETTER, mask);
            }
        }
    }
    public void Refresh()
    {
        if (refreshing)
            return;
        Defer(() => RealRefresh());
        refreshing = true;
    }
    ShaderMaterial GetShaderMaterial()
    {
        Assert(BannerMeshInstance != null, "_bannerMeshInstance is null! Invalid init?");
        Assert(BannerMeshInstance.GetSurfaceMaterialCount() > 0, "Error no surface materials!");
        var rawmat = BannerMeshInstance.GetActiveMaterial(0);
        Assert(rawmat != null, "The only surface material is null!");
        Assert(rawmat is ShaderMaterial, "Material not set to a shadermaterial.");
        var mat = (ShaderMaterial)BannerMeshInstance.GetActiveMaterial(0);
        Assert(mat.Shader != null, "Shader not set!");
        return mat;
    }
    void RealShaderUpdate()
    {
        var mat = GetShaderMaterial();
        mat.SetShaderParam(SHADER_PRIMARY_ENABLED, PrimaryEnabled);
        mat.SetShaderParam(SHADER_SECONDARY_ENABLED, SecondaryEnabled);
        mat.SetShaderParam(SHADER_TERTIARY_ENABLED, TertiaryEnabled);
        mat.SetShaderParam(SHADER_BILLBOARD_ENABLED, Billboard);
        mat.SetShaderParam(SHADER_BACKGROUND_OPACITY, BackgroundOpacity);
        if (ShowIcon)
        {
            mat.SetShaderParam(SHADER_ICON_SCALE_THEME_SETTER, IconScale);
            mat.SetShaderParam(SHADER_ICON_OFFSET_THEME_SETTER, IconOffset);
            mat.SetShaderParam(SHADER_ICON_CENTERED, CenterIcon);
        }
    }
    void ShaderUpdate()
    {
        Defer(RealShaderUpdate);
    }
    void SetTheme(PersonalTheme t)
    {
        if (t == null || BannerMeshInstance == null)
        {
            return;
        }
        var ep = (IExtendedProperties)this;
        var mat = GetShaderMaterial();
        mat.SetShaderParam("albedo", Background switch
        {
            0 => Colors.Transparent,
            1 => t.PrimaryColor,
            2 => t.SecondaryColor,
            3 => t.TertiaryColor,
            _ => throw new InvalidOperationException("This should not happen"),
        });
        if (ShowIcon)
        {
            t.SetFullShader(mat, true, IconScale, IconOffset);
            mat.SetShaderParam(SHADER_ICON_TRANSFORM, TransformIcon);
        }
        else
            t.SetFullShader(mat, false);
        if (Background == 0)
            _backgroundOpacity = 0.0f;


        if (Content == 0)
        {
            _contentData = null;
            mat.SetShaderParam(SHADER_TEXTURE_ENABLED, false);
            mat.SetShaderParam(SHADER_SPRITE_INDEX, -1);
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
            mat.SetShaderParam(SHADER_TEXTURE_ENABLED, ic._texture != null);
            mat.SetShaderParam(SHADER_SPRITE_INDEX, -1);
            mat.SetShaderParam("texture_albedo", ic._texture);
            mat.SetShaderParam(SHADER_MASK_ENABLED_THEME_SETTER, ic._maskEnabled && ic._mask != null);
            if (ic._maskEnabled)
            {
                mat.SetShaderParam(SHADER_MASK_TEXTURE_THEME_SETTER, ic._mask);
            }
        }
        else if (Content == 2) // AnimatedSprite
        {
            AnimatedContent ac;
            if (!(_contentData is AnimatedContent))
            {
                ac = new AnimatedContent();
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
            ac = (AnimatedContent)_contentData;
            if (ac._mask != null)
            {
                if (ac._maskEnabled)
                {
                    if ((ac._animatedMask && !(ac._mask is SpriteFrames)) || (!ac._animatedMask && (ac._mask is SpriteFrames)))
                    {
                        ac._mask = null;
                    }
                }
                else
                    ac._mask = null;
            }
            StartAnimation();
        }
        ShaderUpdate();
        ep.ReloadProperties();
        PropertyListChangedNotify();
    }
    void RealRefresh()
    {
        refreshing = false;
        if (Engine.EditorHint)
        {
            if (Globals.DefaultTheme == null)
                Globals.DefaultTheme = DeserializeFromFile<PersonalTheme>(Constants.DataPaths.DEFAULT_PLAYER_THEME_PATH);
            if ((this as IProp).CurrentTheme == null)
                (this as IProp).CurrentTheme = Globals.DefaultTheme;
        }
        SetTheme((this as IProp).CurrentTheme);
    }
    public override void _Notification(int what)
    {
        if (what == NotificationVisibilityChanged)
        {
            if (Content == 2)
            {
                if (Visible)
                    Defer(() => StartAnimation());
                else
                    Defer(StopAnimation);
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
        var ep = (this as IExtendedProperties);
        if (!ep.IsValid() && Engine.EditorHint)
            RealRefresh();
        return ep.OnGetPropertyList();
    }

    void IProp.UpdateTheme()
    {
        Defer(RealRefresh);
    }
    void IExtendedProperties.LoadProperties()
    {
        var ep = (IExtendedProperties)this;
        ep.AddProperty(new GDProperty("ShowIcon", Variant.Type.Bool, PropertyHint.None),
            () => ShowIcon, o => ShowIcon = (bool)o);
        if (ShowIcon)
        {
            ep.AddProperty(new GDProperty("TransformIcon", Variant.Type.Bool, PropertyHint.None),
                () => TransformIcon, o => TransformIcon = (bool)o);
            ep.AddProperty(new GDProperty("CenterIcon", Variant.Type.Bool, PropertyHint.None),
                () => CenterIcon, o => CenterIcon = (bool)o);
            ep.AddProperty(new GDProperty("IconOffset", Variant.Type.Vector2, PropertyHint.None),
                () => IconOffset, o => IconOffset = (Vector2)o);
            ep.AddProperty(new GDProperty("IconScale", Variant.Type.Vector2, PropertyHint.None),
                () => IconScale, o => IconScale = (Vector2)o);
        }
        if (Background != 0)
        {
            ep.AddProperty(new GDProperty("BackgroundOpacity", Variant.Type.Real, PropertyHint.ExpRange, "0.0,1.0"),
                () => BackgroundOpacity, o => BackgroundOpacity = (float)o);
        }
        if (Content == 1)
        {
            ep.AddProperty(new GDProperty("Texture", Variant.Type.Object, PropertyHint.ResourceType, "Texture"),
                () => (_contentData as ImageContent)._texture, o =>
                {
                    if (_contentData == null || !(_contentData is ImageContent))
                    {
                        _contentData = new ImageContent();
                    }
                    (_contentData as ImageContent)._texture = (Texture)o;
                    Refresh();
                });
            ep.AddProperty(new GDProperty("MaskEnabled", Variant.Type.Bool, PropertyHint.None),
                () => (_contentData as ImageContent)._maskEnabled, o =>
                {
                    if (_contentData == null || !(_contentData is ImageContent))
                    {
                        _contentData = new ImageContent();
                    }
                    (_contentData as ImageContent)._maskEnabled = (bool)o;
                    Refresh();
                });
            if (_contentData is ImageContent ic && ic._maskEnabled)
            {
                ep.AddProperty(new GDProperty("Mask", Variant.Type.Object, PropertyHint.ResourceType, "Texture"),
                    () => (_contentData as ImageContent)._mask, o =>
                    {
                        if (_contentData == null || !(_contentData is ImageContent))
                        {
                            _contentData = new ImageContent();
                        }
                        (_contentData as ImageContent)._mask = (Texture)o;
                        Refresh();
                    });
            }
        }
        else if (Content == 2)
        {
            ep.AddProperty(new GDProperty("Frames", Variant.Type.Object, PropertyHint.ResourceType, "SpriteFrames"),
                () => (_contentData as AnimatedContent)._frames, o =>
                {
                    if (_contentData == null || !(_contentData is AnimatedContent))
                    {
                        _contentData = new AnimatedContent();
                    }
                    (_contentData as AnimatedContent)._frames = o as SpriteFrames;
                    Refresh();
                });
            ep.AddProperty(new GDProperty("MaskEnabled", Variant.Type.Bool, PropertyHint.None),
                () => (_contentData as AnimatedContent)._maskEnabled, o =>
                {
                    if (_contentData == null || !(_contentData is AnimatedContent))
                    {
                        _contentData = new AnimatedContent();
                    }
                    (_contentData as AnimatedContent)._maskEnabled = (bool)o;
                    Refresh();
                });
            if (_contentData is AnimatedContent ac && ac._maskEnabled)
            {
                ep.AddProperty(new GDProperty("AnimatedMask", Variant.Type.Bool, PropertyHint.None),
                    () => (_contentData as AnimatedContent)._animatedMask, o =>
                    {
                        if (_contentData == null || !(_contentData is AnimatedContent))
                        {
                            _contentData = new AnimatedContent();
                        }

                        (_contentData as AnimatedContent)._animatedMask = (bool)o;
                        Refresh();
                    });
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
                        if (_contentData == null || !(_contentData is AnimatedContent))
                        {
                            _contentData = new AnimatedContent();
                        }
                        (_contentData as AnimatedContent)._mask = o;
                        Refresh();
                    }
                );
                ep.AddProperty(new GDProperty("Loop", Variant.Type.Bool, PropertyHint.None),
                    () => false, o =>
                    {
                        // this only exists for backward-compability
                    });
            }
        }
    }
    void UpdateBannerMesh()
    {
        if (_bannerMeshInstance == null || _bannerMeshInstance.GetParent() != this)
        {
            if (this.HasNode("BannerMesh"))
                _bannerMeshInstance = this.GetNodeSafe<MeshInstance>("BannerMesh");
            else
            {
                _bannerMeshInstance = new MeshInstance();
                _bannerMeshInstance.Mesh = new QuadMesh();
                _bannerMeshInstance.Name = "BannerMesh";
                this.AddChild(_bannerMeshInstance);
            }
        }

        var mat = new ShaderMaterial();
        _bannerMeshInstance.MaterialOverride = mat;
        mat.Shader = ResourceLoader.Load<Shader>(Constants.AssetPaths.BANNER_PROP_SHADER);
        _bannerMeshInstance.MaterialOverride.ResourceLocalToScene = true;
        _bannerMeshInstance.Owner = this.Owner;
    }
    public BannerProp()
    {

    }
    public override void _Ready()
    {
        UpdateBannerMesh();
        (this as IProp).InitHierarchy();
        Refresh();
    }

    public override void _Process(float delta)
    {
        if (Content == 2 && _contentData is AnimatedContent ic)
        {
            if (ic._running && ic._frames != null && ic._frames.HasAnimation(ic._currentAnimation))
            {
                if (ic._frames.GetFrameCount(ic._currentAnimation) > 0)
                    TickAnimation(delta);
            }
        }
    }
}
