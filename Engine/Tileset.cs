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
    public abstract class Tileset
    {
        public abstract uint NDefaultTiles{get;}
        public virtual uint NMaxTiles{get => NDefaultTiles;}
        public virtual uint NMinTiles{get => NDefaultTiles;}
        public virtual uint NTileStep{get => 0;}
        protected abstract List<Tile> _GenerateTiles(uint n);
        public virtual List<Tile> GenerateTiles(uint n)
        {
            if(n != NDefaultTiles)
            {
                if(n > NMaxTiles || n < NMaxTiles)
                    throw new Exception("Invalid number of tiles for this tileset");
                if(Abs(n - NDefaultTiles) % NTileStep != 0)
                    throw new Exception("Invalid number of tiles for this tileset (step size not respected)");
            }
            return _GenerateTiles(n);
        }
        public virtual List<Tile> GenerateTiles()
        {
            return GenerateTiles(NDefaultTiles);
        }
    }
}