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
    // Ranking in current session
    //List<Game.GameAgent> PlayersRanking = new List<Game.GameAgent>(); // gdzie to zapisać, żeby był do tego dostęp a zawartość się nie kasowała?
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
            var winnerAgent = game.GetAgent(playersByScore.First());
            _winnerLabel.Text = $"{winnerAgent.Name} HAS WON!";
            _winnerLabel.AddColorOverride("font_color", winnerAgent.CurrentTheme.PrimaryColor);

            // Ranking in current session
            //var winnerType = winnerAgent.GetType();
            var winnerType = winnerAgent.Type;
            GD.Print(winnerType); //LOCAL
            
            if (winnerType.Equals(Game.PlayerType.LOCAL))
            {
                var playerWithTheSameName = Ranking.PlayersRanking.FirstOrDefault(n=> n.Name.Equals(winnerAgent.Name));
                //if(PlayersRanking.Any(n=> n.Name.Equals(winnerAgent.Name)))
                if (playerWithTheSameName != null)
                // jeśli imię występuje, to porownaj wynik, jeśli nowy wynik większy, to podmień
                {
                    if (winnerAgent.Player.EndScore > playerWithTheSameName.Player.EndScore)
                    {
                        int index = Ranking.PlayersRanking.FindIndex(n=> n.Name.Equals(winnerAgent.Name));

                        if (index != -1)
                        {
                            Ranking.PlayersRanking[index] = winnerAgent;
                        }
                    }
                }
                else
                {
                    // jeśli nie występuje - dodaj agenta i posortuj listę
                    Ranking.PlayersRanking.Add(winnerAgent);
                    // objListOrder.Sort((x, y) => x.OrderDate.CompareTo(y.OrderDate));
                    Ranking.PlayersRanking.Sort((x, y) => x.Player.EndScore.CompareTo(y.Player.EndScore)); //to nie sortuje
                }
                Ranking.PlayersRanking.ForEach(p => GD.Print(p.Name + " " + p.Player.EndScore));
                // TODO jeśli lista ma więcej niż 10 - usuń nadwyżkę
                // END Ranking in current session
                
            }
            
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
