using Godot;


using System;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime;
using System.Runtime.CompilerServices;

using static System.Math;

using static Utils;

using ExtraMath;

public partial class Engine
{

    public class BaseGameTileset : Tileset
    {
        public override uint NDefaultTiles {get => 72;}
        protected override List<Tile> _GenerateTiles(uint n)
        {
            List<Tile> l = new List<Tile>()
            {
                new Tile()
            };
            return l;
        }
        
    }
    public class PlaceTileAction : Action
    {
        public Vector2I pos;
        public int rot;
        public void Fill(Vector2I pos, int rot)
        {
            this.IsFilled = true;
            this.pos = pos;
            this.rot = rot;
        }
    }
    public static Engine CreateBaseGame()
    {
        Engine eng = new Engine();
        return eng;
    }
}