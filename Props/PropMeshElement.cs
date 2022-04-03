using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using ExtraMath;
using Godot;
using Newtonsoft.Json;
using static System.Math;
using static Utils;
using Expression = System.Linq.Expressions.Expression;

[Tool]
public class MeshProp : MeshInstance, IProp
{
    PersonalTheme IProp._theme {get; set;}
    string IProp._examplePlayerTheme {get; set;}
    List<IProp> IProp._children {get; set;} = new List<IProp>();
    IProp IProp._parent {get; set;}
    public List<(bool primary, bool secondary, bool tertiary)> SurfaceSettings = new List<(bool, bool, bool)>();
    void MaybeInitSurfaces()
    {
        if (SurfaceSettings.Count != GetSurfaceMaterialCount())
        {
            SurfaceSettings.Clear();
            RepeatN(GetSurfaceMaterialCount(), i => SurfaceSettings.Add((false, false, false)));
            _setters = GenerateThemeSetters();
        }
    }
    public override Godot.Collections.Array _GetPropertyList()
    {
        MaybeInitSurfaces();
        var ret = new Godot.Collections.Array();
        for (int i = 0; i < SurfaceSettings.Count; i++)
        {
            ret.Add(new GDProperty($"Surface #{i}", Variant.Type.Int, PropertyHint.Flags, "Primary,Secondary,Tertiary").ToDictionary());
        }
        return ret;
    }
    public override object _Get(string property)
    {
        MaybeInitSurfaces();
        if (property.Contains("Surface #"))
        {
            int num = int.Parse(property.Split("#")[1]);
            Assert(num >= 0 && num < SurfaceSettings.Count);
            if (this.Mesh.SurfaceGetMaterial(num) is SpatialMaterial)
            {
                int val = ((SurfaceSettings[num].primary) ? 1 : 0) |
                    ((SurfaceSettings[num].secondary) ? 2 : 0) |
                    ((SurfaceSettings[num].tertiary) ? 4 : 0);
                return val;
            }
        }
        return base._Get(property);
    }
    public override bool _Set(string property, object value)
    {
        MaybeInitSurfaces();
        if (property.Contains("Surface #"))
        {
            int num = int.Parse(property.Split("#")[1]);
            Assert(num >= 0 && num < SurfaceSettings.Count);
            int val = (int)value;
            SurfaceSettings[num] = ((val & 0b1) != 0, (val & 0b10) != 0, (val & 0b100) != 0);
            _setters = GenerateThemeSetters();
            (this as IProp).UpdateTheme();
            return true;
        }
        return base._Set(property, value);
    }
    List<Action<PersonalTheme>> GenerateThemeSetters()
    {
        MaybeInitSurfaces();
        void SetMat(MeshInstance mesh, int indx, PersonalTheme theme, bool p, bool s, bool t)
        {
            var mat = (SpatialMaterial)mesh.Mesh.SurfaceGetMaterial(indx);
            Color col = new Color(0, 0, 0, 1);
            int n = 0;
            if (p)
            {
                col += theme.PrimaryColor;
                n++;
            }
            if (s)
            {
                col += theme.SecondaryColor;
                n++;
            }
            if (t)
            {
                col += theme.TertiaryColor;
                n++;
            }
            if (n == 0)
            {
                mat = ResourceLoader.Load<SpatialMaterial>(mat.ResourcePath);
            }
            else
            {
                mat.ResourceLocalToScene = true;
                mat.VertexColorUseAsAlbedo = false;
                mat.AlbedoColor = col;
            }
            GD.Print("Set color to: ", mat.AlbedoColor);
            mesh.Mesh.SurfaceSetMaterial(indx, mat);
        }
        var ret = new List<Action<PersonalTheme>>();
        RepeatN(this.GetSurfaceMaterialCount(), i =>
        {
            var mat = this.Mesh.SurfaceGetMaterial(i);
            if (mat is SpatialMaterial)
            {
                var ss = SurfaceSettings[i];
                ret.Add(t => { SetMat(this, i, t, ss.primary, ss.secondary, ss.tertiary); });
            }
            else if (mat is ShaderMaterial shad)
            {
                ret.AddRange(PersonalTheme.GetSettersForShader(shad));
            }
        });
        return ret;
    }
    List<Action<PersonalTheme>> _setters = new List<Action<PersonalTheme>>();
    public override void _Ready()
    {
        (this as IProp).InitHierarchy();
    }

    void IProp.UpdateTheme()
    {
        if(_setters == null || _setters.Count == 0)
        {
            _setters = GenerateThemeSetters();
        }
        _setters.ForEach(it => it((this as IProp).CurrentTheme));
    }
}
