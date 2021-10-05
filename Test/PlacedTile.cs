using System.ComponentModel;
using Godot;
using System;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime;
using System.Runtime.CompilerServices;

using static System.Math;

using static Utils;

using ExtraMath;


[Tool]
public class PlacedTile : Node2D
{
    [Export]
    public float size = 100;
    public Vector2 outersize = new Vector2();

    [Export]
    public int edgewidth = 5;
    [Export]
    public Color edgecolor = new Color(0.9f, 0.9f, 0.9f, 0.9f);
    [Export]
    public string TileOverride{get; set;} = "";

    [Export]
    public Color RoadColor{get; set;} = new Color(0.3f, 0.3f, 0.6f);
    [Export]
    public Color FarmColor{get; set;} = new Color(0.3f, 0.95f, 0.3f);
    [Export]
    public Color CityColor{get; set;} = new Color(0.4f, 0.4f, 0.1f);
    [Export]
    public float consize = 3.0f;
    [Export]
    public float nodeconsize = 1.5f;
    [Export]
    public float terminsize = 5f;
    public Engine.Tile tile = null;
    public override void _Ready()
    {
        outersize = new Vector2(size, size);
    }

    static int _ev = 0;
    public override void _Process(float delta)
    {
        if(Godot.Engine.EditorHint)
        {
            if(TileOverride != "")
                tile = TileMap.TileGenerator(TileOverride);
            if(_ev >= 60)
            {
                Update();
                _ev = 0;
            }
            _ev++;
        }
    }
    public override void _Draw()
    {

        DrawRect(new Rect2(-outersize/2, outersize), edgecolor, false, edgewidth);

        if(tile == null)
            return;

        Vector2[] edges = new Vector2[4] {-outersize/2, new Vector2(outersize.x/2, -outersize.y/2),
             outersize/2, new Vector2(-outersize.x/2, outersize.y/2)};
        Vector2[] dirs = new Vector2[4]{Vector2.Up, Vector2.Right, Vector2.Down, Vector2.Left};     
        Vector2[] pars = new Vector2[4]{Vector2.Right, Vector2.Down, Vector2.Left, Vector2.Up};

        var points = new Dictionary<Engine.Tile.InternalNode, List<Vector2>>();

        for(int i = 0; i < Engine.N_SIDES; i++)
        {
            for(int ii = 0; ii < Engine.N_CONNECTORS; ii++)
            {
                Engine.Tile.Connection con = tile.sides[i].connectors[ii];
                Color c = con.node switch 
                {
                    Engine.FarmNode _ => FarmColor,
                    Engine.RoadNode _ => RoadColor,
                    Engine.CityNode _ => CityColor,
                    _ => throw new Exception(""),
                };

                Vector2 par = pars[i];
                Vector2 origin = edges[i];
                Vector2 start = origin + (par * ii * outersize / Engine.N_CONNECTORS);
                Vector2 end = origin + (par * (ii+1) * outersize / Engine.N_CONNECTORS);

                DrawLine(start, end, c, consize);

                if(!points.ContainsKey(con.node))
                    points.Add(con.node, new List<Vector2>());
                points[con.node].Add((start+end)/2);
            }
        }
        foreach(var k in points.Keys)
        {
            Color c = k switch 
            {
                Engine.FarmNode _ => FarmColor,
                Engine.RoadNode _ => RoadColor,
                Engine.CityNode _ => CityColor,
                _ => throw new Exception(""),
            };
            if(points[k].Count == 1)
            {
                DrawLine(points[k][0]/2, points[k][0], c, nodeconsize);
                DrawCircle(points[k][0]/2, terminsize, c);
            }
            Vector2 centre = new Vector2();
            foreach(var it in points[k])
            {
                centre += it / points[k].Count;
            }
            centre /= 2;
            foreach(var it in points[k])
            {

                DrawLine(centre, it, c, nodeconsize);
            }
        }
    }
}
