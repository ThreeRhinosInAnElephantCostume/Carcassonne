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
            Debug.Assert(!(NQueued == 0 && current == null), "Attempting to retrieve a tile when there are none available!");

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
        public List<Tile> PeekTiles(int n)
        {
            Debug.Assert(TileQueue.Count >= n, "Attempting to peek more tiles than there are in queue");

            return TileQueue.GetRange(0, n).ToList<Tile>();
        }
        public Tile PeekTile()
        {
            Debug.Assert(TileQueue.Count > 0, "Attempting to peek an empty tile queue");

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
    protected TileManager tilemanager {get; set;}
    protected RNG rng{get; set;}
    List<Action> _history = new List<Action>();
    protected Dictionary<Player, int> basescore = new Dictionary<Player, int>();
    protected List<Player> _players = new List<Player>();
    public Map map{get; protected set;}
    public Player CurrentPlayer{get; protected set;}
    void AddPlayer()
    {
        Player p = new Player(this);
        basescore.Add(p, 0);
        _players.Add(p);
        if(CurrentPlayer == null)
            CurrentPlayer = p;
    }
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
    void SetPlayerScore(Player player, int val)
    {
        basescore[player] = val;
    }
    Player NextPlayer()
    {
        CurrentPlayer = PeekNextPlayer();
        return CurrentPlayer;
    }
    protected Engine()
    {   
        Debug.Assert(Engine.tilesource != null);
    }
}