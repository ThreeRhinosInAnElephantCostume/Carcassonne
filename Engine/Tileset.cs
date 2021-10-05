using Godot;


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

public partial class Engine
{
    public delegate Tile TileSourceDelegate(string name);
    public static TileSourceDelegate tilesource;

    protected static Tile GenerateTile(string name)
    {
        Tile r = tilesource(name);
        Assert(r != null);
        return r;
    }
    protected static Tile[] GenerateTiles(string name, int n)
    {
        Tile[] ret = new Tile[n];
        for(int i = 0; i < n; i++)
        {
            ret[i] = GenerateTile(name);
            Assert(ret[i] != null);
        }
        return ret;
    }
    public abstract class Tileset
    {
        public virtual bool HasStarter{get => false;}
        public virtual Tile Starter{get => null;}
        public abstract uint NDefaultTiles{get;}
        public virtual uint NMaxTiles{get => NDefaultTiles;}
        public virtual uint NMinTiles{get => NDefaultTiles;}
        public virtual uint NTileStep{get => 0;}
        protected abstract List<Tile> _GenerateTiles(uint n);
        public virtual List<Tile> GenerateTiles(uint n)
        {
            if(n != NDefaultTiles)
            {
                Assert(n <= NMaxTiles && n >= NMinTiles, "Invalid number of tiles for this tileset");
                Assert(Abs(n - NDefaultTiles) % NTileStep == 0, "Invalid number of tiles for this tileset (step size not respected)");
            }
            var ret = _GenerateTiles(n);
            Assert(ret.Count == n);
            return ret;
        }
        public virtual List<Tile> GenerateTiles()
        {
            return GenerateTiles(NDefaultTiles);
        }
    }
}