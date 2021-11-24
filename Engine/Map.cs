/*
    *** Map.cs ***

    A Map represents a collection of tiles and graphs. 
    Do note that while each tile has its own unique position on a two-dimensional integer plane,
    the Map itself can be thought of as a collection of as a graph (not a Graph), where each 
    tile has four connections that can either connect to another tile, or to nothing (null)
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using Carcassonne;
using ExtraMath;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

namespace Carcassonne
{
    public partial class Map
    {
        Dictionary<Vector2I, Tile> _tilesByPosition = new Dictionary<Vector2I, Tile>();
        List<Tile> _tiles = new List<Tile>();
        public Tile Root { get; protected set; }
        public List<Graph> Graphs = new List<Graph>();
        public Tile this[Vector2I pos]
        {
            get => (_tilesByPosition.ContainsKey(pos)) ? _tilesByPosition[pos] : null;
            protected set
            {
                PlaceTile(value, pos);
            }
        }
        public Tile this[int indx]
        {
            get => _tiles[indx];
        }
        public int Count { get => _tiles.Count; }
        public List<Tile> GetPlacedTiles()
        {
            return _tiles.ToList();
        }

        public List<Tile> GetNeighbours(Vector2I pos)
        {
            List<Tile> Neighbours = new List<Tile>(4);
            foreach (var p in pos.Neighbours)
            {
                if (this[p] != null)
                    Neighbours.Add(this[p]);
            }
            return Neighbours;
        }
        public List<Tile> GetNeighbours(Tile t)
        {
            return GetNeighbours(t.Position);
        }
        protected void PlaceTile(Tile tile, Vector2I pos, bool check)
        {
            if (_tilesByPosition.ContainsKey(pos))
                throw new Exception("Attempted to replace a tile!");
            if (check && !CanPlaceTile(tile, pos))
                throw new Exception("Invalid tile placement!");
            _tiles.Add(tile);
            _tilesByPosition.Add(pos, tile);
            tile.Position = pos;
            Vector2I[] nepos = pos.Neighbours;
            for (int i = 0; i < N_SIDES; i++)
            {
                Tile n = this[pos.Neighbours[i]];
                if (n == null)
                    continue;
                tile.Sides[i].Attach(n.Sides[AbsMod((i + (N_SIDES / 2)), N_SIDES)]);
                tile.Neighbours[i] = n;
            }
            UpdateGraphs(tile);
        }
        public void PlaceTile(Tile tile, Vector2I pos)
        {
            PlaceTile(tile, pos, true);
        }
        public bool CanPlaceTileVerbose(Tile tile, Vector2I pos, out List<List<int>> invalidconnectors, out bool isoutofbounds)
        {
            bool ret = true;
            invalidconnectors = new List<List<int>>((int)N_SIDES);
            for (uint ii = 0; ii < N_SIDES; ii++)
                invalidconnectors.Add(new List<int>((int)N_CONNECTORS));
            isoutofbounds = false;
            Tile[] neighbours = new Tile[N_SIDES];
            int ncount = 0;
            int i = 0;
            int connindex = 0;
            foreach (var p in pos.Neighbours)
            {
                neighbours[i] = this[p];
                if (this[p] != null)
                {
                    ncount++;
                    List<int> ic = new List<int>();
                    if (!tile.Sides[i].CanAttachVerbose(this[p].Sides[(i + (N_SIDES / 2)) % N_SIDES], out ic, ref connindex))
                        ret = false;
                    invalidconnectors[i].AddRange(ic);
                }
                else
                    connindex += (int)N_CONNECTORS;
                i++;
            }
            if (ncount == 0)
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
            List<int> rots = new List<int>();
            for (int i = 0; i < N_SIDES; i++)
            {
                if (CanPlaceTile(tile, pos))
                {
                    rots.Add(i);
                    if (breakfast)
                    {
                        tile.Rotate(-i);
                        return (true, rots);
                    }
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
            foreach (var t in _tiles)
            {
                foreach (var n in t.Position.Neighbours)
                {
                    if (this[n] != null)
                        continue;
                    (bool can, List<int> rots) fit = TryFindFits(tile, n);
                    if (fit.can)
                    {
                        foreach (var it in fit.rots)
                            ret.Add((n, it));
                    }
                }
            }
            return ret;
        }
        void Update()
        {

        }
        public Map(Tile root)
        {
            this.Root = root;
            PlaceTile(root, new Vector2I(0, 0), false);
        }
    }
}
