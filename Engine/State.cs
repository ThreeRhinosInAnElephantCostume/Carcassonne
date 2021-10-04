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
    public abstract class Action
    {
        public bool IsFilled{get; protected set;} = false;
        public abstract Type moduletype{get;}
        public abstract void _Execute(State state, Module mod);
        public virtual void Execute(State state, Module mod)
        {
            state.History.Add(this);
            _Execute(state, mod);
        }
    }
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
    public abstract class Module
    {
        public class IncompatibleModuleException : Exception
        {
            public IncompatibleModuleException(Type t0, Type t1) 
                : base("Incompatible module combination (" + t0.Name + " with " + t1.Name + ")") {}
        }
        public State state {get; protected set;}
        public abstract int Priority {get;}
        public abstract bool IsCompatible(Type other);
        public abstract bool IsCompatible(List<Type> others);
        public abstract List<Action> GetBaseActions();
        public abstract List<Action> GetPossibleActions();
        public abstract bool IsDone{get;}
        public abstract bool IsEnabled{get;}

        public abstract int GetPlayerEndScore(Player player);
        public abstract int GetPlayerProbableScore(Player player);
        public abstract void PostAction(Action lastaction);
        public void Init()
        {

        }
        public Module(State state, List<Type> others)
        {
            this.state = state;
            List<Type> cmptester = new List<Type>();
            foreach(var it in others)
            {
                cmptester.Add(it);
                if(!IsCompatible(it))
                    throw new IncompatibleModuleException(it, this.GetType());
                if(!IsCompatible(cmptester))
                    throw new IncompatibleModuleException(it, this.GetType());
            }
        }

    }
    public class State
    {
        public List<Action> History{get; protected set;} = new List<Action>();
        public RNG rng{get; protected set;}
        protected Dictionary<Player, int> basescore = new Dictionary<Player, int>();
        protected List<Player> _players = new List<Player>();
        public List<Player> Players {get => _players.ToList(); protected set => _players = value;}
        public Map map;
        public List<Module> modules = new List<Module>();
        public List<Module> EnabledModules 
        {
            get 
            {
                List<Module> res = new List<Module>(modules.Count);
                foreach(var it in modules)
                {
                    if(it.IsEnabled)
                        res.Add(it);
                }
                return res;
            }
        }
        public List<Action> GetBaseActions()
        {
            List<Action> ret = new List<Action>();
            foreach(var it in EnabledModules)
            {
                ret.AddRange(it.GetBaseActions());
            }
            return ret;
        }
        public List<Action> GetPossibleActions()
        {
            List<Action> ret = new List<Action>();
            foreach(var it in EnabledModules)
            {
                ret.AddRange(it.GetPossibleActions());
            }
            return ret;
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
            int res = 0;
            foreach(var it in EnabledModules)
            {
                res += it.GetPlayerEndScore(player);
            }
            return res;
        }
        public int GetPlayerProbableScore(Player player)
        {
            int res = 0;
            foreach(var it in EnabledModules)
            {
                res += it.GetPlayerProbableScore(player);
            }
            return res;
        }
        public bool IsGameOver
        {
            get
            {
                foreach(var it in EnabledModules)
                {
                    if(!it.IsDone)
                        return false;
                }
                return true;
            }
        }
        bool _initialized = false;
        public void Initialize()
        {
            if(_initialized)
                throw new Exception("Attempting to reinitialize a state");
            modules.Sort
            ( 
                (Module m0, Module m1)=>
                {
                    return m0.Priority.CompareTo(m1.Priority);
                } 
            );
            foreach(var it in modules)
            {
                it.Init();
            }
        }
        public State(ulong seed)
        {
            this.rng = new RNG(seed);
        }
    }
}