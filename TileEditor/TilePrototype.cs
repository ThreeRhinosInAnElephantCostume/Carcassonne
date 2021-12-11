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

[Serializable]
public class TilePrototype
{
	public int[] NodeTypes { get; set; }
	public int[] Assignments { get; set; }
	public bool UserEditable { get; set; }
	public Dictionary<int, List<int>> NodeAttributes { get; set; } = new Dictionary<int, List<int>>();
	public List<int> TileAttributes { get; set; } = new List<int>();
	public List<string> AssociatedModels { get; set; } = new List<string>();

	public object MetaData;

	public static Vector2[] RelativeConnectorPositions
	{
		get
		{
			Vector2 size = new Vector2(1, 1);
			Vector2[] ret = new Vector2[N_CONNECTORS * N_SIDES];
			Vector2[] edges = new Vector2[4] {-size/2, new Vector2(size.x/2, -size.y/2),
				size/2, new Vector2(-size.x/2, size.y/2)};
			Vector2[] dirs = new Vector2[4] { Vector2.Right, Vector2.Down, Vector2.Left, Vector2.Up };

			for (int i = 0; i < GameEngine.N_SIDES; i++)
			{
				for (int ii = 0; ii < GameEngine.N_CONNECTORS; ii++)
				{
					ret[i * N_CONNECTORS + ii] = edges[i] + (((float)ii + 1) / ((float)N_CONNECTORS + 1)) * (dirs[i]);
				}
			}
			return ret;
		}
	}

	public bool IsValid
	{
		get
		{
			return
				(Assignments != null && NodeTypes != null) &&
				(Assignments.Length == GameEngine.N_CONNECTORS * GameEngine.N_SIDES) &&
				(NodeTypes.Length > 0) &&
				(!NodeTypes.ToList().Contains((int)NodeType.ERR)) &&
				(Assignments.ToList().FindAll(i => (i < 0 || i >= NodeTypes.Length)).Count == 0);
		}
	}

	public Tile Convert()
	{
		Check();

		Assert(NodeTypes != null);
		Assert(NodeTypes.Length > 0);
		Assert(Assignments != null);
		Assert(Assignments.Length == 12);

		List<(NodeType tp, List<NodeAttributeType> attrs)> nodetypes = new List<(NodeType tp, List<NodeAttributeType> attrs)>();

		for (int i = 0; i < NodeTypes.Length; i++)
		{
			nodetypes.Add(((NodeType)NodeTypes[i],
				NodeAttributes[i].ConvertAll(it => (NodeAttributeType)it)));
		}

		var t = new Tile(nodetypes, Assignments.ToList(), TileAttributes.ConvertAll(it => (TileAttributeType)it));
		t.MetaData = MetaData;
		return t;
	}
	public void Check()
	{
		for (int i = 0; i < NodeTypes.Length; i++)
		{
			if (!NodeAttributes.ContainsKey(i))
			{
				NodeAttributes.Add(i, new List<int>());
			}
		}
	}
	public TilePrototype(NodeType[] nodes, int[] assignments)
	{
		if (nodes == null)
			NodeTypes = new int[0];
		else
		{
			NodeTypes = new int[nodes.Length];
			for (int i = 0; i < NodeTypes.Length; i++)
			{
				NodeTypes[i] = (int)nodes[i];
			}
		}
		if (assignments == null)
			assignments = new int[0];
		this.Assignments = assignments;
		this.UserEditable = false;
		Check();
	}
	public TilePrototype() : this(null, null)
	{

	}
}
