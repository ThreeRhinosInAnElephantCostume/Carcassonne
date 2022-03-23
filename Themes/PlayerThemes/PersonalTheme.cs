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

[Serializable]
public class PersonalTheme
{
    public Color PrimaryColor;
    public Color SecondaryColor;
    public Color TertiaryColor;
    public Texture Icon;
    public Texture Avatar;
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
}
