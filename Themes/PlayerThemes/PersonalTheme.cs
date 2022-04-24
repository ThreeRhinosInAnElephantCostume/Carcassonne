

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
using static Constants.ShaderParams.ThemableShader;
using static Utils;
using Expression = System.Linq.Expressions.Expression;

[Serializable]
public class PersonalTheme
{
    public Color PrimaryColor;
    public Color SecondaryColor;
    public Color TertiaryColor;
    public string _iconPath;
    [JsonIgnore]
    public string IconPath { get => _iconPath; set { if (_icon != null) _icon.Dispose(); _icon = null; _iconPath = value; } }
    [JsonIgnore]
    Texture _icon;
    [JsonIgnore]
    public Texture Icon
    {
        get
        {
            if (_icon == null)
            {
                _icon = ResourceLoader.Load<Texture>(IconPath);
                Assert(_icon != null, "Could not load icon from " + IconPath);
            }
            return _icon;
        }
    }
    public string _avatarPath;
    [JsonIgnore]
    public string AvatarPath { get => _avatarPath; set { if (_avatar != null) _avatar.Dispose(); _avatar = null; _avatarPath = value; } }
    [JsonIgnore]
    Texture _avatar;
    [JsonIgnore]
    public Texture Avatar
    {
        get
        {
            if (_avatar == null)
            {
                _avatar = ResourceLoader.Load<Texture>(AvatarPath);
                Assert(_avatar != null, "Could not load avatar from " + AvatarPath);
            }
            return _avatar;
        }
    }
    [JsonIgnore]
    public Color[] Colors => new Color[] { PrimaryColor, SecondaryColor, TertiaryColor };
    public static List<Action<PersonalTheme>> GetSettersForShader(ShaderMaterial shad, bool iconenabled = true)
    {
        var ret = new List<Action<PersonalTheme>>();
        if (shad.GetShaderParam(SHADER_PRIMARY_THEME_SETTER) != null)
            ret.Add(t => shad.SetShaderParam(SHADER_PRIMARY_THEME_SETTER, t.PrimaryColor));
        if (shad.GetShaderParam(SHADER_SECONDARY_THEME_SETTER) != null)
            ret.Add(t => shad.SetShaderParam(SHADER_SECONDARY_THEME_SETTER, t.SecondaryColor));
        if (shad.GetShaderParam(SHADER_TERTIARY_THEME_SETTER) != null)
            ret.Add(t => shad.SetShaderParam(SHADER_TERTIARY_THEME_SETTER, t.TertiaryColor));
        if (shad.GetShaderParam(SHADER_ICON_ENABLED_THEME_SETTER) != null)
            ret.Add(t => shad.SetShaderParam(SHADER_ICON_ENABLED_THEME_SETTER, iconenabled));
        if (iconenabled && shad.GetShaderParam(SHADER_ICON_TEXTURE_THEME_SETTER) != null)
            ret.Add(t => shad.SetShaderParam(SHADER_ICON_TEXTURE_THEME_SETTER, t.Icon));
        return ret;
    }
    public void SetFullShader(ShaderMaterial shad, bool icon_enabled, Vector2? icon_scale = null, Vector2? icon_offset = null)
    {
        shad.SetShaderParam(SHADER_PRIMARY_THEME_SETTER, PrimaryColor);
        shad.SetShaderParam(SHADER_SECONDARY_THEME_SETTER, SecondaryColor);
        shad.SetShaderParam(SHADER_TERTIARY_THEME_SETTER, TertiaryColor);
        shad.SetShaderParam(SHADER_ICON_ENABLED_THEME_SETTER, icon_enabled);
        if (icon_enabled)
        {
            Assert(Icon != null);
            Assert(icon_scale != null);
            Assert(icon_offset != null);
            shad.SetShaderParam(SHADER_ICON_TEXTURE_THEME_SETTER, Icon);
            shad.SetShaderParam(SHADER_ICON_SCALE_THEME_SETTER, icon_scale.Value);
            shad.SetShaderParam(SHADER_ICON_OFFSET_THEME_SETTER, icon_offset.Value);
        }
    }
    public Color TransformColor(Color col)
    {
        return new Color((PrimaryColor * col.r) + (SecondaryColor * col.g) + (TertiaryColor * col.b), col.a);
    }
    public Image TransformImage(Image _img)
    {
        Image img = new Image();
        img.CopyFrom(_img);
        if (img.IsCompressed())
            img.Decompress();
        img.Lock();
        for (int i = 0; i < img.GetSize().x; i++)
        {
            for (int ii = 0; ii < img.GetSize().y; ii++)
            {
                img.SetPixel(i, ii, TransformColor(img.GetPixel(i, ii)));
            }
        }
        img.Unlock();
        return img;
    }
    public Texture TransformTexture(Texture tex)
    {
        var imgtex = new ImageTexture();
        imgtex.CreateFromImage(TransformImage(tex.GetData()));
        return imgtex;
    }
    public PersonalTheme Copy()
    {
        return DeserializeFromString<PersonalTheme>(SerializeToString(this));
    }
}
