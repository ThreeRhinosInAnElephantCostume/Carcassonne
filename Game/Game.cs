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
    public IGameHandles Handles { get; set; }
    public GameEngine Engine { get; protected set; }
    public GameMode Mode { get; protected set; }
    public GameState State { get; protected set; }
    public bool IsMultiplayer => (Mode == GameMode.MULTIPLAYER_CLIENT || Mode == GameMode.MULTIPLAYER_SERVER);
    public List<GameAgent> Agents { get; protected set; } = new List<GameAgent>();
    public GameAgent CurrentAgent => (State == GameState.ENDED) ? null : GetAgent(Engine.CurrentPlayer);
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
        }
        else
        {
            State = GameState.AWAITING_MOVE;
            Handles.OnNextPlayerTurn(CurrentAgent);
        }
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
    public static Game NewLocalGame(IGameHandles handles, int players, string tileset)
    {
        Game game = new Game(handles);
        game.Mode = GameMode.LOCAL;
        game.State = GameState.AWAITING_MOVE;
        game.Engine = GameEngine.CreateBaseGame(TileDataLoader.GlobalLoader, 666, players, tileset);
        game.Agents = game.Engine.Players.ConvertAll<GameAgent>(it => new GameLocalAgent(game, $"PLAYER {it.ID}", it));
        return game;
    }

    public static Game NewAIGame(IGameHandles handles, int players, string tileset)
    {
        Game game = new Game(handles);
        game.Mode = GameMode.LOCAL;
        game.State = GameState.AWAITING_MOVE;
        game.Engine = GameEngine.CreateBaseGame(TileDataLoader.GlobalLoader, 666, players, tileset);
        game.Agents.Add(new GameLocalAgent(game, $"PLAYER 1", game.Engine.Players.ElementAt(0)));
        game.Agents.Add(new GameAIAgent(game, $"TEST AI", game.Engine.Players.ElementAt(1)));
        //game.Agents = game.Engine.Players.ConvertAll<GameAgent>(it => new GameAIAgent(game, $"PLAYER {it.ID}", it));
        return game;
    }
    Game(IGameHandles handles)
    {
        this.Handles = handles;
    }

}
