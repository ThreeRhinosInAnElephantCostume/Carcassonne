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

namespace AI
{
    public class RandomAI : AIPlayer
    {
        public override void MakeMove(GameEngine engine)
        {
            if(engine.CurrentState == State.PLACE_TILE)
            {
                var placements = engine.PossibleTilePlacements();
                var selected = placements[AbsMod((int)rng.NextLong(), placements.Count)];
                engine.PlaceCurrentTile(selected.pos, selected.rot);
            }
            else if(engine.CurrentState == State.PLACE_PAWN)
            {
                var attributes = engine.PossibleMeepleAttributePlacements();
                var nodes = engine.PossibleMeepleNodePlacements();
                List<(bool isattribute, int indx)> placements = new List<(bool isattribute, int indx)>();
                attributes.ForEach(it => placements.Add((true, it)));
                nodes.ForEach(it => placements.Add((false, it)));
                if(placements.Count == 0)
                {
                    engine.SkipPlacingPawn();
                }
                else
                {
                    var placement = placements[AbsMod((int)rng.NextLong(), placements.Count)];
                    if(placement.isattribute)
                        engine.PlacePawnOnAttribute(placement.indx);
                    else
                        engine.PlacePawnOnNode(placement.indx);
                }
            }
            else 
            {
                Assert(false, $"Unsupported state for this AI ({nameof(RandomAI)})");
            }
        }
        public RandomAI(RNG rng) : base(rng)
        {

        }
    }
}