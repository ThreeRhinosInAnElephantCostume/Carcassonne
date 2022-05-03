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
    public class MediumAI : AIPlayer
    {
        public override void MakeMove(GameEngine engine)
        {
            if (engine.CurrentState == State.PLACE_TILE)
            {
                /* MID BOT:
                Stwórz kopię silnika gry i przeprować symulację każdego z dostępnych ruchów.

                Dla wszystkich dostępnych ruchów
                    Znajdź ruch, po zakończeniu którego bot zdobędzie najwyższą liczbę punktów
                */
                var tile_placements = engine.PossibleTilePlacements();

                var selected_tile_placement = tile_placements[AbsMod((int)rng.NextLong(), tile_placements.Count)];
                (bool isattribute, int indx) selected_meeple_placement = (false, -1);

                int maximum_point_value = 0; //Maksymalna liczba punktów osiągnięta we wszystkich symulacjach ruchów

                foreach (var tile_placement in tile_placements)
                {
                    GameEngine engineTileTest = engine.Clone();
                    engineTileTest.PlaceCurrentTile(tile_placement.pos, tile_placement.rot);

                    if (engineTileTest.CurrentState == State.PLACE_PAWN) //Symulacja - kładzenie meepla
                    {
                        List<(bool isattribute, int indx)> meeple_placements = engineTileTest.AllPossibleMeeplePlacements();

                        if (meeple_placements.Count > 0)
                        {
                            foreach (var meeple_placement in meeple_placements)
                            {
                                GameEngine enginePawnTest = engineTileTest.Clone();
                                var current_player = enginePawnTest.CurrentPlayer;
                                enginePawnTest.PlacePawn(meeple_placement.isattribute, meeple_placement.indx);

                                int current_points = current_player.PotentialScore;
                                if (current_points > maximum_point_value)
                                {
                                    maximum_point_value = current_points;
                                    selected_tile_placement = tile_placement;
                                    selected_meeple_placement = meeple_placement;
                                }
                                GD.Print("MID AI obliczył punktację dla " + tile_placement + ". Wynosi ona: " + current_points);
                            }
                        }
                    }
                }
                engine.PlaceCurrentTile(selected_tile_placement.pos, selected_tile_placement.rot);
                if (engine.CurrentState == State.PLACE_PAWN)
                {
                    if (selected_meeple_placement.indx == -1)
                        engine.SkipPlacingPawn();
                    else
                        engine.PlacePawn(selected_meeple_placement.isattribute, selected_meeple_placement.indx);
                }
            }
            else
            {
                Assert(false, $"Unsupported state for this AI ({nameof(MediumAI)})");
            }
        }
        public MediumAI(RNG rng) : base(rng)
        {

        }
    }
}