using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Carcassonne;
using ExtraMath;
using Godot;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

public partial class Game
{
    public class GameAIAgent : GameAgent
    {
        public AI.AIPlayer AI { get; protected set; }
        public override void OnTurn(GameEngine engine)
        {
            Assert(this._game.CurrentAgent.Player == this.Player);
            ThreadPool.QueueUserWorkItem(o =>
            {
                DateTime start = DateTime.Now;
                GameEngine clone;
                clone = engine.Clone();
                AI.MakeMove(clone);

                // Ensure that at least Constants.MINIMUM_AI_DELAY_S has passed
                var seconds = (DateTime.Now - start).TotalSeconds;
                var dif = Constants.MINIMUM_AI_DELAY_S - seconds;
                if (dif > 0)
                    System.Threading.Thread.Sleep((int)(dif * 1000f));

                Defer(() => this._game.AgentExecute(this, clone.History.Last()));
            }, null);
        }
        public GameAIAgent(Game game, string name, GameEngine.Player player, AI.AIPlayer AI, PersonalTheme theme) : base(game, name, PlayerType.AI, player, theme)
        {
            Assert(AI != null);
            this.AI = AI;
        }
    }
}
