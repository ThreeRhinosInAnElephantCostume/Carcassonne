using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Threading;
using Carcassonne;
using ExtraMath;
using Godot;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

namespace Carcassonne
{
    public partial class GameEngine
    {
        [Serializable]
        public class PlaceTileAction : Action
        {
            public Vector2I pos;
            public int rot;
            public PlaceTileAction(Vector2I pos, int rot)
            {
                this.IsFilled = true;
                this.pos = pos;
                this.rot = rot;
            }
        }
        [ActionExec(typeof(PlaceTileAction))]
        void PlaceCurrentTileExec(Action _act)
        {
            var act = (PlaceTileAction)_act;

            AssertState(State.PLACE_TILE);
            Tile c = tilemanager.CurrentTile();

            _lastTile = c;

            c.Rotate(act.rot);
            if (!map.CanPlaceTile(c, act.pos))
            {
                c.Rotate(-act.rot);
                throw new Exception("INVALID TILE PLACEMENT");
            }
            map.PlaceTile(c, act.pos);

            if (GetPossibleMeeplePlacements(c).Count == 0)
            {
                UpdatePoints();
                if (tilemanager.NextTile() == null)
                    EndGame();

                NextPlayer();
            }
            else
                CurrentState = State.PLACE_PAWN;
        }
        [Serializable]
        public class SkipPawnAction : Action
        {
            public SkipPawnAction()
            {
                this.IsFilled = true;
            }
        }
        [ActionExec(typeof(SkipPawnAction))]
        void SkipPawnExec(Action _act)
        {
            var act = (SkipPawnAction)_act;

            AssertState(State.PLACE_PAWN);


            UpdatePoints();

            CurrentState = State.PLACE_TILE;

            if (tilemanager.NextTile() == null)
            {
                EndGame();
                return;
            }

            NextPlayer();
        }
        [Serializable]
        public class PlacePawnAction : Action
        {
            public int indx;
            public bool isattribute;
            public PlacePawnAction(int indx, bool isattribute)
            {
                Assert(indx >= 0);
                this.IsFilled = true;

                this.indx = indx;
                this.isattribute = isattribute;
            }
            public PlacePawnAction(InternalNode node) : this(node.Index, false) { }
            public PlacePawnAction(Tile.TileAttribute attr) : this(attr.tile.Attributes.IndexOf(attr), true) { }
        }
        [ActionExec(typeof(PlacePawnAction))]
        void PlacePawnExec(Action _act)
        {
            var act = (PlacePawnAction)_act;

            AssertState(State.PLACE_PAWN);
            Assert(GetFreeMeepleCount(CurrentPlayer) > 0);

            var meeple = GetFreeMeeple(CurrentPlayer);

            if (act.isattribute)
            {
                Tile.TileAttribute attr = _lastTile.Attributes[act.indx];

                Assert(GetPossibleMeeplePlacements(_lastTile).Contains(attr));

                meeple.Place(_lastTile, attr);

                if (attr is TileMonasteryAttribute mon)
                    _activeMonasteries.Add(mon);
            }
            else
            {
                InternalNode node = _lastTile.Nodes[act.indx];

                Assert(GetPossibleMeeplePlacements(_lastTile).Contains(node));

                meeple.Place(node);
            }


            UpdatePoints();

            if (tilemanager.NextTile() == null)
            {
                EndGame();
                return;
            }

            NextPlayer();

            CurrentState = State.PLACE_TILE;
        }
        [Serializable]
        protected class StartBaseGameAction : Action
        {
            public ulong seed;
            public int players;
            public ITileset tileset;
            public StartBaseGameAction(ulong seed, ITileset tileset, int players)
            {
                IsFilled = true;
                this.players = players;
                this.seed = seed;
                this.tileset = tileset;
            }
        }
        [ActionExec(typeof(StartBaseGameAction))]
        void StartBaseGameExec(Action _act)
        {
            var act = (StartBaseGameAction)_act;

            rng = new RNG(act.seed);

            Assert(act.players >= MIN_PLAYERS && act.players <= MAX_PLAYERS);

            tilemanager = new TileManager(this);

            tilemanager.AddTiles(act.tileset.GenerateTiles(rng), true);

            tilemanager.NextTile();

            for (int i = 0; i < act.players; i++)
            {
                AddPlayer();
            }
            CurrentPlayer = _players[0];

            CurrentState = State.PLACE_TILE;

            Assert(act.tileset.HasStarter);

            var starter = act.tileset.GenerateStarter(rng);

            Assert(starter != null);

            map = new Map(starter);

            foreach (var it in Players)
            {
                RepeatN(STARTER_MEEPLES, () => it.Pawns.Add(new Meeple(it)));
            }
        }
    }
}
