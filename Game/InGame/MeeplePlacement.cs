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
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

public class MeeplePlacement : Spatial
{
	MeshInstance _mesh;
	Game.GameAgent _agent;
	void UpdateColor()
	{
		RepeatN(_mesh.GetSurfaceMaterialCount(), i =>
		{
			var mat = _mesh.Mesh.SurfaceGetMaterial(i) as SpatialMaterial;
			if (mat == null)
				return;
			mat.ResourceLocalToScene = true;
			mat = (SpatialMaterial)mat.Duplicate(true);
			mat.AlbedoColor = _agent.BaseColor;
			mat.DepthEnabled = false;
			mat.RenderPriority = 1;
			mat.FlagsNoDepthTest = true;
			_mesh.SetSurfaceMaterial(i, mat);
		});
	}
	public int Index { get; set; }
	public bool IsAttribute { get; set; }
	public Game.GameAgent Agent
	{
		get => _agent; set
		{
			_agent = value;
			if (_mesh != null)
				UpdateColor();
		}
	}
	public override void _Ready()
	{
		_mesh = GetNode<MeshInstance>("meeple/Mesh");
		if (Agent != null)
			UpdateColor();
	}

	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	//  public override void _Process(float delta)
	//  {
	//      
	//  }
}
