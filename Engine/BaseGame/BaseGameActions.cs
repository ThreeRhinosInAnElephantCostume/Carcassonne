/*
    *** BaseGameActions.cs ***

    Base game action and their associated functions.
*/

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
            private PlaceTileAction() { }
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
            Tile c = _tileManager.CurrentTile();

            _lastTile = c;

            c.Rotate(act.rot);
            if (!map.CanPlaceTile(c, act.pos))
            {
                c.Rotate(-act.rot);
                throw new IllegalMoveException("INVALID TILE PLACEMENT");
            }
            map.PlaceTile(c, act.pos);

            if (GetPossibleMeeplePlacements(CurrentPlayer, c).Count == 0)
            {
                UpdatePoints();
                if (!NextTileEnsurePlaceable())
                {
                    EndGame();
                    return;
                }

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

            if (!NextTileEnsurePlaceable())
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
            private PlacePawnAction() { }
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
            AssertRule(GetFreeMeepleCount(CurrentPlayer) > 0,
                 "Attempted to place a Meeple when there are none remaining.");

            var meeple = GetFreeMeeple(CurrentPlayer);

            if (act.isattribute)
            {
                Tile.TileAttribute attr = _lastTile.Attributes[act.indx];

                AssertRule(GetPossibleMeeplePlacements(CurrentPlayer, _lastTile).Contains(attr),
                    "Attempted to place a meeple on a nonexistent attribute.");

                if (attr is TileMonasteryAttribute _mon)
                {
                    AssertRule(_mon.Owner == null,
                        "Attempted to place a meeple on a monastery that's already been claimed.");
                }

                meeple.Place(_lastTile, attr);

                if (attr is TileMonasteryAttribute mon)
                    _activeMonasteries.Add(mon);
            }
            else
            {
                InternalNode node = _lastTile.Nodes[act.indx];

                AssertRule(GetGraphOwners(node.Graph).Count == 0,
                    "Attempted to place a meeple on a node that's already owned.");

                AssertRule(GetPossibleMeeplePlacements(CurrentPlayer, _lastTile).Contains(node),
                    "Attempted to place a meeple on a nonexistent node");

                meeple.Place(node);
            }


            UpdatePoints();

            NextPlayer();

            CurrentState = State.PLACE_TILE;

            if (!NextTileEnsurePlaceable())
            {
                EndGame();
                return;
            }

        }
        [Serializable]
        public class StartBaseGameAction : Action
        {
            public ulong seed;
            public int players;
            public string tileset;
            private StartBaseGameAction() { }
            public StartBaseGameAction(ulong seed, string tileset, int players)
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

            _rng = new RNG(act.seed);

            _tileset = _dataSource.GetTileset(act.tileset);

            Assert(act.players >= MIN_PLAYERS && act.players <= MAX_PLAYERS);

            _tileManager = new TileManager(this);

            _tileManager.AddTiles(_tileset.GenerateTiles(_rng), true);

            CurrentState = State.PLACE_TILE;

            for (int i = 0; i < act.players; i++)
            {
                AddPlayer();
            }
            CurrentPlayer = _players[0];


            Assert(_tileset.HasStarter);

            var starter = _tileset.GenerateStarter(_rng);

            Assert(starter != null);

            map = new Map(starter);

            foreach (var it in Players)
            {
                RepeatN(STARTER_MEEPLES, () => it.Pawns.Add(new Meeple(it)));
            }

            Assert(NextTileEnsurePlaceable(), "Cannot attach the first tile to the starter tile");
        }
    }
}
