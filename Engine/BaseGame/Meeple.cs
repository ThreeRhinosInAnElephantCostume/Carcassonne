/*
    *** Meeple.cs ***

    The Meeple and Occupier classes.
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
using System.Threading;
using Carcassonne;
using ExtraMath;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

namespace Carcassonne
{
    public abstract class Occupier : Pawn
    {
        public abstract int Weight { get; }

        public Occupier(Player player)
        {
            Assert(player != null);
            this.Owner = player;
        }
    }
    public class Meeple : Occupier
    {
        public enum Role
        {
            NONE,
            FARMER,
            KNIGHT,
            HIGHWAYMAN,
            MONK
        }
        public Role CurrentRole { get; set; } = Role.NONE;
        public override int Weight => 1;
        public override bool IsInPlay => CurrentRole != Role.NONE;
        public object _place = null;
        static Role MatchRole(NodeType nt)
        {
            Assert(nt != NodeType.ERR);

            switch (nt)
            {
                case NodeType.FARM:
                    return Role.FARMER;
                case NodeType.CITY:
                    return Role.KNIGHT;
                case NodeType.ROAD:
                    return Role.HIGHWAYMAN;
                default:
                    throw new Exception("Invalid/unaccounted for NodeType!");
            }
        }
        static Role MatchRole(TileAttributeType attr)
        {
            Assert(attr == TileAttributeType.MONASTERY);
            return Role.MONK;
        }
        public bool IsConnectedToNode(InternalNode node)
        {
            return _place == node;
        }
        public bool IsConnectedToAttribute(Tile.TileAttribute attr)
        {
            return _place == attr;
        }
        public void Place(InternalNode node)
        {
            Assert(node != null);
            Assert(node.Type != NodeType.ERR);

            node.Graph.Owners.Add(this);
            this.CurrentTile = node.ParentTile;
            this.CurrentRole = MatchRole(node.Type);
            _place = node;
        }
        public void Place(Tile tile, Tile.TileAttribute attr)
        {
            Assert(tile != null);
            Assert(tile.Attributes.Contains(attr));
            Assert(attr != null);
            Assert(attr is TileMonasteryAttribute);

            var mon = (TileMonasteryAttribute)attr;
            mon.Owner = this;
            _place = mon;
            this.CurrentRole = MatchRole(attr.Type);
        }
        public void Remove()
        {
            Assert(IsInPlay);

            if (CurrentRole == Role.MONK)
            {
                var mon = (TileMonasteryAttribute)_place;
                mon.Owner = null;
            }
            else
            {
                var node = (InternalNode)_place;
                node.Graph.Owners.Remove(this);
            }
            CurrentRole = Role.NONE;
        }
        public Meeple(Player player) : base(player)
        {
            Assert(player != null);
            this.Owner = player;
        }
    }
}
