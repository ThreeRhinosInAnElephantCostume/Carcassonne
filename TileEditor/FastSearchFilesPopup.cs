using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
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
public class FastSearchFilesPopup : WindowDialog
{
    LineEdit _search;
    ItemList _list;
    Button _selectButton;
    Button _cancelButton;
    string _path = "";
    List<string> _files = new List<string>();
    public string Path { get => _path; set { _path = value; SetPath(_path); } }
    public Func<string, bool> FilterHandle = s => true;
    public Action<string, bool> CompleteHandle; //result, !cancelled
    void SelfDestruct()
    {
        this.PopupExclusive = false;
        this.GetParent().RemoveChild(this);
        QueueFree();
    }
    List<string> LookupFiles(string path)
    {
        List<string> ret = new List<string>(64);
        Directory dm = new Directory();
        void LookRecursively(string path)
        {
            foreach (var it in ListDirectoryContents(path))
            {
                string itpath = ConcatPaths(path, it);
                if (dm.DirExists(itpath))
                {
                    LookRecursively(itpath);
                }
                else if (dm.FileExists(itpath))
                {
                    if (FilterHandle(itpath))
                    {
                        ret.Add(itpath);
                    }
                }
            }
        }

        LookRecursively(path);

        return ret;
    }
    void FilterBy(string filter)
    {
        List<string> files = LookupFiles(Path);
        filter = filter.ToLower();
        if (filter != "")
        {
            if (filter.Contains(" "))
            {
                var split = filter.Split(" ").ToList();
                files = files.FindAll
                (
                    (string s) =>
                    {
                        return s.ToLower().Contains(filter) || (split.Find(ssp => s.ToLower().Contains(ssp)) != default(string));
                    }
                );
            }
            else
                files = files.FindAll(s => s.Contains(filter));
        }
        _files = files;
        _list.Clear();
        if (files.Count == 0)
        {
            _selectButton.Disabled = true;
            return;
        }
        foreach (var it in files)
        {
            _list.AddItem(it);
        }
    }
    void SearchChanged(string s)
    {
        FilterBy(s);
    }
    void Activated(int indx)
    {
        string path = _files[indx];
        Assert(FilterHandle(path));
        CompleteHandle(path, true);
        SelfDestruct();
    }
    void Selected(int indx)
    {
        _selectButton.Disabled = false;
    }
    void Deselected()
    {
        _selectButton.Disabled = true;
    }
    void ActivatedCurrent()
    {
        Assert(_list.IsAnythingSelected());
        Activated(_list.GetSelectedItems()[0]);
    }
    void Cancelled()
    {
        CompleteHandle("", false);
        SelfDestruct();
    }
    void SetPath(string path)
    {
        if (_search == null)
        {
            CallDeferred("SetPath", path);
            return;
        }
        _path = path;
        Reset();
        if (path == null || path == "")
            return;


        _search.Connect("text_changed", this, "SearchChanged");

        FilterBy("");
    }
    void Reset()
    {
        if (_search.IsConnected("text_changed", this, "SearchChanged"))
            _search.Disconnect("text_changed", this, "SearchChanged");
        _list.Clear();
        _search.Text = "";
        _selectButton.Disabled = true;
    }
    public override void _Ready()
    {
        this.GetCloseButton().Disabled = true;
        this.PopupExclusive = true;

        _search = (LineEdit)GetNode("VBoxContainer/HBoxContainer/SearchBar");
        _list = (ItemList)GetNode("VBoxContainer/ItemList");
        _cancelButton = (Button)GetNode("VBoxContainer/HBoxContainer2/Cancel");
        _selectButton = (Button)GetNode("VBoxContainer/HBoxContainer2/Select");

        _list.AllowReselect = true;
        _selectButton.Disabled = true;

        _list.Connect("item_activated", this, "Activated");
        _list.Connect("item_selected", this, "Selected");
        _list.Connect("nothing_selected", this, "Deselected");
        _cancelButton.Connect("pressed", this, "Cancelled");
        _selectButton.Connect("pressed", this, "ActivatedCurrent");


        Reset();
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
