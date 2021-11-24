/* 
    *** Engine.cs ***
    
    Most of the engine's internal state is defined here.
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
    ///<summary>A mostly self-contained simulation of Carcassonne.</summary>
    public partial class GameEngine
    {
        public enum State
        {
            ///<summary>This value should never occur.</summary>
            ERR = 0,
            ///<summary>This value should indicate that the engine is in an uninitialized state.</summary>
            NONE,
            ///<summary>It's the CurrentPlayer's turn to place a tile.</summary>
            PLACE_TILE,
            ///<summary>It's the CurrentPlayer's turn to place a pawn or skip.</summary>
            PLACE_PAWN,
            ///<summary>The game has ended (there are no more tiles queued, and the current player can't place any more pawns).</summary>
            GAME_OVER
        }
        protected TileManager _tileManager { get; set; }
        protected RNG _rng { get; set; }
        List<Action> _history = new List<Action>();
        protected List<Player> _players = new List<Player>();
        public Map map { get; protected set; }
        public Player CurrentPlayer { get; protected set; }
        private int _nextUniqueID = 0;
        protected IExternalDataSource _dataSource;
        protected ITileset _tileset;
        ///<summary>
        ///    Returns an ID unique to the current instance of the engine, obtained by incrementing a counter.
        ///</summary>
        public int NextUniqueID()
        {
            return _nextUniqueID++;
        }
        void AddPlayer()
        {
            Player p = new Player(this, _players.Count);
            _players.Add(p);
            if (CurrentPlayer == null)
                CurrentPlayer = p;
        }
        void AssertState(Player curplayer, State state)
        {
            if (CurrentPlayer != curplayer)
                throw new Exception("Player assertion failed!");
            if (state != this.CurrentState)
                throw new InvalidStateException(CurrentState, state);
        }
        void AssertState(Player curplayer)
        {
            AssertState(curplayer, this.CurrentState);
        }
        void AssertState(State state)
        {
            AssertState(this.CurrentPlayer, state);
        }
        void AssertRule(bool b, string msg)
        {
            if (!b)
                throw new IllegalMoveException(msg);
        }
        Player NextPlayer(bool nextturn = true)
        {
            CurrentPlayer = PeekNextPlayer();
            if (nextturn)
                Turn++;
            return CurrentPlayer;
        }
        ///<summary>Returns a deep clone of the engine.</summary>
        public GameEngine Clone()
        {
            return CreateFromHistory(_dataSource, this.History);
        }
        ///<summary>Returns a deep clone of the engine, before the latest action.</summary>
        public GameEngine StepBack(int steps = 1)
        {
            Assert(steps < this.History.Count);
            return CreateFromHistory(_dataSource, this.History.GetRange(0, this.History.Count - steps));
        }

        protected GameEngine(IExternalDataSource source)
        {
            this._dataSource = source;
            Assert(this.actionmethods.Length > 0);
        }
    }
}
