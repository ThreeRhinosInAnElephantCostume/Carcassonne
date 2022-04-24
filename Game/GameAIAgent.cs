

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
            AI.MakeMove(engine);
            this._game.AgentExecuteImplied(_game.CurrentAgent);
        }
        public GameAIAgent(Game game, string name, GameEngine.Player player, AI.AIPlayer AI, PersonalTheme theme) : base(game, name, PlayerType.AI, player, theme)
        {
            Assert(AI != null);
            this.AI = AI;
        }
    }
}
