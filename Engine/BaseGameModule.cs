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

    public class BaseGameTileset : Tileset
    {
        public override uint NDefaultTiles{get => 72;}
        protected override List<Tile> _GenerateTiles(uint n)
        {
            List<Tile> l = new List<Tile>()
            {
                new Tile()
            };
            return l;
        }
        
    }
    public class BaseGameModule : Module
    {
        public class PlaceTileAction : Action
        {
            public Vector2I pos {get; protected set;}
            public override Type moduletype{get=>typeof(BaseGameModule);}
            public override void _Execute(State state, Module mod)
            {
                state.History.Add(this);
            }
            public void Fill(Vector2I pos)
            {
                IsFilled = true;
                this.pos = pos;
            }   
        }
        enum PlaceState
        {
            ERROR=0,
            OK,
            OutOfBounds,
            BadConnection,
            Illegal,
            OtherDisallowed
        }
        List<Tile> AllTiles = new List<Tile>();
        public List<Tile> TileQueue = new List<Tile>();
        public void QueueTiles(List<Tile> tiles, bool randomise)
        {
            AllTiles.AddRange(tiles);
            if(randomise)
            {
                foreach(var it in tiles)
                    TileQueue.Insert((int)state.rng.NextLong(0, TileQueue.Count), it);
            }
            else
                TileQueue.AddRange(tiles);
        }
        public void ShuffleTiles()
        {

        }
        public int RemainingTiles{get => TileQueue.Count;}
        public Tile PopTile()
        {
            if(TileQueue.Count == 0)
                throw new Exception("Attempting to pop a tile when there are none remaining");
            Tile t = TileQueue.Last();
            TileQueue.RemoveAt(TileQueue.Count-1);
            return t;
        }
        public Tile PeekTile()
        {
            return TileQueue.Last();
        }
        public override int Priority {get => 0;}
        public override bool IsCompatible(Type other)
        {
            throw new NotImplementedException();
        }
        public override bool IsCompatible(List<Type> others)
        {
            throw new NotImplementedException();
        }
        public override List<Action> GetBaseActions()
        {

        }
        public override List<Action> GetPossibleActions()
        {
            throw new NotImplementedException();
        }
        public override bool IsDone{get;}
        public override bool IsEnabled{get;}

        public override int GetPlayerEndScore(Player player)
        {
            throw new NotImplementedException();
        }
        public override int GetPlayerProbableScore(Player player)
        {
            throw new NotImplementedException();
        }
        public BaseGameModule(State state, List<Type> others) : base(state, others) 
        {
            Tileset
        }
    }
}