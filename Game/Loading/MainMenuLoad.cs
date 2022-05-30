

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Threading;
using ExtraMath;
using Godot;
using Newtonsoft.Json;
using static System.Math;
using static Utils;
using Expression = System.Linq.Expressions.Expression;
using Thread = System.Threading.Thread;


public class MainMenuLoad : Control
{
    public enum LoadSteps
    {
        AWAITING_INPUT = -2,
        COMPLETE = -1,
        NONE = 0,
        LOADING_RESOURCES = 35,
        SEARCHING_FOR_FILES = 40,
        LOADING_TILES = 100,
        END = LOADING_TILES,
    }
    const float SLIDER_STEP = 1.0f / (float)LoadSteps.END;
    LoadSteps _step = LoadSteps.NONE;
    ProgressBar _progressBar;
    Label _loadingLabel;
    List<string> _prototypePaths = new List<string>();
    ConcurrentQueue<string> _remainingPrototypePaths = new ConcurrentQueue<string>();
    readonly ConcurrentQueue<string> _lastProcessedPaths = new ConcurrentQueue<string>();
    float _resourceLoadProgress = 0;
    public float Progress { get; protected set; } = 0;
    AudioPlayer _gameAudio;
    void TileLoader(object state)
    {
        string path;
        while (_remainingPrototypePaths.TryDequeue(out path))
        {
            GD.Print(Thread.CurrentThread.ManagedThreadId, ": " + path);
            TilePrototype prot = TileDataLoader.LoadTilePrototype(path);
            Assert(prot != null, $"Could not deserialize {path}");
            var models = TileDataLoader.LoadPrototypeModels(path);
            Assert(models.Count != 0, $"Tile {path} does not have any models assigned!");
            _lastProcessedPaths.Enqueue(path);
        }
        (state as AutoResetEvent).Set();
    }
    void LoadControlThread(object state)
    {
        _step = LoadSteps.NONE;

        _step = LoadSteps.LOADING_RESOURCES;
        int scene_steps = 0;
        int scene_steps_completed = 0;

        List<(string path, Action<Resource> onend)> scenestoload = LoadFrom.ToLoad;
        scenestoload.ForEach(it => Assert(FileExists(it.path), "Could not find resource: " + it.path));
        var loaders
            = scenestoload.ConvertAll<(ResourceInteractiveLoader loader, Action<Resource> onend)>(it =>
                (ResourceLoader.LoadInteractive(it.path), it.onend));
        loaders.ForEach(it => scene_steps += it.loader.GetStageCount());

        foreach (var it in loaders)
        {
            while (it.loader.Poll() != Error.FileEof)
            {
                _resourceLoadProgress = ((float)(scene_steps_completed + it.loader.GetStage())) / ((float)scene_steps);
            }
            scene_steps_completed += it.loader.GetStageCount();
            it.onend(it.loader.GetResource());
        }

        _step = LoadSteps.SEARCHING_FOR_FILES;
        _prototypePaths = Utils.ListDirectoryFilesRecursively(Constants.DataPaths.TILE_DIRECTORY);

        _step = LoadSteps.LOADING_TILES;
        _remainingPrototypePaths = new ConcurrentQueue<string>(_prototypePaths);
        int nthreads = Max(1, System.Environment.ProcessorCount - 1);
        nthreads = 1;
        var events = RepeatN<WaitHandle>(nthreads, i => new AutoResetEvent(false));
        RepeatN(nthreads, i => ThreadPool.QueueUserWorkItem(TileLoader, events[i]));
        AutoResetEvent.WaitAll(events.ToArray());
        Assert(_remainingPrototypePaths.Count == 0);
        while (_lastProcessedPaths.Count > 0)
            Thread.Sleep(1);
        Thread.Sleep(100);
        _step = LoadSteps.COMPLETE;
    }
    public override void _Ready()
    {
        _gameAudio = GetNode<AudioPlayer>("/root/AudioPlayer");

        _loadingLabel = GetNode<Label>("VBoxContainer/VBoxContainer/LoadingLabel");
        _progressBar = GetNode<ProgressBar>("VBoxContainer/VBoxContainer/HBoxContainer/LoadingProgressBar");
        ThreadPool.QueueUserWorkItem(LoadControlThread);
    }
    bool _skipWait = false;
    public override void _Process(float delta)
    {
        if (Input.IsKeyPressed((int)KeyList.Enter) || Input.IsKeyPressed((int)KeyList.Escape))
            _skipWait = true;
        float stepfactor = (((float)_step - (float)EnumPrev(_step)) / (float)LoadSteps.END);
        float progress = SLIDER_STEP * ((float)EnumPrev(_step));
        switch (_step)
        {
            case LoadSteps.NONE:
                {
                    _loadingLabel.Text = "Initializing...";
                    _gameAudio.StopMainMenuMusic();
                    _gameAudio.PlayIntroMusic(0);
                    break;
                }
            case LoadSteps.LOADING_RESOURCES:
                {
                    _loadingLabel.Text = "Loading Resources...";
                    progress += _resourceLoadProgress * stepfactor;
                    break;
                }
            case LoadSteps.SEARCHING_FOR_FILES:
                {
                    _loadingLabel.Text = "Indexing Files...";
                    break;
                }
            case LoadSteps.LOADING_TILES:
                {
                    string lastres = "";
                    if (_lastProcessedPaths.TryDequeue(out lastres))
                    {
                        _loadingLabel.Text = "Loading Tiles: " + lastres.Split('/').Last().Replace(".tscn", "");
                    }
                    int pc = Max(_prototypePaths.Count, 1);
                    float rt = 1.0f - ((float)_remainingPrototypePaths.Count / (float)pc);
                    progress += rt * stepfactor;
                    break;
                }
            case LoadSteps.COMPLETE:
                {
                    _gameAudio.StopIntroMusic();
                    _gameAudio.PlayMainMenuMusic(0);
                    _loadingLabel.Text = "PRESS <ENTER> TO CONTINUE";
                    _progressBar.Visible = false;
                    _step = LoadSteps.AWAITING_INPUT;
                    break;
                }
            case LoadSteps.AWAITING_INPUT:
                {
                    if (_skipWait || Input.IsActionJustPressed("ui_accept") || Input.IsActionPressed("ui_accept") ||
                        Input.IsKeyPressed((int)KeyList.Tab) || Input.IsMouseButtonPressed((int)ButtonList.Left) ||
                        Input.IsMouseButtonPressed((int)ButtonList.Right) || Input.IsMouseButtonPressed((int)ButtonList.Middle))
                    {
                        var scene = Globals.Scenes.MainMenuPacked.Instance();
                        GetTree().Root.AddChild(scene);
                        DestroyNode(this);
                        return;
                    }
                    break;
                }
        }


        _progressBar.Value = (_progressBar.MaxValue * progress);
        Progress = progress;
    }
}
