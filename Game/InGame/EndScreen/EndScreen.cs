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

[Tool]
public class EndScreen : Control
{
    Label _winnerLabel;
    Label _infoLabel;
    public void Activate(Game game)
    {
        Assert(game.Engine.CurrentState == State.GAME_OVER);
        var winners = game.Engine.GetWinners();
        var playersByScore = game.Engine.Players.OrderBy(it => it.EndScore).Reverse();

        Assert(winners.Count > 0);

        if (winners.Count > 1)
        {
            _winnerLabel.Text = "DRAW!";
        }
        else
        {
            var winner = winners[0];
            var wa = game.GetAgent(winner);
            _winnerLabel.Text = $"{wa.Name} HAS WON!";
            _winnerLabel.AddColorOverride("font_color", wa.CurrentTheme.PrimaryColor);
        }

        List<string> info = new List<string>();

        foreach (var it in playersByScore)
        {
            var agent = game.GetAgent(it);
            info.Add($"{agent.Name} has {it.EndScore} points");
        }

        string infostr = string.Join("\n", info);

        _infoLabel.Text = infostr;

        this.Visible = true;
    }
    public override void _Ready()
    {
        this.Visible = false;
        _winnerLabel = this.GetNodeSafe<Label>("CenterContainer/VBoxContainer/Control2/TextureRect/CenterContainer/WinnerLabel");
        _infoLabel = this.GetNodeSafe<Label>("CenterContainer/VBoxContainer/ScrollContainer/Panel/InfoLabel");
    }
}
