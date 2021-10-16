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
    public static TilePrototype LoadTilePrototype(string path)
    {
        if (!path.Contains("res://"))
            path = ConcatPaths(Constants.TILE_DIRECTORY, path);

        Assert(path.EndsWith(".json"));
        Assert(FileExists(path));

        string data = ReadFile(path);
        TilePrototype tp = JsonConvert.DeserializeObject<TilePrototype>(data);

        Assert(tp != null);

        tp.MetaData = path;

        return tp;
    }
    public static Tile LoadTile(string path)
    {
        return LoadTilePrototype(path).Convert();
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
