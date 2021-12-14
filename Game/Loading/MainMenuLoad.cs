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
	ConcurrentQueue<string> _lastProcessedPaths = new ConcurrentQueue<string>();
	PackedScene _InGameScenePacked = null;
	float _resourceLoadProgress = 0;
	public float Progress { get; protected set; } = 0;
	AudioPlayer _gameAudio;
	void TileLoader(object state)
	{
		string path;
		while (_remainingPrototypePaths.TryDequeue(out path))
		{
			TilePrototype prot = TileDataLoader.LoadTilePrototype(path);
			Assert(prot != null);
			var models = TileDataLoader.LoadPrototypeModels(path);
			Assert(models.Count != 0);
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

		List<(string path, Action<Resource> onend)> scenestoload =
			new List<(string path, Action<Resource> onend)>()
			{
				("res://Game/InGame/InGameUI.tscn", r => _InGameScenePacked = (PackedScene)r)
			};

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
		_prototypePaths = Utils.ListDirectoryFilesRecursively(Constants.TILE_DIRECTORY);

		_step = LoadSteps.LOADING_TILES;
		_remainingPrototypePaths = new ConcurrentQueue<string>(_prototypePaths);
		int nthreads = Max(1, System.Environment.ProcessorCount - 1);
		var events = RepeatN<WaitHandle>(nthreads, i => new AutoResetEvent(false));
		RepeatN(nthreads, i => ThreadPool.QueueUserWorkItem(TileLoader, events[i]));
		AutoResetEvent.WaitAll(events.ToArray());
		Assert(_remainingPrototypePaths.Count == 0);
		while (_lastProcessedPaths.Count > 0)
			Thread.Sleep(1);
		_step = LoadSteps.COMPLETE;
	}
	public override void _Ready()
	{
		_gameAudio = GetNode<AudioPlayer>("/root/AudioPlayer");
		_loadingLabel = GetNode<Label>("VBoxContainer/VBoxContainer/LoadingLabel");
		_progressBar = GetNode<ProgressBar>("VBoxContainer/VBoxContainer/HBoxContainer/LoadingProgressBar");
		ThreadPool.QueueUserWorkItem(LoadControlThread);
	}
	public override void _Process(float delta)
	{
		float stepfactor = (((float)_step - (float)EnumPrev(_step)) / (float)LoadSteps.END);
		float progress = SLIDER_STEP * ((float)EnumPrev(_step));
		switch (_step)
		{
			case LoadSteps.NONE:
				{
					_loadingLabel.Text = "Initializing...";
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
						_loadingLabel.Text = "Loading Tiles: " + lastres;
					}
					int pc = Max(_prototypePaths.Count, 1);
					float rt = 1.0f - ((float)_remainingPrototypePaths.Count / (float)pc);
					progress += rt * stepfactor;
					break;
				}
			case LoadSteps.COMPLETE:
				{
					var igs = _InGameScenePacked.Instance();
					var t = GetTree();
					var r = t.Root;
					r.AddChild(igs);
					DestroyNode(this);
					return;
				}
		}
		_gameAudio.PlayIntroMusic(7);
		_progressBar.Value = (_progressBar.MaxValue * progress);
		Progress = progress;
	}
}
