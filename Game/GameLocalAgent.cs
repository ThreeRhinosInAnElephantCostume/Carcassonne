// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
    public class GameLocalAgent : GameAgent
    {
        public bool IsMyMove => _game.CurrentAgent == this;
        public override void OnTurn(GameEngine engine)
        {

        }
        public void PlaceTile(Vector2I pos, int rot)
        {
            _game.Engine.PlaceCurrentTile(pos, rot);
            ExecuteImplied();
        }
        public void PlaceMeepleOnAttribute(int indx)
        {
            _game.Engine.PlacePawnOnAttribute(indx);
            ExecuteImplied();
        }
        public void PlaceMeepleOnNode(int indx)
        {
            _game.Engine.PlacePawnOnNode(indx);
            ExecuteImplied();
        }
        public GameLocalAgent(Game game, string name, GameEngine.Player player) : base(game, name, PlayerType.LOCAL, player)
        {

        }
    }
}
