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

public static class TileDataLoader
{
    public class TileModel
    {
        public PackedScene Scene;
        public TileGraphicsConfig.Config Config;
    }
    public static GameExternalDataLoader GlobalLoader = new GameExternalDataLoader();
    static Dictionary<string, TilePrototype> _prototypeCache = new Dictionary<string, TilePrototype>();
    static Dictionary<string, EditableTileset> _tilesetCache = new Dictionary<string, EditableTileset>();
    static Dictionary<string, List<TileModel>> _modelsByPrototypeCache = new Dictionary<string, List<TileModel>>();
    public static List<TileModel> LoadPrototypeModels(string protpath, bool cached = true)
    {
        lock (_modelsByPrototypeCache)
        {
            if (cached && _modelsByPrototypeCache.ContainsKey(protpath))
                return _modelsByPrototypeCache[protpath].ToList();
        }
        TilePrototype prot = LoadTilePrototype(protpath);
        List<TileModel> models = new List<TileModel>();
        foreach (var modpath in prot.AssociatedModels)
        {
            if (!FileExists(modpath))
            {
                GD.PrintErr("Model not found: " + modpath);
                continue;
            }
            TileModel mod = new TileModel();
            string modconfpath = modpath.Replace(".tscn", ".json");
            if (!FileExists(modconfpath))
            {
                GD.Print("Model config not found: " + modconfpath);
                continue;
            }
            TileGraphicsConfig conf = DeserializeFromFile<TileGraphicsConfig>(modconfpath);
            if (!conf.Configs.ContainsKey(protpath))
            {
                GD.PrintErr("Model does not mention ");
                continue;
            }
            mod.Config = conf.Configs[protpath];
            var loader = ResourceLoader.LoadInteractive(modpath);
            loader.Wait();

            mod.Scene = (PackedScene)loader.GetResource();
            models.Add(mod);
        }
        if (models.Count == 0)
        {
            GD.PrintErr("No models found for TilePrototype: " + protpath);
        }
        if (cached)
        {
            lock (_modelsByPrototypeCache) _modelsByPrototypeCache.Add(protpath, models);
        }
        return models;
    }
    public static TilePrototype LoadTilePrototype(string path, bool failsafe = false, bool cache = true)
    {
        if (!path.Contains("res://"))
            path = ConcatPaths(Constants.TILE_DIRECTORY, path);

        lock (_prototypeCache)
        {
            if (cache && _prototypeCache.ContainsKey(path))
                return _prototypeCache[path];
        }

        if (failsafe)
        {
            if (!FileExists(path) || !path.EndsWith(".json"))
                return null;
        }
        Assert(path.EndsWith(".json"));
        Assert(FileExists(path));

        string data = ReadFile(path);
        TilePrototype tp = JsonConvert.DeserializeObject<TilePrototype>(data);

        if (failsafe && tp == null)
            return null;

        Assert(tp != null);

        tp.MetaData = path;

        if (cache)
        {
            lock (_prototypeCache)
            {
                _prototypeCache.Add(path, tp);
            }
        }


        return tp;
    }
    public static Tile LoadTile(string path, bool failsafe = false)
    {
        var prot = LoadTilePrototype(path, failsafe);
        if (failsafe && prot == null)
            return null;
        return prot.Convert();
    }
    public static Carcassonne.ITileset LoadTileset(string path, bool cache = true)
    {
        if (!path.Contains("res://"))
            path = ConcatPaths(Constants.TILESET_DIRECTORY, path);

        lock (_tilesetCache)
        {
            if (cache && _tilesetCache.ContainsKey(path))
                return _tilesetCache[path];
        }

        Assert(path.EndsWith(".json"));
        Assert(FileExists(path));

        string data = ReadFile(path);
        EditableTileset tp = JsonConvert.DeserializeObject<EditableTileset>(data);

        Assert(tp != null);

        if (cache)
            lock (_tilesetCache)
            {
                _tilesetCache.Add(path, tp);
            }
        return tp;
    }
}
