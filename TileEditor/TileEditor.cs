using Godot;
using System;

[Tool]
public class TileEditor : Control
{
    enum Mode
    {
        TILES,
        TILESETS
    }
    Mode _mode = Mode.TILES;
    ItemBrowser _folderBrowser;
    ItemBrowser _objectBrowser;
    public override void _Ready()
    {
        _folderBrowser = (ItemBrowser) GetNode("MainContainer/BrowserContainer/FolderBrowser");
        _objectBrowser = (ItemBrowser) GetNode("MainContainer/BrowserContainer/ObjectBrowser");
    }
}
