

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
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

public class Tile3D : Spatial
{
    public Tile AssociatedTile { get; protected set; }

    readonly TileDataLoader.TileModel _model;
    readonly Spatial _root;

    Vector2I _position = new Vector2I();
    readonly Dictionary<string, Vector3> _groupAveragePosition = new Dictionary<string, Vector3>();
    readonly Dictionary<string, List<IProp>> _propGroups = new Dictionary<string, List<IProp>>();
    readonly List<PotentialMeeplePlacement> _potentialPlacements = new List<PotentialMeeplePlacement>();
    readonly List<MeeplePlacement> _placements = new List<MeeplePlacement>();
    public Vector3 CalculateAttributePlacementPosition(int indx)
    {
        List<string> associated = new List<string>();
        return _groupAveragePosition[_model.Config.AttributeAssociations.Keys.ToList().Find(it =>
            _model.Config.AttributeAssociations[it] == indx)];
    }
    public Vector3 CalculateNodePlacementPosition(int indx)
    {
        List<string> associated = new List<string>();
        return _groupAveragePosition[_model.Config.NodeAssociations.Keys.ToList().Find(it =>
            _model.Config.NodeAssociations[it] == indx)];
    }
    public void ClearPotentialPlacements()
    {
        _potentialPlacements.ForEach(it => DestroyNode(it));
        _potentialPlacements.Clear();
    }
    public void AddPotentialAttributePlacement(Game.GameLocalAgent agent, int indx)
    {
        var pot = Globals.Scenes.PotentialMeeplePlacementPacked.Instance<PotentialMeeplePlacement>();
        pot.Agent = agent;
        pot.Index = indx;
        pot.IsAttribute = true;
        pot.Translation = CalculateAttributePlacementPosition(indx);
        pot.AssociatedTile = AssociatedTile;
        AddChild(pot);
        _potentialPlacements.Add(pot);
    }
    public void AddPotentialNodePlacement(Game.GameLocalAgent agent, int indx)
    {
        var pot = Globals.Scenes.PotentialMeeplePlacementPacked.Instance<PotentialMeeplePlacement>();
        pot.Agent = agent;
        pot.Index = indx;
        pot.IsAttribute = false;
        pot.Translation = CalculateNodePlacementPosition(indx);
        pot.AssociatedTile = AssociatedTile;
        AddChild(pot);
        _potentialPlacements.Add(pot);
    }
    public void AddAttributePlacement(Game.GameAgent agent, int indx)
    {
        // if (_placements.Any(it => it.Agent == agent && it.Index == indx && it.IsAttribute))
        //     return;
        // var meep = Globals.MeeplePlacementPacked.Instance<MeeplePlacement>();
        // (meep as IProp).CurrentTheme = agent.CurrentTheme;
        // meep.Agent = agent;
        // meep.IsAttribute = false;
        // meep.Index = indx;
        // meep.Translation = CalculateAttributePlacementPosition(indx);
        // AddChild(meep);
        // _placements.Add(meep);

        var rootnode = _model.Config.AttributeAssociations.Keys.ToList().Find(it =>
            _model.Config.AttributeAssociations[it] == indx);
        Defer(() => AddOccupierPlacement(agent, rootnode));
    }
    public void AddNodePlacement(Game.GameAgent agent, int indx)
    {
        // if (_placements.Any(it => it.Agent == agent && it.Index == indx && !it.IsAttribute))
        //     return;
        // var meep = Globals.MeeplePlacementPacked.Instance<MeeplePlacement>();
        // (meep as IProp).CurrentTheme = agent.CurrentTheme;
        // meep.Agent = agent;
        // meep.IsAttribute = false;
        // meep.Index = indx;
        // meep.Translation = CalculateNodePlacementPosition(indx);
        // AddChild(meep);
        // _placements.Add(meep);
        var rootnode = _model.Config.NodeAssociations.Keys.ToList().Find(it =>
            _model.Config.NodeAssociations[it] == indx);
        Defer(() => AddOccupierPlacement(agent, rootnode));
    }
    void AddOccupierPlacement(Game.GameAgent agent, string rootnode)
    {
        Assert(_propGroups.ContainsKey(rootnode));
        _propGroups[rootnode].ForEach(prop =>
        {
            if (prop is CapturableProp cap)
                cap.Potential = false;
            prop.CurrentTheme = agent.CurrentTheme;
        });
    }
    public Vector2I Pos
    {
        get => _position;
        set
        {
            _position = value;
            this.Translation = GridPosTo3D(_position);
        }
    }
    int _rotation = 0;
    public int Rot
    {
        get => _rotation;
        set
        {
            _rotation = value;
            this.Rotation = new Vector3(this.Rotation.x, -(float)_rotation * (float)PI / 2, this.Rotation.z);
        }
    }
    void CachePositions()
    {

        _groupAveragePosition.Clear();

        void Cache(Dictionary<string, int> dict)
        {
            dict.Keys.ToList().ForEach(s => _groupAveragePosition.Add(s, new Vector3(0, 0, 0)));

            dict.Keys.ToList().ForEach(s =>
            {
                int n = 0;
                _root.GetNode<Spatial>(s).GetChildrenRecrusively<MeshInstance>().ForEach(
                mi =>
                {
                    n++;
                    Vector3 av = new Vector3(0, 0, 0);
                    for (int i = 0; i < mi.Mesh.GetSurfaceCount(); i++)
                    {
                        var arr = mi.Mesh.SurfaceGetArrays(i);
                        Vector3[] a = (Vector3[])arr[(int)ArrayMesh.ArrayType.Vertex];
                        foreach (var it in a)
                        {
                            av += it / a.Length;
                        }
                    }
                    _groupAveragePosition[s] = _groupAveragePosition[s] + av;
                });
                _groupAveragePosition[s] = (_groupAveragePosition[s] / n) + _root.GetNode<Spatial>(s).Translation;
            });
        }

        // Note: Change if meshes get proper transforms. TAGS: translations, transforms, meshinstances
        Cache(_model.Config.NodeAssociations);
        Cache(_model.Config.AttributeAssociations);
    }
    void PreProcessProps()
    {
        void ProcessPotentialPropNodes(Dictionary<string, int> dict, bool attribute)
        {
            dict.Keys.ToList().ForEach(s =>
            {
                var l = new List<IProp>();
                var node = _root.GetNodeSafe<Spatial>(s);
                if (node is IProp prop)
                {
                    l.Add(prop);
                }
                else if (node is CapturableProp cap)
                {
                    l.Add(cap);
                }
                l.AddRange(node.GetChildrenRecrusively<IProp>().FindAll(it => it._parent == null));
                int indx = dict[s];
                OccupierContainer cont = (attribute) ? (OccupierContainer)AssociatedTile.Attributes[indx] : (OccupierContainer)AssociatedTile.Nodes[indx];

                foreach (IProp p in l)
                {
                    if (p is CapturableProp cap)
                    {
                        cap.Data = (cont, (attribute) ? null : ((InternalNode)cont).Graph);
                    }
                    p.CurrentTheme = null;
                }
                _propGroups.Add(s, l);
            });
        }
        ProcessPotentialPropNodes(_model.Config.NodeAssociations, false);
        ProcessPotentialPropNodes(_model.Config.AttributeAssociations, true);
    }
    public override void _Ready()
    {
        PreProcessProps();
    }
    public Tile3D(Tile tile, int rot, RNG rng)
    {
        this.AssociatedTile = tile;
        Assert(tile.MetaData is string);
        string path = (string)tile.MetaData;
        var models = TileDataLoader.LoadPrototypeModels(path);
        Assert(models.Count > 0);
        _model = models[(int)(((uint)rng.NextLong()) % models.Count)];
        _root = (Spatial)_model.Scene.Instance();
        this.AddChild(_root);

        _root.RotateY((float)(_model.Config.Rotation * PI / 2));
        Pos = tile.Position;
        Rot = rot;
        CachePositions();
    }
}
