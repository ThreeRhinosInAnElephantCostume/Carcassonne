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
    public class TileManager
    {
        Tile current = null;
        Engine eng;
        public List<Tile> TileList {get; protected set;}= new List<Tile>();
        public List<Tile> TileQueue {get; protected set;} = new List<Tile>();
        public int NQueued{get => TileQueue.Count;}
        public void AddTile(Tile tile, bool shuffle)
        {
            if(shuffle)
                TileQueue.Insert((int)eng.rng.NextLong(0, TileQueue.Count), tile);
            else
                TileQueue.Add(tile);
        }
        public void AddTiles(List<Tile> tiles, bool shuffle)
        {
            foreach(var it in tiles)
                AddTile(it, shuffle);
        }
        public void Shuffle()
        {
            List<Tile> res = new List<Tile>(TileQueue.Count);
            while(TileQueue.Count > 0)
            {
                int indx = (int)eng.rng.NextLong(0, TileQueue.Count);
                res.Add(TileQueue[indx]);
                TileQueue.RemoveAt(indx);
            }
        }
        public Tile CurrentTile()
        {
            return current;
        }
        public void SkipTile()
        {
            Tile c = current;
            NextTile();
            if(NQueued == 0)
                TileQueue.Add(c);
            else
                TileQueue.Insert(1, c);
        }
        public Tile NextTile()
        {
            if(NQueued == 0 && current == null)
                throw new Exception("Attempting to retrieve more tiles than available!");
            if(NQueued == 0)
            {
                current = null;
            }
            else 
            {
                current = TileList[0];
                TileList.RemoveAt(0);
            }
            return current;
        }
        public List<Tile> PeekTiles(uint n)
        {
            if(TileQueue.Count == 0)
                throw new Exception("Attempting to peek an empty tile queue");
            return TileQueue.GetRange(0, (int)Min(n, TileQueue.Count)).ToList<Tile>();
        }
        public Tile PeekTile()
        {
            if(TileQueue.Count == 0)
                throw new Exception("Attempting to peek an empty tile queue");
            return TileQueue[0];
        }

        public TileManager(Engine eng)
        {
            this.eng = eng;
        }
        
    }
    public enum State
    {
        ERR=0,
        NONE,
        PLACE_TILE,
        PLACE_PAWN,
        GAME_OVER
    }
    public TileManager tilemanager {get; protected set;}
    public RNG rng{get; protected set;}
    public bool IsGameOver { get; protected set; }
    public State CurrentState{get; protected set;}
    public uint turn = 0;
    public List<Action> History{get; protected set;} = new List<Action>();
    protected Dictionary<Player, int> basescore = new Dictionary<Player, int>();
    protected List<Player> _players = new List<Player>();
    public List<Player> Players {get => _players.ToList(); protected set => _players = value;}
    public Map map{get; protected set;}
    public Player CurrentPlayer{get; protected set;}
    void AssertState(Player curplayer, State state)
    {
        if(CurrentPlayer != curplayer)
            throw new Exception("Player assertion failed!");
        if(state != this.CurrentState)
            throw new Exception("State assertion failed. Current state is " + this.CurrentState.ToString() 
            + ", expected " + state.ToString());
    }
    void AssertState(Player curplayer)
    {
        AssertState(curplayer, this.CurrentState);
    }
    void AssertState(State state)
    {
        AssertState(this.CurrentPlayer, state);
    }
    public void SetPlayerScore(Player player, int val)
    {
        basescore[player] = val;
    }
    public int GetPlayerScore(Player player)
    {
        return basescore[player];
    }
    public int GetPlayerEndScore(Player player)
    {
        throw new NotImplementedException();
    }
    public int GetPlayerProbableScore(Player player)
    {
        throw new NotImplementedException();
    }


    protected Engine()
    {
        this.rng = new RNG(0);
    }
}