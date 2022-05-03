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
    public enum GameMode
    {
        LOCAL,
        MULTIPLAYER_CLIENT,
        MULTIPLAYER_SERVER,
    }
    public enum GameState
    {
        CONNECTING,
        LOBBY,
        LOADING,
        SYNCHRONIZING,
        AWAITING_MOVE,
        ENDED,
    }
    public enum PlayerType
    {
        LOCAL,
        NETWORK,
        AI
    }
    public interface IGameHandles
    {
        void OnAction(GameAgent agent, GameEngine.Action action);
        void OnNextPlayerTurn(GameAgent agent);
        void OnGameOver(List<GameAgent> winners);
    }
    protected ulong _seed;
    public IGameHandles Handles { get; set; }
    public GameEngine Engine { get; protected set; }
    public GameMode Mode { get; protected set; }
    public GameState State { get; protected set; }
    public bool IsMultiplayer => (Mode == GameMode.MULTIPLAYER_CLIENT || Mode == GameMode.MULTIPLAYER_SERVER);
    public List<GameAgent> Agents { get; protected set; } = new List<GameAgent>();
    public GameAgent CurrentAgent => (State == GameState.ENDED) ? null : GetAgent(Engine.CurrentPlayer);
    RNG _rng;
    public static Color[] PlayerColors { get; set; } = new Color[]
    {
        new Color(1f, 0.3f, 0.3f),
        new Color(0.5f, 0.5f, 1f),
        new Color(1, 0.5f, 1f),
        new Color(0.7f, 1, 0.7f),
        new Color(1f, 1f, 1f),
        new Color(0.5f, 0.5f, 0.5f),
    };
    public void UpdateEngine(GameAgent agent)
    {
        Handles.OnAction(agent, Engine.History.Last());
        if (Engine.CurrentState == GameEngine.State.GAME_OVER)
        {
            State = GameState.ENDED;
            Handles.OnGameOver(Engine.GetWinners().ConvertAll<GameAgent>(it => GetAgent(it)));
            SaveStatistics(true);
        }
        else
        {
            State = GameState.AWAITING_MOVE;
            Handles.OnNextPlayerTurn(CurrentAgent);
            Defer(() => CurrentAgent.OnTurn(Engine));
        }
        if (Engine.Turn > 5)
            SaveStatistics(true);
    }
    public GameAgent GetAgent(GameEngine.Player p)
    {
        return Agents.Find(it => it.Player == p);
    }
    public void AgentExecute(GameAgent agent, GameEngine.Action action)
    {
        Engine.ExecuteAction(action);
        UpdateEngine(agent);
    }
    public void AgentExecuteImplied(GameAgent agent) // an action has already been executed
    {
        UpdateEngine(agent);
    }
    public delegate GameAgent AgentGenerator(Game game, GameEngine engine, int indx, GameEngine.Player player, RNG rng);
    public static Game NewLocalGame(IGameHandles handles, List<AgentGenerator> agentGenerators, string tileset, ulong seed)
    {
        Assert(agentGenerators.Count > 1);
        int players = agentGenerators.Count;
        Game game = new Game(handles);
        game._seed = seed;
        game._rng = new RNG(new RNG(seed).NextULong() + seed);
        game.Mode = GameMode.LOCAL;
        game.State = GameState.AWAITING_MOVE;
        game.Engine = GameEngine.CreateBaseGame(TileDataLoader.GlobalLoader, seed, players, tileset);
        game.Agents = new List<GameAgent>();
        for (int i = 0; i < agentGenerators.Count; i++)
        {
            game.Agents.Add(agentGenerators[i](game, game.Engine, i, game.Engine.Players[i], game._rng));
        }
        return game;
    }
    Game(IGameHandles handles)
    {
        this.Handles = handles;
    }

}
