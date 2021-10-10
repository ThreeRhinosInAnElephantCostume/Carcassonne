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
public class GroupedFileManager : HBoxContainer
{
    ItemBrowser _folderBrowser;
    ItemBrowser _objectBrowser;
    public string CurrentDirectory {get; protected set;}
    public string CurrentFile {get; protected set;} 
    public Func<string, bool> FilterHandle = s => s.Length > 0;
    public Action<string> _createFileHandle = s => throw new NotImplementedException();
    public Action<string> CreateFileHandle 
    {
        get => _createFileHandle; 
        set => _createFileHandle = s => value(EnsurePath(s, false)); 
    }
    public Action<string> _fileOpenHandle = s => throw new NotImplementedException();
    public Action<string> FileOpenHandle 
    {
        get => _fileOpenHandle; 
        set => _fileOpenHandle = s => value(EnsurePath(s)); 
    }
    public Action<string> _fileCloseHandle = s => throw new NotImplementedException();
    public Action<string> FileCloseHandle
    {
        get => _fileCloseHandle; 
        set => _fileCloseHandle = s => value(EnsurePath(s)); 
    }
    public Func<string, bool> IsProtectedHandle = s => false;
    string _path = ""; 
    public string Path{get => _path; set { _path = value; SetPath(value);}}
    bool browsersenabled = false;
    string EnsurePath(string p, bool check = true)
    {
        if(!p.Contains("res://"))
            p = ConcatPaths(Path, CurrentDirectory, p);

        if(check)
            Assert(new Directory().FileExists(p));

        return p;
    }

    void LoadFiles(string path)
    {
       _objectBrowser.Reset();

       List<string> files = ListDirectoryFiles(path).FindAll(s => FilterHandle(ConcatPaths(path, s)));
       foreach(var it in files)
       {
           _objectBrowser.AddItem(it, IsProtectedHandle(ConcatPaths(path, it)));
       }
    }
    void LoadDirs(string path)
    {
        _folderBrowser.Reset();
        _objectBrowser.Reset();
        
        List<string> subdirs = ListDirectorySubDirs(path);

        foreach(var it in subdirs)
        {
            _folderBrowser.AddItem(it, 
                ListDirectoryFiles(ConcatPaths(path, it)).Find(
                    s => FilterHandle(s) && IsProtectedHandle(s)) 
                == default(string));
        }
    }
    void SetPath(string path)
    {
        browsersenabled = false;

        _path = path;
        
        _folderBrowser.Reset();
        _objectBrowser.Reset();

        if(CurrentFile != null && CurrentFile != "")
            FileCloseHandle(CurrentFile);    

        CurrentFile = "";
        CurrentDirectory = "";

        LoadDirs(path);

        browsersenabled = true;
    }
    public override void _Ready()
    {
        browsersenabled = false;
        _folderBrowser = (ItemBrowser)GetNode("FolderBrowser");
        _objectBrowser = (ItemBrowser)GetNode("ObjectBrowser");

        _folderBrowser.CloningEnabled = false;
        
        _folderBrowser.CloneHandle = (string name, string nname) =>
        {
            return false;
        };

        _folderBrowser.NewHandle = (string nname) =>
        {
            if(!browsersenabled)
                return false;

            string p = ConcatPaths(Path, nname);

            Directory dm = new Directory();
            Assert(!dm.DirExists(p));

            dm.MakeDir(p);
            
            return true;
        };

        _folderBrowser.DeleteHandle = (string name) => 
        {
            if(!browsersenabled)
                return false;

            string p = ConcatPaths(Path, CurrentDirectory, name);

            Directory dm = new Directory();
            Assert(dm.FileExists(p));
            Assert(!IsProtectedHandle(name));

            List<string> files = ListDirectoryFiles(p).FindAll(s => FilterHandle(s));

            Assert(files.FindAll(s => IsProtectedHandle(s)).Count == 0);

            foreach(var it in files)
            {
                if(it == CurrentFile)
                {
                    FileCloseHandle(CurrentFile);
                    CurrentFile = "";
                }
                dm.Remove(it);
            }

            dm.Remove(p);

            return true;
        };

        _folderBrowser.SelectHandle = (string nname) =>
        {   
            if(nname == null)
                nname = "";
            if(CurrentFile != null && CurrentFile != "")
                FileCloseHandle(CurrentFile);    
            CurrentFile = "";

            CurrentDirectory = nname;
            LoadFiles(ConcatPaths(Path, CurrentDirectory));
        };

        _objectBrowser.CloneHandle = (string name, string nname) =>
        {
            if(!browsersenabled)
                return false;
            Assert(CurrentDirectory != "");
            Assert(name != nname);

            string from = ConcatPaths(Path, CurrentDirectory, nname);
            string to = ConcatPaths(Path, CurrentDirectory, name);

            Directory dm = new Directory();

            Assert(!dm.FileExists(to));
            Assert(dm.FileExists(from));

            dm.Copy(from, to);
            return true;    
        };

        _objectBrowser.NewHandle = (string nname) =>
        {
            if(!browsersenabled)
                return false;

            string p = ConcatPaths(Path, CurrentDirectory, nname);

            Directory dm = new Directory();
            Assert(!dm.FileExists(p));

            CreateFileHandle(p);
            
            return true;
        };

        _objectBrowser.DeleteHandle = (string name) => 
        {
            if(!browsersenabled)
                return false;
            if(name == CurrentFile)
            {
                FileCloseHandle(CurrentFile);
            }

            string p = ConcatPaths(Path, CurrentDirectory, name);

            Directory dm = new Directory();
            Assert(dm.FileExists(p));
            Assert(!IsProtectedHandle(name));

            dm.Remove(p);

            return true;
        };

        _objectBrowser.SelectHandle = (string nname) =>
        {   
            if(nname == null)
                nname = "";
            if(CurrentFile != null && CurrentFile != "")
                FileCloseHandle(CurrentFile);    
            CurrentFile = nname;
            
            FileOpenHandle(CurrentFile);
        };


    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
