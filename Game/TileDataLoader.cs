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
    public static GameExternalDataLoader Loader = new GameExternalDataLoader();
    public static TilePrototype LoadTilePrototype(string path, bool failsafe = false)
    {
        if (!path.Contains("res://"))
            path = ConcatPaths(Constants.TILE_DIRECTORY, path);

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

        return tp;
    }
    public static Tile LoadTile(string path, bool failsafe = false)
    {
        var prot = LoadTilePrototype(path, failsafe);
        if (failsafe && prot == null)
            return null;
        return prot.Convert();
    }
    public static Carcassonne.ITileset LoadTileset(string path)
    {
        if (!path.Contains("res://"))
            path = ConcatPaths(Constants.TILESET_DIRECTORY, path);

        Assert(path.EndsWith(".json"));
        Assert(FileExists(path));

        string data = ReadFile(path);
        EditableTileset tp = JsonConvert.DeserializeObject<EditableTileset>(data);

        Assert(tp != null);

        return tp;
    }
}
