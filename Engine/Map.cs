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
        List<Tile> alltiles = new List<Tile>();
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
            get => alltiles[indx];
        }
        public int Count {get=>alltiles.Count;}
        public List<Tile> GetPlacedTiles()
        {
            return alltiles.ToList();
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

        public void PlaceTile(Tile tile, Vector2I pos)
        {
            if(tilesbyposition.ContainsKey(pos))
                throw new Exception("Attempted to replace a tile!");
            if(!CanPlaceTile(tile, pos))
                throw new Exception("Invalid tile placement!");
            alltiles.Add(tile);
            tilesbyposition.Add(pos, tile);
            tile.position = pos;
            Vector2I[] nepos = pos.Neigbours;
            for(int i = 0; i < N_SIDES; i++)
            {
                Tile n = this[pos];
                if(n == null)
                    continue;
                tile.sides[i].Attach(n.sides[(i+(N_SIDES/2)) % N_SIDES]);
                tile.neighbours[i] = n;
            }
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
                    ncount++;
                else
                    connindex += (int)N_CONNECTORS;
                List<int> ic;
                if(!tile.sides[i].CanAttachVerbose(this[p].sides[(N_SIDES + (N_SIDES/2)) % N_SIDES], out ic, ref connindex))
                    ret = false;
                invalidconnectors[i].AddRange(ic);
                i++;
            }
            if(ncount == 0)
            {
                isoutofbounds = true;
                return false;
            }
            return true;
        }
        public bool CanPlaceTile(Tile tile, Vector2I pos)
        {
            List<List<int>> invalidconnections;
            bool isoutofbounds;
            return CanPlaceTileVerbose(tile, pos, out invalidconnections, out isoutofbounds);
        }
        public bool TryFindFit(Tile tile, Vector2I pos, out int rotation)
        {
            rotation = 0;
            for(uint i = 0; i < N_SIDES; i++)
            {
                if(CanPlaceTile(tile, pos))
                {
                    rotation = (int)i;
                    tile.Rotate(-rotation);
                    return true;
                }
                tile.Rotate(1);
            }
            return false;
        }
    }
}