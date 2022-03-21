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

public partial class Game
{
    public class GameAIAgent : GameAgent
    {
        public bool IsMyMove => _game.CurrentAgent == this;

        // wydaje mi się, że raczej trzeba pomyśleć, jakie zmieny dać w 'TileMap3D' - tam zajmujemy się szukaniem 'PossiblePlacement' (ina)
        public override void NewTurn()
        {
            if (_game.Engine.CurrentState == GameEngine.State.PLACE_TILE)
            {
                int PossibleTilePlacements = _game.Engine.PossibleTilePlacements().Count();
                if (PossibleTilePlacements > 0)
                {
                    this.PlaceTile(_game.Engine.PossibleTilePlacements().ElementAt(0).pos, _game.Engine.PossibleTilePlacements().ElementAt(0).rot);

                }
            }
            else
                ExecuteImplied();
        }
        public void PlaceTile(Vector2I pos, int rot)
        {
            GD.Print("AI place tile");
            _game.Engine.PlaceCurrentTile(pos, rot);
            if (_game.Engine.CurrentState == GameEngine.State.PLACE_PAWN)
                _game.Engine.SkipPlacingPawn(); // TO DO - ZNALEŹĆ ODPOWIEDNIE MIEJSCE NA UMIESZCZENIE MEEPLA - MS
            ExecuteImplied();
        }
        public void PlaceMeepleOnAttribute(int indx)
        {
            GD.Print("AI place meeple on attribute");
            _game.Engine.PlacePawnOnAttribute(0);
            ExecuteImplied();
        }
        public void PlaceMeepleOnNode(int indx)
        {
            GD.Print("AI place meeple on node");
            _game.Engine.PlacePawnOnNode(0);
            ExecuteImplied();
        }
        public GameAIAgent(Game game, string name, GameEngine.Player player) : base(game, name, PlayerType.AI, player)
        {

        }
    }
}
