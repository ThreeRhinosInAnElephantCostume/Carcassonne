using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Threading;
using Carcassonne;
using ExtraMath;
using Godot;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

//[Tool]
public class TileEditor : Control
{
    GroupedFileManager _fileManager;
    TileLogicEditor _logicEditor;
    public override void _Ready()
    {
        _fileManager = (GroupedFileManager)GetNode("MainContainer/BrowserContainer");

        _logicEditor = (TileLogicEditor)GetNode("MainContainer/TabContainer/TileLogicEditor");

        Assert(_fileManager != null);
        Assert(_logicEditor != null);

        _fileManager.FilterHandle = s =>
        {
            if (!ResourceLoader.Exists(s))
                return false;
            Resource r = ResourceLoader.Load(s, "TilePrototype");
            return (r != null && (r as TilePrototype) != null);
        };
        _fileManager.CreateFileHandle = s =>
        {
            Resource r = new TilePrototype();
            Error err = ResourceSaver.Save(s, r, ResourceSaver.SaverFlags.BundleResources);
            Assert(err == Error.Ok);
        };
        _fileManager.IsProtectedHandle = s =>
        {
            Assert(ResourceLoader.Exists(s));
            Resource r = ResourceLoader.Load(s, "TilePrototype");
            Assert(r != null && (r as TilePrototype) != null);

            var tp = (TilePrototype)r;

            return !tp.UserEditable;

        };
        _fileManager.FileOpenHandle = s =>
        {
            Assert(new Directory().FileExists(s));
            _logicEditor.Path = s;
        };
        _fileManager.FileCloseHandle = s =>
        {
            _logicEditor.Path = "";
        };
        _fileManager.Path = Constants.TILE_DIRECTORY;
    }
}
