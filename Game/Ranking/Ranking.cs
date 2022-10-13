using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using static Utils;

public class Ranking : Control
{
    Button _quitToMainMenuButton;
    Label _winner1, _winner2, _winner3, _winner4, _winner5;
    public static List<Game.GameAgent> PlayersRanking = new List<Game.GameAgent>(); 
    List<Game.GameAgent> sortedPlayersRanking = new List<Game.GameAgent>();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _winner1 = this.GetNodeSafe<Label>("VBoxContainer/Winner1");
        _winner2 = this.GetNodeSafe<Label>("VBoxContainer/Winner2");
        _winner3 = this.GetNodeSafe<Label>("VBoxContainer/Winner3");
        _winner4 = this.GetNodeSafe<Label>("VBoxContainer/Winner4");
        _winner5 = this.GetNodeSafe<Label>("VBoxContainer/Winner5");

        if (PlayersRanking.Any())
            sortedPlayersRanking = PlayersRanking.OrderByDescending(x => x.Player.EndScore).ToList();


        _winner1.Text = "1st: ";
        _winner2.Text = "2nd: ";
        _winner3.Text = "3th: ";
        _winner4.Text = "4th: ";
        _winner5.Text = "5th: ";

        if (sortedPlayersRanking.Count > 0)
            _winner1.Text = _winner1.Text + sortedPlayersRanking[0].Name + " " + sortedPlayersRanking[0].Player.EndScore;
        if (sortedPlayersRanking.Count > 1)
            _winner2.Text = _winner2.Text + sortedPlayersRanking[1].Name + " " + sortedPlayersRanking[1].Player.EndScore;
        if (sortedPlayersRanking.Count > 2)
            _winner3.Text = _winner3.Text + sortedPlayersRanking[2].Name + " " + sortedPlayersRanking[2].Player.EndScore;
        if (sortedPlayersRanking.Count > 3)
            _winner4.Text = _winner4.Text + sortedPlayersRanking[3].Name + " " + sortedPlayersRanking[3].Player.EndScore;
        if (sortedPlayersRanking.Count > 4)
            _winner5.Text = _winner5.Text + sortedPlayersRanking[4].Name + " " + sortedPlayersRanking[4].Player.EndScore;



        _quitToMainMenuButton = this.GetNodeSafe<Button>("VBoxContainer/QuitMainMenuButton");
        _quitToMainMenuButton.OnButtonPressed(() => 
        {
            SetMainScene(Globals.Scenes.MainMenuPacked);
        });
    }
}
