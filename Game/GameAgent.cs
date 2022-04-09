// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
        public GameAgent(Game game, string name, PlayerType type, GameEngine.Player player, PersonalTheme theme = null)
        {
            this._game = game;
            this.Name = name;
            this.Player = player;
            this.Type = type;
            this.CurrentTheme = theme;
            if (this.CurrentTheme == null)
            {
                this.CurrentTheme = Globals.PersonalThemesList[(int)player.ID % Globals.PersonalThemesList.Count];
            }
        }
    }
}
