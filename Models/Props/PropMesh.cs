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
using static Utils;
using Expression = System.Linq.Expressions.Expression;

[Tool]
public class PropMesh : MeshInstance, IPropElement
{
    [Export]
    public int[] PrimarySurfaces = new int[0];
    [Export]
    public int[] SecondarySurfaces = new int[0];
    [Export]
    public int[] TertiarySurfaces = new int[0];
    List<Action<PlayerTheme>> IPropElement.GetThemeSetters()
    {
        void SetMat(MeshInstance mesh, int indx, Color col)
        {
            var mat = (SpatialMaterial)mesh.Mesh.SurfaceGetMaterial(indx);
            mat.AlbedoColor = col;
            mesh.Mesh.SurfaceSetMaterial(indx, mat);
        }
        var ret = new List<Action<PlayerTheme>>();
        RepeatN(this.GetSurfaceMaterialCount(), i =>
        {
            var mat = this.Mesh.SurfaceGetMaterial(i);
            if (mat is SpatialMaterial)
            {
                if (PrimarySurfaces[i] > 0)
                {
                    ret.Add(t => { SetMat(this, i, t.PrimaryColor); });
                }
                else if (SecondarySurfaces[i] > 0)
                {
                    ret.Add(t => { SetMat(this, i, t.SecondaryColor); });
                }
                else if (TertiarySurfaces[i] > 0)
                {
                    ret.Add(t => { SetMat(this, i, t.SecondaryColor); });
                }
            }
            else if (mat is ShaderMaterial shad)
            {
                if (shad.GetShaderParam(Constants.SHADER_PRIMARY_THEME_SETTER) != null)
                    ret.Add(t => shad.SetShaderParam(Constants.SHADER_PRIMARY_THEME_SETTER, t.PrimaryColor));
                if (shad.GetShaderParam(Constants.SHADER_SECONDARY_THEME_SETTER) != null)
                    ret.Add(t => shad.SetShaderParam(Constants.SHADER_SECONDARY_THEME_SETTER, t.SecondaryColor));
                if (shad.GetShaderParam(Constants.SHADER_TERTIARY_THEME_SETTER) != null)
                    ret.Add(t => shad.SetShaderParam(Constants.SHADER_TERTIARY_THEME_SETTER, t.TertiaryColor));
            }
        });
        return ret;
    }
    System.Action IPropElement.OnChangeHandle { get; set; }
    public override void _Ready()
    {
        int[] FitSurfaces(int[] surfaces)
        {
            int c = this.GetSurfaceMaterialCount();
            if (surfaces.Length == c)
                return surfaces;
            int[] ret = new int[c];
            RepeatN(Min(c, surfaces.Length), i => ret[i] = surfaces[i]);
            if (surfaces.Length < c)
            {
                RepeatN(c, i => ret[i + surfaces.Length] = 0);
            }
            return ret;
        }
        PrimarySurfaces = FitSurfaces(PrimarySurfaces);
        SecondarySurfaces = FitSurfaces(SecondarySurfaces);
        TertiarySurfaces = FitSurfaces(TertiarySurfaces);
        var n = (Node)this;
        while ((n = n.GetParent()) != null)
        {
            if (n is Prop prop)
            {
                prop.MarkForLoad();
                break;
            }
        }
    }
    public override bool _Set(string property, object value)
    {
        if ((this as IPropElement).OnChangeHandle != null)
            (this as IPropElement).OnChangeHandle();
        return base._Set(property, value);
    }
}
