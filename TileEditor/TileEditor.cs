

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
    TileGraphicsEditor _graphicsEditor;
    TabContainer _editorContainer;
    public void TabChanged(int indx)
    {
        void SetInputRecursively(Node node, bool b)
        {
            node.SetProcessInput(b);
            foreach (var it in node.GetChildren())
            {
                SetInputRecursively((Node)it, b);
            }
        }
        foreach (var it in _editorContainer.GetChildren())
        {
            if (it is Control con)
            {
                SetInputRecursively(con, false);
            }
        }
        var sel = _editorContainer.GetChild(indx);
        Assert(sel is Control);
        SetInputRecursively(sel, true);
    }
    public override void _Ready()
    {
        _editorContainer = (TabContainer)GetNode("MainContainer/TabContainer");
        _fileManager = (GroupedFileManager)GetNode("MainContainer/BrowserContainer");
        _logicEditor = (TileLogicEditor)GetNode("MainContainer/TabContainer/TileLogicEditor");
        _graphicsEditor = (TileGraphicsEditor)GetNode("MainContainer/TabContainer/TileGraphicsEditor");

        Assert(_editorContainer != null);
        Assert(_fileManager != null);
        Assert(_logicEditor != null);
        Assert(_graphicsEditor != null);

        _editorContainer.Connect("tab_changed", this, "TabChanged");
        TabChanged(_editorContainer.CurrentTab);

        _fileManager.Extension = ".json";
        _fileManager.SortAlphabetically = true;

        _fileManager.FilterHandle = s =>
        {
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
#if DEBUG
            return false;
#else

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
#endif

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
        _fileManager.Path = Constants.DataPaths.TILE_DIRECTORY;

        _logicEditor.TileChangedHandle = (t, p) => _graphicsEditor.SetTile(t, _logicEditor.Path);
    }
}
