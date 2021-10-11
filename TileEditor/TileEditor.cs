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
using Newtonsoft.Json;
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

        _fileManager.Extension = ".json";

        _fileManager.FilterHandle = s =>
        {
            // if (!ResourceLoader.Exists(s))
            //     return false;
            //TilePrototype r = ResourceLoader.Load<TilePrototype>(s, "res://TileEditor/TilePrototype.cs", true);
            //TilePrototype r = GD.Load<TilePrototype>(s);
            var dm = new Directory();
            Assert(dm.FileExists(s));
            var fm = new File();
            Assert(fm.Open(s, File.ModeFlags.Read));
            var dt = fm.GetAsText();
            fm.Close();
            try
            {
                var pt = JsonConvert.DeserializeObject<TilePrototype>(dt);
                return pt != null;
            }
            catch (Exception)
            {
                return false;
            }
        };
        _fileManager.CreateFileHandle = s =>
        {
            TilePrototype r = new TilePrototype();
            // r.ResourceLocalToScene = true;
            // Error err = ResourceSaver.Save(s, r, ResourceSaver.SaverFlags.RelativePaths);
            // Assert(err == Error.Ok);
            // #if DEBUG
            // TilePrototype _r = ResourceLoader.Load<TilePrototype>(s, "res://TileEditor/TilePrototype.cs", true);
            // Assert(_r != null);
            // #endif
            string data = JsonConvert.SerializeObject(r);
            var dm = new Directory();
            Assert(!dm.FileExists(s));
            var fm = new File();
            fm.Open(s, File.ModeFlags.Write);
            fm.StoreString(data);
            fm.Close();

        };
        _fileManager.IsProtectedHandle = s =>
        {
            //Assert(ResourceLoader.Exists(s));
            // Resource r = ResourceLoader.Load<TilePrototype>(s, "res://TileEditor/TilePrototype.cs", true);
            // Assert(r != null);

            // var tp = (TilePrototype)r;
#if DEBUG
            return false;
#endif

            var dm = new Directory();
            Assert(dm.FileExists(s));
            var fm = new File();
            Assert(fm.Open(s, File.ModeFlags.Read));
            var dt = fm.GetAsText();
            fm.Close();
            try
            {
                var pt = JsonConvert.DeserializeObject<TilePrototype>(dt);
                return !pt.UserEditable;
            }
            catch (Exception)
            {
                return true;
            }

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
