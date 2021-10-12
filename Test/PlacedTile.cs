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
using Carcassonne;
using ExtraMath;
using Godot;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

[Tool]
public class PlacedTile : TestTile
{

    [Export]
    public float edgewidth = 5;
    [Export]
    public Color edgecolor { get; set; } = new Color(0.9f, 0.9f, 0.9f, 0.9f);

    [Export]
    public int HighlightedNode { get; set; } = -1;
    [Export]
    public Color HighlightColor { get; set; } = new Color(0.9f, 0.2f, 0.2f, 0.9f);

    string _tileoverride = "";
    [Export]
    public string TileOverride
    {
        get => _tileoverride;
        set
        {
            _tileoverride = value;
            if (Godot.Engine.EditorHint)
            {
                var ntile = TileGenerator.LoadPrototype(_tileoverride).Convert();
                if (ntile != null)
                    tile = ntile;
            }
        }
    }

    [Export]
    public Color RoadColor { get; set; } = new Color(0.9f, 0.9f, 0.05f);
    [Export]
    public Color FarmColor { get; set; } = new Color(0.3f, 0.95f, 0.3f);
    [Export]
    public Color CityColor { get; set; } = new Color(0.4f, 0.4f, 0.1f);
    [Export]
    public float consize = 3.0f;
    [Export]
    public float nodeconsize = 1.5f;
    [Export]
    public float terminsize = 5f;
    public Tile tile = null;

    float _unconnecteddiv = 2;
    [Export(PropertyHint.Range, "1,10")]
    public float unconnecteddiv { get => _unconnecteddiv; set { _unconnecteddiv = value; Update(); } }
    float _connecteddiv = 2;
    [Export(PropertyHint.Range, "1,10")]
    public float connecteddiv { get => _connecteddiv; set { _connecteddiv = value; Update(); } }
    [Export]
    public float OpacityMP { get; set; } = 1.0f;
    Color GetTypeColor(NodeType tp)
    {
        return tp switch
        {
            NodeType.ERR => throw new NullReferenceException(),
            NodeType.FARM => FarmColor,
            NodeType.ROAD => RoadColor,
            NodeType.CITY => CityColor,
            _ => throw new Exception(),
        };
    }
    public override void _Draw()
    {
        Color edgecolor = new Color(this.edgecolor, this.edgecolor.a * OpacityMP);
        DrawRect(new Rect2(-outersize / 2, outersize), edgecolor, false, edgewidth);

        if (tile == null)
        {
            if (Godot.Engine.EditorHint && TileOverride != "")
            {
                tile = TileGenerator.LoadPrototype(TileOverride).Convert();
            }
            DrawCircle(new Vector2(0, 0), 5, edgecolor);
            return;
        }

        Vector2[] edges = new Vector2[4] {-outersize/2, new Vector2(outersize.x/2, -outersize.y/2),
             outersize/2, new Vector2(-outersize.x/2, outersize.y/2)};
        Vector2[] dirs = new Vector2[4] { Vector2.Up, Vector2.Right, Vector2.Down, Vector2.Left };
        Vector2[] pars = new Vector2[4] { Vector2.Right, Vector2.Down, Vector2.Left, Vector2.Up };

        var points = new Dictionary<InternalNode, List<Vector2>>();

        for (int i = 0; i < GameEngine.N_SIDES; i++)
        {
            for (int ii = 0; ii < N_CONNECTORS; ii++)
            {
                Tile.Connection con = tile.sides[i].connectors[ii];
                Color c = GetTypeColor(con.node.type);

                int indx = tile.nodes.ToList().IndexOf(con.node);
                if (indx == HighlightedNode)
                    c = HighlightColor;

                c.a *= OpacityMP;
                Vector2 par = pars[i];
                Vector2 origin = edges[i];
                Vector2 start = origin + (par * ii * outersize / N_CONNECTORS);
                Vector2 end = origin + (par * (ii + 1) * outersize / N_CONNECTORS);

                DrawLine(start, end, c, consize);

                if (!points.ContainsKey(con.node))
                    points.Add(con.node, new List<Vector2>());
                points[con.node].Add((start + end) / 2);
            }
        }
        foreach (var k in points.Keys)
        {
            Color c = GetTypeColor(k.type);
            c.a *= OpacityMP;

            int indx = tile.nodes.ToList().IndexOf(k);
            if (indx == HighlightedNode)
                c = HighlightColor;

            if (points[k].Count == 1)
            {
                DrawLine(points[k][0] / unconnecteddiv, points[k][0], c, nodeconsize);
                DrawCircle(points[k][0] / unconnecteddiv, terminsize, c);
                continue;
            }
            Vector2 centre = new Vector2();
            foreach (var it in points[k])
            {
                centre += it / points[k].Count;
            }
            centre /= connecteddiv;
            foreach (var it in points[k])
            {

                DrawLine(centre, it, c, nodeconsize);
            }
        }
    }
}
