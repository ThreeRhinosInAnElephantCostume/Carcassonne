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
		SEARCHING_FOR_FILES = 20,
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
	PackedScene _ingamescene = null;	
	
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
		_ingamescene = ResourceLoader.Load<PackedScene>("res://Game/InGame/InGameUI.tscn");
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
		_loadingLabel = GetNode<Label>("VBoxContainer/VBoxContainer/LoadingLabel");
		_progressBar = GetNode<ProgressBar>("VBoxContainer/VBoxContainer/HBoxContainer/LoadingProgressBar");
		ThreadPool.QueueUserWorkItem(LoadControlThread);
		
		PackedScene AudioPlayerScene = ResourceLoader.Load("res://Audio/AudioPlayer.tscn") as PackedScene;				
		AddChild(AudioPlayerScene.Instance());
		_gameAudio = (AudioPlayer)GetNode("AudioPlayer");
	}
	public override void _Process(float delta)
	{
		float progress = SLIDER_STEP * ((float)EnumPrev(_step));
		
		switch (_step)
		{
			case LoadSteps.NONE:
				{
					_loadingLabel.Text = "Initializing...";
					break;
				}
			case LoadSteps.SEARCHING_FOR_FILES:
				{
					_loadingLabel.Text = "Indexing files...";
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
					progress += rt * (((float)LoadSteps.LOADING_TILES - (float)EnumPrev(LoadSteps.LOADING_TILES)) / (float)LoadSteps.END);
					break;
				}
			case LoadSteps.COMPLETE:
				{
					var igs = _ingamescene.Instance();
					var t = GetTree();
					var r = t.Root;
					r.AddChild(igs);
					DestroyNode(this);
					return;
				}
		}
		_progressBar.Value = (_progressBar.MaxValue * progress);
		_gameAudio.PlayIntroMusic(7);
		if(_progressBar.Value > 80){
			_gameAudio.SetIntroVolume(-1*(float)_progressBar.Value+80);
		
		}
	}
}
