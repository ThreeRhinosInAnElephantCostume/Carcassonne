

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

public partial class Game
{
    public abstract class GameAgent
    {
        [JsonIgnore]
        protected Game _game { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public GameEngine.Player Player { get; protected set; }
        public PlayerType Type { get; protected set; }
        [JsonIgnore]
        public PersonalTheme CurrentTheme { get; protected set; }
        protected void ExecuteAction(GameEngine.Action action)
        {
            _game.AgentExecute(this, action);
        }
        protected void ExecuteImplied()
        {
            _game.AgentExecuteImplied(this);
        }
        public abstract void OnTurn(GameEngine engine);
        public static GameAgent DeserializeFromDirectory(Game game, Player player, string path)
        {
            GameAgent agent = Utils.DeserializeFromFile<GameAgent>(ConcatPaths(path, "AgentData.json"));
            agent._game = game;
            agent.Player = player;
            agent.CurrentTheme = PersonalTheme.DeserializeFromDirectory(ConcatPaths(path, "Theme"));
            return agent;
        }
        public void SerializeToDirectory(string path)
        {
            SerializeToFile<object>(ConcatPaths(path, "AgentData.json"), this, true, true);
            CurrentTheme.SerializeToDirectory(ConcatPaths(path, "Theme"));
        }
        public GameAgent(Game game, string name, PlayerType type, GameEngine.Player player, PersonalTheme theme = null)
        {
            this._game = game;
            this.Name = name;
            this.Player = player;
            this.Type = type;
            this.CurrentTheme = theme;
            if (this.CurrentTheme == null && player != null)
            {
                this.CurrentTheme = Globals.PersonalThemesList[(int)player.ID % Globals.PersonalThemesList.Count];
            }
        }
    }
}
