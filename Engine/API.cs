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

    public State CurrentState{get; protected set;}
    public bool IsGameOver { get => CurrentState == State.GAME_OVER;}
    public uint Turn {get; protected set;}
    public List<Player> Players {get => _players.ToList();}
    public List<Action> History {get => History.ToList();}
    public int GetPlayerScore(Player player)
    {
        return basescore[player];
    }
    public int GetPlayerEndScore(Player player)
    {
        throw new NotImplementedException();
    }
    public int GetPlayerProbableScore(Player player)
    {
        throw new NotImplementedException();
    }
    public void PlaceCurrentTile(Vector2I pos, int rot)
    {
        PlaceTileAction act = new PlaceTileAction(pos, rot);
        ExecuteAction(act);
    }
    public void SkipPlacingPawn()
    {
        ExecuteAction(new SkipPawnAction());
    }
    Tile CurrentTile()
    {
        return tilemanager.CurrentTile();
    }
    Tile UpcomingTile()
    {
        return tilemanager.PeekTile();
    }
    List<Tile> QueuedTiles()
    {
        return tilemanager.TileQueue.ToList();
    }

    public List<(Vector2I pos, int rot)> PossiblePlacements()
    {
        AssertState(State.PLACE_TILE);
        Debug.Assert(tilemanager.CurrentTile() != null);

        return map.TryFindAllFits(tilemanager.CurrentTile());
    }


    public Player PeekNextPlayer()
    {
        int indx = _players.IndexOf(CurrentPlayer);
        Debug.Assert(indx >= 0);
        return _players[(indx+1) % _players.Count];
    }



}