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
    public abstract class GameAgent
    {
        protected Game _game;
        public string Name { get; set; }
        public GameEngine.Player Player { get; protected set; }
        public PlayerType Type { get; protected set; }
        public Color BaseColor { get; set; }
        protected void ExecuteAction(GameEngine.Action action)
        {
            _game.AgentExecute(this, action);
        }
        protected void ExecuteImplied()
        {
            _game.AgentExecuteImplied(this);
        }
        public GameAgent(Game game, string name, PlayerType type, GameEngine.Player player)
        {
            this._game = game;
            this.Name = name;
            this.Player = player;
            this.Type = type;
            if (player.ID >= Game.PlayerColors.Length)
                BaseColor = Game.PlayerColors.Last();
            else
                BaseColor = Game.PlayerColors[player.ID];
        }
    }
}
