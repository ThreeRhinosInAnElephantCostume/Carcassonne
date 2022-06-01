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
using System.Security.AccessControl;
using System.Threading;
using ExtraMath;
using Godot;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using static System.Math;
using static Utils;
using Expression = System.Linq.Expressions.Expression;
using Thread = System.Threading.Thread;

public class SaveLoadGame : Control
{
    public enum SLMode
    {
        Save,
        Load
    }
    public interface ISaveLoadHandler
    {
        public bool CanShow(string path)
        {
            return true;
        }
        public bool CanDelete(string path);
        public void OnSelected(string path);
        public void OnDelete(string path);
        public void OnCancelled();
    }
    public SLMode Mode { get; protected set; }
    string _rootPath;
    string _currentPath;
    string _previousPath;
    string _selected = "";
    bool _isSelecting = false;
    bool _isDirectorySelected = false;
    ConfirmationDialog _confirmationDialog;
    ItemList _fileList;
    LineEdit _saveNameEdit;
    Button _selectButton;
    Button _deleteButton;
    Button _exitButton;
    string _selectText = "SELECT";
    public string OpenFolderText = "OPEN";
    ISaveLoadHandler _handles;
    readonly List<Action> _onItemActions = new List<Action>();
    public string SelectText
    {
        get => _selectText;
        set
        {
            _selectText = value;
            if (!_isDirectorySelected)
                _selectButton.Text = value;
        }
    }
    void RefreshList()
    {
        _fileList.Clear();
        _onItemActions.Clear();
        if (_currentPath != _rootPath)
        {
            _fileList.AddItem("<---");
            _onItemActions.Add(() => { SelectFile(this._previousPath); });
        }
        var contents = ListDirectoryContents(_currentPath);
        List<string> filepaths = new List<string>(contents.Count);
        foreach (var it in contents)
        {
            if (!_handles.CanShow(it))
                continue;
            if (Utils.IsDirectory(it))
            {
                _fileList.AddItem(it.GetFile());
                _onItemActions.Add(() => SelectFile(it));
            }
            else
            {
                filepaths.Add(it);
            }
        }
        foreach (var it in filepaths)
        {
            string name = it.GetFile();
            _fileList.AddItem(name);
            _onItemActions.Add(() => SelectFile(it));
        }
    }
    void Deselect()
    {
        _selected = "";
        _isSelecting = false;
        _isDirectorySelected = false;
        _selectButton.Text = SelectText;
        _selectButton.Disabled = (Mode == SLMode.Load);
        if (Mode == SLMode.Load)
            _saveNameEdit.Text = "";
    }
    void SelectFile(string path)
    {
        Assert(FileExists(path) || DirectoryExists(path));
        if (IsDirectory(path))
        {
            _isDirectorySelected = true;
            _selectButton.Text = OpenFolderText;
        }
        else
        {
            _isDirectorySelected = false;
            _selectButton.Text = SelectText;
            _saveNameEdit.Text = path.BaseName();
        }
        _deleteButton.Disabled = !_handles.CanDelete(path);
        _selectButton.Disabled = false;
        _selected = path;
        _isSelecting = true;
    }
    void SetPath(string path)
    {
        Assert(DirectoryExists(path), $"{path} not found!");
        this._currentPath = path;
        if (this._currentPath != this._rootPath)
        {
            this._previousPath = path.GetBaseDir();
        }
        Deselect();
        RefreshList();
    }
    void End()
    {
        this.Visible = false;
        this.GetChildrenRecrusively<BaseButton>().ForEach(it => it.Disabled = true);
    }
    void Start(ISaveLoadHandler handles, string root, string path)
    {
        this._rootPath = root;
        this._handles = handles;
        this.Visible = true;
        this.GetChildrenRecrusively<BaseButton>().ForEach(it => it.Disabled = false);
        SetPath(path);
    }
    public void StartSave(ISaveLoadHandler handles, string root, string path, string savename)
    {
        this.Mode = SLMode.Save;
        Start(handles, root, path);
        _saveNameEdit.Text = savename;
        _saveNameEdit.Editable = true;
    }
    public void StartLoad(ISaveLoadHandler handles, string root, string path)
    {
        this.Mode = SLMode.Load;
        Start(handles, root, path);
        _saveNameEdit.Text = "";
        _saveNameEdit.Editable = false;
    }
    void OnSelectPath(string path)
    {
        if (_isDirectorySelected)
        {
            SetPath(_selected);
        }
        else
        {
            _handles.OnSelected(path);
            End();
        }
    }
    void OnSelectPressed()
    {
        if (Mode == SLMode.Save)
        {
            if (!_isSelecting)
            {
                if (_saveNameEdit.Text.Length == 0)
                    return;
                _selected = ConcatPaths(_currentPath, _saveNameEdit.Text);
            }
        }
        OnSelectPath(_selected);
    }
    void OnDeleteConfirmed()
    {
        _handles.OnDelete(_selected);
        if (_isDirectorySelected)
        {
            DeleteDirectoryRecursively(_selected);
        }
        else
        {
            DeleteFile(_selected);
        }
        Deselect();
        RefreshList();
    }
    void OnDeleteCancelled()
    {

    }
    void OnDeletePressed()
    {
        _confirmationDialog.PopupCentered();
    }
    void OnExitPressed()
    {
        End();
        _handles.OnCancelled();
    }
    public override void _Ready()
    {
        _selectButton = this.GetNodeSafe<Button>("Panel/MarginContainer/VBoxContainer/HBoxContainer/SelectButton");
        _selectButton.OnButtonPressed(OnSelectPressed);
        _selectButton.Text = SelectText;

        _deleteButton = this.GetNodeSafe<Button>("Panel/MarginContainer/VBoxContainer/HBoxContainer/DeleteButton");
        _deleteButton.OnButtonPressed(OnDeletePressed);

        _exitButton = this.GetNodeSafe<Button>("Panel/MarginContainer/VBoxContainer/HBoxContainer/ExitButton");
        _exitButton.OnButtonPressed(OnExitPressed);

        _confirmationDialog = this.GetNodeSafe<ConfirmationDialog>("ConfirmationDialog");
        _confirmationDialog.OnAccepted(OnDeleteConfirmed);
        _confirmationDialog.OnCancelled(OnDeleteCancelled);

        // _dialogContainer = this.GetNodeSafe<Control>("DialogContainer");
        // _dialogContainer.Visible = false;

        _saveNameEdit = this.GetNodeSafe<LineEdit>("Panel/MarginContainer/VBoxContainer/SaveNameEdit");
        _saveNameEdit.OnTextChanged(s =>
        {
            string ns = s.Replace("/", "".Replace("\\", "")).Replace(":", "").Replace(".", "");
            if (ns != s)
                _saveNameEdit.Text = ns;
            if (Mode == SLMode.Save)
                _selectButton.Disabled = !(s.Length > 0);
        });
        _saveNameEdit.OnTextSubmitted(s =>
        {
            if (_isSelecting)
                OnSelectPressed();
        });

        _fileList = this.GetNodeSafe<ItemList>("Panel/MarginContainer/VBoxContainer/MarginContainer/ItemList");
        _fileList.OnItemActivated(indx =>
        {
            _onItemActions[indx]();
            OnSelectPressed();
        });
        _fileList.OnItemSelected(indx => _onItemActions[indx]());
    }
}
