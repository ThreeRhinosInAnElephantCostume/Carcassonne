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

[Tool]
public class ItemBrowser : VBoxContainer
{   
    public Func<string, bool> CloneHandle = s => false;
    public Func<string, bool> NewHandle = s => true;
    public Action<string> SelectHandle = s => {};
    public Action<string> OpenHandle = s => {};
    public Func<string, bool> DeleteHandle;
    Button _newButton;
    Button _deleteButton;
    Button _cloneButton;
    ItemList _list;
    NameDialog _dialog;
    List<bool> _deletability = new List<bool>();
    List<string> _items = new List<string>();
    string _selected = null;
    public string Selected{get => _selected; protected set {_selected = value; SetEditable(_selected != null); }}
    int _selectedIndex = -1;
    void SetEditable(bool b)
    {
        _deleteButton.Disabled = !b;
        _cloneButton.Disabled = !b;
    }
    void SetInteractable(bool b)
    {
        SetEditable(b);
        _newButton.Disabled = !b;
        _list.PauseMode = (b) ? PauseModeEnum.Process : PauseModeEnum.Stop;
    }
    public void AddItem(string name, bool deletable=true)
    {
        _list.AddItem(name);
    }
    public void AddItems(List<string> names, bool deletable=true)
    {
        foreach(var it in names)
            AddItem(it, deletable);
    }
    void ItemSelected(int indx)
    {
        Selected = _items[indx];
        _selectedIndex = indx;
        if(SelectHandle != null)
            SelectHandle(_items[indx]);
    }
    void NothingSelected()
    {
        Selected = null;
        _selectedIndex = -1;
        if(SelectHandle != null)
            SelectHandle(null);
       
    }
    bool IsValidName(string s)
    {
        if(s.Length < 1)
            return false;
        var l = s.ToList();
        if(l.FindIndex(0, ch => char.IsWhiteSpace(ch)) != -1)
            return false;
        if(l.FindIndex(0, ch => !char.IsLetterOrDigit(ch) && ch != '_') != -1)
            return false;
        return true;
    }
    void CloneCurrent()
    {
        Assert(Selected != null && _selectedIndex != -1);

        _dialog = new NameDialog();

        _dialog.ChangedHandle = s => (IsValidName(s) , "Single word, only letters, digits, and \'_\'");
        _dialog.CompleteHandle = s => 
        {
            if(CloneHandle(s))
            {
                AddItem(s);
            }
            SetInteractable(true);
            _dialog.QueueFree();
            RemoveChild(_dialog);
            _dialog = null;
        };
        AddChild(_dialog);

        SetInteractable(false);
        _dialog.Popup_();
    }
    void DeleteCurrent()
    {
        Assert(Selected != null && _selectedIndex != -1);

        if(_deletability[_selectedIndex] && DeleteHandle(Selected))
        {
            int indx = _selectedIndex;
            _items.RemoveAt(indx);
            _deletability.RemoveAt(indx);
            _selectedIndex = -1;
            Selected = null;
            _list.RemoveItem(indx);
        }
    }
    void New()
    {

        _dialog = new NameDialog();

        _dialog.ChangedHandle = s => (IsValidName(s) , "Single word, only letters, digits, and \'_\'");
        _dialog.CompleteHandle = s => 
        {
            if(NewHandle(s))
            {
                AddItem(s);
            }
            SetInteractable(true);
            _dialog.QueueFree();
            RemoveChild(_dialog);
            _dialog = null;
        };
        AddChild(_dialog);

        SetInteractable(false);
        _dialog.Popup_();
    }
    public override void _Ready()
    {
        var buttoncontainer = FindChild<HBoxContainer>(this);
        if(null == (_cloneButton = (Button)buttoncontainer.FindNode("CloneButtob")))
        {
            _cloneButton = new Button();
            buttoncontainer.AddChild(_cloneButton);
        }
        if(null == (_deleteButton = (Button)buttoncontainer.FindNode("DeleteButton")))
        {
            _deleteButton = new Button();
            buttoncontainer.AddChild(_deleteButton);
        }
        if(null == (_newButton = (Button)buttoncontainer.FindNode("NewButton")))
        {
            _newButton = new Button();
            buttoncontainer.AddChild(_newButton);
        }
        _cloneButton.Text = "Clone";
        _cloneButton.Connect("pressed", this, "CloneCurrent");
        _deleteButton.Disabled = true;

        _deleteButton.Text = "Delete";
        _deleteButton.Connect("pressed", this, "DeleteCurrent");
        _deleteButton.Disabled = true;
        
        _newButton.Text = "New";
        _newButton.Connect("pressed", this, "New");
        _newButton.Disabled = false;

        _list = FindChild<ItemList>(this);
        _list.Connect("nothing_selected", this, "NothingSelected");
    }

    public override void _Process(float delta)
    {
        
    }
}
