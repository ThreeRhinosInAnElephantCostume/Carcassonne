using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
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

    public delegate GameAgent AgentGenerator(Game game, GameEngine engine, int indx, GameEngine.Player player, RNG rng);
    public class SerializableState
    {
        public GameMode Mode { get; set; }
        public GameState State { get; set; }

        public int NAgents { get; set; }

        public void Set(Game game)
        {
            game.Mode = Mode;
            game.State = State;
        }
        public SerializableState()
        {

        }
        public SerializableState(Game game)
        {
            this.Mode = game.Mode;
            this.State = game.State;
            this.NAgents = game.Agents.Count;
        }
    }
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
    public string GameName { get; protected set; }
    public IGameHandles Handles { get; set; }
    public GameEngine Engine { get; protected set; }
    public GameMode Mode { get; protected set; }
    public GameState State { get; protected set; }
    public bool IsMultiplayer => (Mode == GameMode.MULTIPLAYER_CLIENT || Mode == GameMode.MULTIPLAYER_SERVER);
    public List<GameAgent> Agents { get; protected set; } = new List<GameAgent>();
    public GameAgent CurrentAgent => (State == GameState.ENDED) ? null : GetAgent(Engine.CurrentPlayer);
    RNG _rng { get; set; }
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
        // if (Engine.Turn > 5)
        //     SaveStatistics(true);
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
    public void SaveToFile(string savepath)
    {
        string temppath = GetTemporaryDirectory();

        EnsurePathExists(temppath);

        if (Engine.Turn > 1)
            SaveStatistics(true, ConcatPaths(temppath, "STATISTICS"));

        byte[] engdata = this.Engine.Serialize();
        WriteFile(ConcatPaths(temppath, Constants.DataPaths.SAVE_ENGINE_FILE), engdata);

        WriteFile(ConcatPaths(temppath, Constants.DataPaths.SAVE_NAME_FILE), GameName);
        WriteFile(ConcatPaths(temppath, Constants.DataPaths.SAVE_RNG_FILE), _rng.Serialize());

        var ss = new SerializableState(this);
        SerializeToFile(ConcatPaths(temppath, Constants.DataPaths.SAVE_EXTRA_FILE), ss, true, true);

        for (int i = 0; i < Agents.Count; i++)
        {
            var ag = Agents[i];
            string apath = ConcatPaths(temppath, Constants.DataPaths.SAVE_AGENTS_SUBFOLDER, i.ToString());
            EnsurePathExists(apath);
            ag.SerializeToDirectory(apath);
        }
        EnsurePathExists(savepath.GetBaseDir());
        ZipFile.CreateFromDirectory(GetRealUserDirectory(temppath), GetRealUserDirectory(savepath));
        DeleteDirectoryRecursively(temppath);
    }
    public static Game LoadLocalGame(IGameHandles handles, string savepath)
    {
        Assert(FileExists(savepath));
        string temppath = GetTemporaryDirectory();
        ZipFile.ExtractToDirectory(GetRealUserDirectory(savepath), GetRealUserDirectory(temppath));
        byte[] engdata = ReadFileBytes(ConcatPaths(temppath, Constants.DataPaths.SAVE_ENGINE_FILE));

        GameEngine engine = GameEngine.Deserialize(TileDataLoader.GlobalLoader, engdata);

        string name = ReadFile(ConcatPaths(temppath, Constants.DataPaths.SAVE_NAME_FILE));
        RNG rng = RNG.Deserialize(ReadFile(ConcatPaths(temppath, Constants.DataPaths.SAVE_RNG_FILE)));

        SerializableState ss = DeserializeFromFile<SerializableState>(ConcatPaths(temppath, Constants.DataPaths.SAVE_EXTRA_FILE));

        var game = new Game(name, handles);

        ss.Set(game);
        game._rng = rng;
        game.Engine = engine;
        game.Agents = new List<GameAgent>();

        for (int i = 0; i < ss.NAgents; i++)
        {
            string apath = ConcatPaths(temppath, Constants.DataPaths.SAVE_AGENTS_SUBFOLDER, i.ToString());
            GameAgent ga = GameAgent.DeserializeFromDirectory(game, engine.Players[i], apath);
            game.Agents.Add(ga);
        }

        return game;
    }
    public static Game NewLocalGame(IGameHandles handles, List<AgentGenerator> agentGenerators, string tileset, ulong seed)
    {
        Assert(agentGenerators.Count > 1);
        int players = agentGenerators.Count;
        string name = $"LOCAL_{players}_PLAYERS_{GetFormattedTimeNow()}";
        Game game = new Game(name, handles);
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
    Game(string name, IGameHandles handles)
    {
        this.Handles = handles;
        this.GameName = name;
    }

}
