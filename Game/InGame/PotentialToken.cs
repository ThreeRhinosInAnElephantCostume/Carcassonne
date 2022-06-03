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

public class PotentialToken : Spatial
{
    const string PLACE_ACTION = "map_tile_place";
    Area _area;
    CollisionShape _shape;
    CapturableProp _capturableProp;
    [Export(PropertyHint.Enum, "Farmer,Knight,Highwayman,Monk")]
    public int MeepleType = 0;
    static bool _sharedInputLockout = false;
    void AreaInputEvent(Camera camera, InputEvent @event, Vector3 clickpos, Vector3 clicknormal, int shapeindx)
    {
        if (_sharedInputLockout)
            return;
        if (_capturableProp == null || _capturableProp.Data == null)
            return;
        if (InputMap.EventIsAction(@event, PLACE_ACTION) && Input.IsActionJustPressed(PLACE_ACTION))
        {
            var container = _capturableProp.Data.Value.container;
            Meeple.Role role = (container is Tile.TileAttribute attr) ? Meeple.MatchRole(attr.Type) :
                Meeple.MatchRole(((container as InternalNode).Type));
            // !!!
            GD.Print("Meeple placed: " + role.ToString().Capitalize());
            AudioPlayer _gameAudio = GetNode<AudioPlayer>("/root/AudioPlayer");
            string soundNodeName = "Meeple" + role.ToString().Capitalize() + "Sound";
            _gameAudio.PlaySound(soundNodeName);

            _sharedInputLockout = true;

            var agent = (Game.GameLocalAgent)_capturableProp.Data.Value.game.CurrentAgent;

            if (container is Tile.TileAttribute attribute)
                agent.PlaceMeepleOnAttribute(attribute.GetIndex());
            else
                agent.PlaceMeepleOnNode(container.GetIndex());
        }
    }
    public override void _Ready()
    {
        Defer(() =>
        {
            _capturableProp = GetParent<CapturableProp>();
            Assert(_capturableProp != null);
        });
        _sharedInputLockout = false;
        _area = this.GetNodeSafe<Area>("Area");
        _area.OnInputEvent(AreaInputEvent);

        _shape = this.GetNodeSafe<CollisionShape>("Area/CollisionShape");

        this.OnVisibilityChanged(b =>
        {
            _shape.Disabled = !b;
        });
    }
}
