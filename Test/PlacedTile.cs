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
    public float EdgeWidth = 5;
    [Export]
    public Color EdgeColor { get; set; } = new Color(0.9f, 0.9f, 0.9f, 0.9f);

    public List<int> HighlightedNodes = new List<int>();

    [Export]
    public int HighlightedNode 
    { 
        get 
        {
            if(HighlightedNodes.Count == 0)
                return -1;
            return HighlightedNodes[0];
        } 
        set 
        {
            HighlightedNodes.Clear();
            if(value != -1)
            {
                HighlightedNodes.Add(value);
            }
        }
    }
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
                var ntile = TileDataLoader.LoadTilePrototype(_tileoverride).Convert();
                if (ntile != null)
                    RenderedTile = ntile;
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
    public Color MonasteryColor { get; set; } = new Color(0.4f, 0.4f, 0f);
    [Export]
    public Color CityBonusColor { get; set; } = new Color(0.1f, 0.1f, 1.0f);
    [Export]
    public float ConSize = 3.0f;
    [Export]
    public float NodeConSize = 1.5f;
    [Export]
    public float TerminSize = 5f;
    [Export]
    public float MeepleSize = 15.0f;
    [Export]
    public float MonasterySize = 20.0f;
    [Export]
    public float CityBonusSize = 10.0f;
    [Export]
    public Color[] PlayerColors {get; set;} = new Color[]
    {
        new Color(1f, 0.3f, 0.3f),
        new Color(0.5f, 0.5f, 1f),
        new Color(1, 0.5f, 1f),
        new Color(0.7f, 1, 0.7f),
        new Color(1f, 1f, 1f),
        new Color(0.5f, 0.5f, 0.5f),
    };
    public Tile RenderedTile  {get; set;} = null;

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
    Color GetPlayerColor(int indx)
    {
        if (PlayerColors.Length <= indx)
        {
            return new Color(0, 0, 0, 1);
        }
        return PlayerColors[indx];
    }
    void DrawMeeple(Vector2 pos, Color color)
    {
        float sz = MeepleSize;
        var vsz = new Vector2(sz, sz);
        DrawRect(new Rect2(pos-vsz/2,  vsz), color);
    }
    void DrawMonastery(Vector2 pos, Color color)
    {
        DrawRect(new Rect2(pos - (new Vector2(MonasterySize/6, MonasterySize/2)), new Vector2(MonasterySize / 3, MonasterySize)), color);
        DrawRect(new Rect2(pos - (new Vector2(MonasterySize/2, MonasterySize/6)), new Vector2(MonasterySize, MonasterySize / 3)), color);
    }
    void DrawCityBonus(Vector2 pos, Color color)
    {
        var prim = new Vector2[]
        {
            new Vector2(-1, -1)*CityBonusSize + pos,
            new Vector2(1, -1)*CityBonusSize + pos,
            new Vector2(0, 1)*CityBonusSize + pos,
        };
        DrawPrimitive(prim, new Color[] { color, color, color }, prim);
    }
    public override void _Draw()
    {
        Color edgecolor = new Color(this.EdgeColor, this.EdgeColor.a * OpacityMP);
        DrawRect(new Rect2(-outersize / 2, outersize), edgecolor, false, EdgeWidth);

        if (RenderedTile == null)
        {
            if (Godot.Engine.EditorHint && TileOverride != "")
            {
                RenderedTile = TileDataLoader.LoadTilePrototype(TileOverride).Convert();
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
                Tile.Connection con = RenderedTile.Sides[i].Connections[ii];
                Color c = GetTypeColor(con.INode.Type);

                int indx = RenderedTile.Nodes.ToList().IndexOf(con.INode);
                if (HighlightedNodes.Contains(indx))
                    c = HighlightColor;

                c.a *= OpacityMP;
                Vector2 par = pars[i];
                Vector2 origin = edges[i];
                Vector2 start = origin + (par * ii * outersize / N_CONNECTORS);
                Vector2 end = origin + (par * (ii + 1) * outersize / N_CONNECTORS);

                DrawLine(start, end, c, ConSize);

                if (!points.ContainsKey(con.INode))
                    points.Add(con.INode, new List<Vector2>());
                points[con.INode].Add((start + end) / 2);
            }
        }
        foreach (var k in points.Keys)
        {
            Color c = GetTypeColor(k.Type);
            c.a *= OpacityMP;

            int indx = RenderedTile.Nodes.ToList().IndexOf(k);
            if (HighlightedNodes.Contains(indx))
                c = HighlightColor;
            bool drawmeeple = false;
            Color meeplecolor = new Color();
            if (k.Graph != null)
            {
                object mo = k.Graph.Owners.Find(o => o is Meeple m && m.IsConnectedToNode(k));
                if (mo != null && mo is Meeple m)
                {
                    drawmeeple = true;
                    meeplecolor = GetPlayerColor((int)m.Owner.ID);
                }
            }
            if (points[k].Count == 1)
            {
                DrawLine(points[k][0] / unconnecteddiv, points[k][0], c, NodeConSize);
                if (drawmeeple)
                    DrawMeeple((points[k][0] + points[k][0] / unconnecteddiv) / 2, meeplecolor);
                DrawCircle(points[k][0] / unconnecteddiv, TerminSize, c);
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
                DrawLine(centre, it, c, NodeConSize);
            }
            if (k.AttributeTypes.Contains(NodeAttributeType.CITY_BONUS))
            {
                DrawCityBonus(centre + (points[k][0] - centre) * 0.1f, CityBonusColor);
            }
            if (drawmeeple)
                DrawMeeple(centre, meeplecolor);
        }
        if (RenderedTile.AttributeTypes.Contains(TileAttributeType.MONASTERY))
        {
            TileMonasteryAttribute mon = (TileMonasteryAttribute)RenderedTile.Attributes.Find(it => it.Type == TileAttributeType.MONASTERY);
            DrawMonastery(new Vector2(0, 0), (mon.Owner != null) ? (GetPlayerColor((int)(mon.Owner as Meeple).Owner.ID)) : MonasteryColor);
        }
    }
}
