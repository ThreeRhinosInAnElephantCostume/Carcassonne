using System;
using System.Collections.Generic;
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

public class EditableTileset : Resource, Carcassonne.ITileset
{
    [Export]
    public bool UserEditable { get; set; }
    [Export]
    public bool SingleStarter { get; set; }
    [Export]
    public int NStarterTiles { get; set; }
    [Export]
    public int NPossibleTiles { get; set; }
    [Export]
    public int NOutputTiles { get; set; }
    [Export]
    public TilePrototype[] Tiles { get; set; }
    [Export]
    public TilePrototype[] StarterTiles { get; set; }

    public void UpdateInternals()
    {
        if (Tiles.Length > NPossibleTiles)
        {
            NPossibleTiles = Tiles.Length;
        }
        if (SingleStarter)
        {
            if (StarterTiles.Length > 1)
            {
                SingleStarter = false;
                NStarterTiles = 1;
            }
            else
            {
                NStarterTiles = -1;
            }
        }
        else
        {
            if (NStarterTiles < 0)
                NStarterTiles = (StarterTiles.Length > 0) ? 1 : 0;
        }



    }

    public override bool _Set(string property, object value)
    {
        bool r = base._Set(property, value);
        UpdateInternals();
        return r;
    }

    public EditableTileset(bool UserEditable)
    {
        this.UserEditable = false;
        SingleStarter = true;
        NStarterTiles = -1;
        NPossibleTiles = 0;
        NOutputTiles = 0;
        Tiles = new TilePrototype[0];
        StarterTiles = new TilePrototype[0];
    }

    bool ITileset.HasStarter => (NStarterTiles > 0 || (SingleStarter && StarterTiles.Length > 0));

    int ITileset.NPossibleStarters => (SingleStarter && StarterTiles.Length > 0) ? 1 : StarterTiles.Length;

    int ITileset.NPossibleTiles => NPossibleTiles;

    int ITileset.NTiles => NOutputTiles;

    Tile ITileset.GenerateStarter(RNG rng)
    {
        Assert(!(SingleStarter && NStarterTiles > 1));
        if (SingleStarter)
        {
            Assert(StarterTiles.Length > 0 && StarterTiles[0] != null);
            return StarterTiles[0].Convert();
        }
        else if (NStarterTiles == 0)
        {
            return null;
        }
        return StarterTiles[rng.NextLong(0, StarterTiles.Length) % StarterTiles.Length].Convert();
    }

    List<Tile> ITileset.GenerateTiles(RNG rng)
    {
        Assert(Tiles != null);
        Assert(Tiles.Length > 0);
        Assert(NOutputTiles > 0);
        Assert(NPossibleTiles > 0);

        List<TilePrototype> tocreate = new List<TilePrototype>(NOutputTiles);
        if (NOutputTiles >= Tiles.Length)
        {
            tocreate.AddRange(Tiles);
        }
        while (tocreate.Count < NOutputTiles)
        {
            tocreate.Add(Tiles[rng.NextLong(0, Tiles.Length)]);
        }

        List<Tile> ret = new List<Tile>(tocreate.Count);
        foreach (var it in tocreate)
        {
            ret.Add(it.Convert());
        }
        return ret;
    }
}
