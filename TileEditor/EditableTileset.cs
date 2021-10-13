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
using Newtonsoft.Json;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

namespace Carcassonne
{
    [Serializable]
    public class EditableTileset : Carcassonne.ITileset
    {
        public bool UserEditable { get; set; }
        public bool SingleStarter { get; set; }
        public int NStarterTiles { get; set; }
        public int NPossibleTiles { get; set; }
        public int NOutputTiles { get; set; }
        public List<string> Tiles { get; set; }
        public List<string> StarterTiles { get; set; }

        public EditableTileset(bool UserEditable)
        {
            this.UserEditable = false;
            SingleStarter = true;
            NStarterTiles = -1;
            NPossibleTiles = 0;
            NOutputTiles = 0;
            Tiles = new List<string>(8);
            StarterTiles = new List<string>(8);
        }

        bool ITileset.HasStarter => (NStarterTiles > 0 || (SingleStarter && StarterTiles.Count > 0));

        int ITileset.NPossibleStarters => (SingleStarter && StarterTiles.Count > 0) ? 1 : StarterTiles.Count;

        int ITileset.NPossibleTiles => NPossibleTiles;

        int ITileset.NTiles => NOutputTiles;

        Tile ITileset.GenerateStarter(RNG rng)
        {
            Assert(!(SingleStarter && NStarterTiles > 1));
            if (SingleStarter)
            {
                Assert(StarterTiles.Count > 0 && StarterTiles[0] != null);
                return TileDataLoader.LoadTile(StarterTiles[0]);
            }
            else if (NStarterTiles == 0)
            {
                return null;
            }
            return TileDataLoader.LoadTile(StarterTiles[(int)rng.NextLong(0, StarterTiles.Count) % StarterTiles.Count]);
        }

        List<Tile> ITileset.GenerateTiles(RNG rng)
        {
            Assert(Tiles != null);
            Assert(Tiles.Count > 0);
            Assert(NOutputTiles > 0);
            Assert(NPossibleTiles > 0);

            List<TilePrototype> tocreate = new List<TilePrototype>(NOutputTiles);
            if (NOutputTiles >= Tiles.Count)
            {
                tocreate.AddRange(Tiles.ConvertAll(s => TileDataLoader.LoadTilePrototype(s)));
            }
            while (tocreate.Count < NOutputTiles)
            {
                tocreate.Add(TileDataLoader.LoadTilePrototype(Tiles[(int)rng.NextLong(0, Tiles.Count)]));
            }

            List<Tile> ret = new List<Tile>(tocreate.Count);
            foreach (var it in tocreate)
            {
                ret.Add(it.Convert());
            }
            return ret;
        }
    }
}
