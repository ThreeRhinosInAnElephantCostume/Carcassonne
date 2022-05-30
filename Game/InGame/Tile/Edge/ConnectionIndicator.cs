using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Enumeration;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using Carcassonne;
using ExtraMath;
using Godot;
using static System.Math;
using static Utils;

public class ConnectionIndicator : Spatial
{
    MeshInstance _mainMesh;
    MeshProp _ownerIndicator;

    NodeType _type = NodeType.ERR;
    public NodeType Type
    {
        get => _type; set
        {
            if (_type == value)
                return;
            _type = value;
            Defer(UpdateNodeType);
        }
    }
    void UpdateNodeType()
    {
        if (Type == NodeType.ERR)
        {
            _mainMesh.Visible = false;
            return;
        }
        var mat = (SpatialMaterial)_mainMesh.GetActiveMaterial(0);
        var col = GetNodeTypeColor(Type);
        mat.AlbedoColor = col;
        _mainMesh.Visible = true;
        mat.EmissionEnabled = true;
        mat.Emission = col;
    }
    Game.GameAgent _owner = null;
    public Game.GameAgent OwningAgent
    {
        get => _owner;
        set
        {
            _owner = value;
            Defer(UpdateOwner);
        }
    }
    void UpdateOwner()
    {
        if (OwningAgent == null)
        {
            _ownerIndicator.Visible = false;
            return;
        }
        (_ownerIndicator as IProp).CurrentTheme = OwningAgent.CurrentTheme;
        _ownerIndicator.Visible = true;
    }

    public override void _Ready()
    {
        _mainMesh = this.GetNodeSafe<MeshInstance>("MeshInstance");
        _ownerIndicator = this.GetNodeSafe<MeshProp>("OwnerIndicator");

        _mainMesh.Visible = false;
        _ownerIndicator.Visible = false;
    }
}
