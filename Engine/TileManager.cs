/* 
    *** TileManager.cs ***
    
    TileManager manages the tile queue.
*/

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
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

namespace Carcassonne
{
    public partial class GameEngine
    {
        /// <summary>
        /// A tile queue with built-in shuffling, skipping, and swapping tiles.
        /// </summary>
        public class TileManager
        {
            Tile current = null;
            GameEngine eng;
            public List<Tile> TileList { get; protected set; } = new List<Tile>();
            public List<Tile> TileQueue { get; protected set; } = new List<Tile>();
            public int NQueued { get => TileQueue.Count; }

            ///<summary>Forces the next tile</summary>
            public void SetNextTile(Tile tile, bool insert = true)
            {
                Assert(tile != null);

                if (!TileList.Contains(tile))
                    TileList.Add(tile);
                if (insert || TileQueue.Count == 0)
                    TileQueue.Insert(0, tile);
                else
                    TileQueue[0] = tile;

            }
            ///<summary>Adds a tile, inserting it either at the end of the queue, or randomly if specified</summary>
            public void AddTile(Tile tile, bool shuffle)
            {
                Assert(tile != null);

                TileList.Add(tile);
                if (shuffle && TileQueue.Count > 0)
                    TileQueue.Insert((int)eng._rng.NextLong(0, TileQueue.Count), tile);
                else
                    TileQueue.Add(tile);
            }
            ///<summary>Adds a list of tiles, equivalent to repeatedly calling AddTile</summary>
            public void AddTiles(List<Tile> tiles, bool shuffle)
            {
                Assert(tiles != null);

                foreach (var it in tiles)
                    AddTile(it, shuffle);
            }
            ///<summary>Shuffle the tile queue</summary>
            public void Shuffle()
            {
                List<Tile> res = new List<Tile>(TileQueue.Count);
                while (TileQueue.Count > 0)
                {
                    int indx = (int)eng._rng.NextLong(0, TileQueue.Count);
                    res.Add(TileQueue[indx]);
                    TileQueue.RemoveAt(indx);
                }
            }
            ///<summary>The current tile, can be null if NextTile was never called or if the Queue has ran out.</summary>
            public Tile CurrentTile()
            {
                return current;
            }
            ///<summary>
            /// Pops the first tile from the list, making it the CurrentTile and returning it. 
            /// Will return null when the Queue is exhaused, but only once. 
            /// Attempting to call the function after it has returned null will result in an assertion failure.
            ///</summary>
            public Tile NextTile()
            {
                Assert(!(NQueued == 0 && current == null), "Attempting to retrieve a tile when there are none available!");

                if (NQueued == 0)
                {
                    current = null;
                }
                else
                {
                    current = TileQueue[0];
                    TileQueue.RemoveAt(0);
                }
                return current;
            }
            ///<summary>
            /// Similar to NextTile, but the current tile is re-inserted at the end of the queue.
            ///</summary>
            public Tile SwapTile()
            {
                Assert(!(NQueued == 0 && current == null));
                if (NQueued <= 1)
                {
                    current = null;
                    if (NQueued > 0)
                        TileQueue.RemoveAt(0);
                }
                else
                {
                    TileQueue.Add(current);
                    NextTile();
                }
                return current;
            }
            ///<summary>See n tiles ahead</summary>
            public List<Tile> PeekTiles(int n)
            {
                Assert(n > 0);
                Assert(TileQueue.Count >= n, "Attempting to peek more tiles than there are in queue");

                return TileQueue.GetRange(0, n).ToList<Tile>();
            }
            ///<summary>See the result of calling NextTile, without actually changing the current tile.</summary>
            public Tile PeekTile()
            {
                Assert(TileQueue.Count > 0, "Attempting to peek an empty tile queue");

                return TileQueue[0];
            }

            public TileManager(GameEngine eng)
            {
                Assert(eng != null);

                this.eng = eng;
            }

        }
    }
}
