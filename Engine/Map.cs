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
    public class Map
    {
        Dictionary<Vector2I, Tile> tilesbyposition = new Dictionary<Vector2I, Tile>();
        List<Tile> tiles = new List<Tile>();
        public Tile root {get; protected set;}
        public Tile this[Vector2I pos]
        {
            get => (tilesbyposition.ContainsKey(pos))?tilesbyposition[pos] : null; 
            protected set 
            { 
                PlaceTile(value, pos);
            }
        }
        public Tile this[int indx]
        {
            get => tiles[indx];
        }
        public int Count {get=>tiles.Count;}
        public List<Tile> GetPlacedTiles()
        {
            return tiles.ToList();
        }

        public List<Tile> GetNeighbours(Vector2I pos)
        {   
            List<Tile> Neighbours =  new List<Tile>(4);
            foreach(var p in pos.Neigbours)
            {
                if(this[p] != null)
                    Neighbours.Add(this[p]);
            }
            return Neighbours;
        }   
        public List<Tile> GetNeighbours(Tile t)
        {
            return GetNeighbours(t.position);
        }
        protected void PlaceTile(Tile tile, Vector2I pos, bool check)
        {
            if(tilesbyposition.ContainsKey(pos))
                throw new Exception("Attempted to replace a tile!");
            if(check && !CanPlaceTile(tile, pos))
                throw new Exception("Invalid tile placement!");
            tiles.Add(tile);
            tilesbyposition.Add(pos, tile);
            tile.position = pos;
            Vector2I[] nepos = pos.Neigbours;
            for(int i = 0; i < N_SIDES; i++)
            {
                Tile n = this[pos.Neigbours[i]];
                if(n == null)
                    continue;
                tile.sides[i].Attach(n.sides[(i+(N_SIDES/2)) % N_SIDES]);
                tile.neighbours[i] = n;
            }
        }
        public void PlaceTile(Tile tile, Vector2I pos)
        {
            PlaceTile(tile, pos, true);
        }
        public bool CanPlaceTileVerbose(Tile tile, Vector2I pos, out List<List<int>> invalidconnectors, out bool isoutofbounds)
        {
            bool ret = true;
            invalidconnectors = new List<List<int>>((int)N_SIDES);
            for(uint ii = 0; ii < N_SIDES; ii++)
                invalidconnectors.Add(new List<int>((int)N_CONNECTORS));
            isoutofbounds = false;
            Tile[] neighbours = new Tile[N_SIDES];
            int ncount = 0;
            int i = 0;
            int connindex = 0;
            foreach(var p in pos.Neigbours)
            {
                neighbours[i] = this[p];
                if(this[p] != null)
                {
                    ncount++;
                    List<int> ic = new List<int>();
                    if(!tile.sides[i].CanAttachVerbose(this[p].sides[(N_SIDES + (N_SIDES/2)) % N_SIDES], out ic, ref connindex))
                        ret = false;
                    invalidconnectors[i].AddRange(ic);
                }
                else
                    connindex += (int)N_CONNECTORS;
                i++;
            }
            if(ncount == 0)
            {
                isoutofbounds = true;
                return false;
            }
            return ret;
        }
        public bool CanPlaceTile(Tile tile, Vector2I pos)
        {
            List<List<int>> invalidconnections;
            bool isoutofbounds;
            return CanPlaceTileVerbose(tile, pos, out invalidconnections, out isoutofbounds);
        }
        public (bool can, List<int> rots) TryFindFits(Tile tile, Vector2I pos, bool breakfast = false)
        {
            int rotation = 0;
            List<int> rots = new List<int>();
            for(uint i = 0; i < N_SIDES; i++)
            {
                if(CanPlaceTile(tile, pos))
                {
                    rotation = (int)i;
                    tile.Rotate(-rotation);
                    rots.Add(rotation);
                    if(breakfast)
                        return (true, rots);
                }
                tile.Rotate(1);
            }
            return (rots.Count > 0, rots);
        }
        public (bool can, int rot) TryFindFit(Tile tile, Vector2I pos)
        {
            var f = TryFindFits(tile, pos, true);

            Assert(f.can == (f.rots.Count > 0));

            return (f.can, (f.rots.Count > 0) ? f.rots[0] : 0);
        }
        public List<(Vector2I pos, int rot)> TryFindAllFits(Tile tile)
        {
            List<(Vector2I, int)> ret = new List<(Vector2I, int)>(64);
            foreach(var t in tiles)
            {
                (bool can, List<int> rots) fit = TryFindFits(tile, t.position);
                if(fit.can)
                {
                    foreach(var it in fit.rots)
                        ret.Add((t.position, it));
                }
            }
            return ret;
        }
        void Update()
        {
            
        }
        public Map(Tile root)
        {
            this.root = root;
            PlaceTile(root, new Vector2I(0, 0), false);
        }
    }
}