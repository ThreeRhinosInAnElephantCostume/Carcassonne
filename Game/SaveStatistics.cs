using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Carcassonne;
using ExtraMath;
using Godot;
using static System.Math;
using static Carcassonne.GameEngine;
using static Utils;

public partial class Game
{
    public class GameInfo
    {
        public ulong Seed;
        public DateTime Date;
        public GameMode Mode;
        public int Players;
        public uint Turn;
        public string State;
        public string[] Winners;
        public int WinningPoints;
        public string[] PlayerNames;
        public string[] PlayerDescriptors;
        public PlayerType[] PlayerTypes;
        public string[] PlayerTypeNames;
        public Dictionary<string, long> PlayerIDsByName = new Dictionary<string, long>();
        public Dictionary<long, string> PlayerNamesByID = new Dictionary<long, string>();
        public GameEngine.Statistics Statistics;
        public string GenerateCSV()
        {
            var Columns = new List<(string name, Func<int, string> generator)>()
            {
                ("Turn", t => t.ToString()),

                ("Winners", t =>
                    string.Join(" ", Statistics.TurnsData[t].WinningPlayers.ToList().ConvertAll<string>(it => PlayerNamesByID[it]))),

                ("Placed/Total Tiles", t => $"{Statistics.TurnsData[t].PlacedTiles}/{Statistics.TotalTiles}"),

                ("Total Points", t => Statistics.TurnsData[t].CombinedPoints.Total.ToString()),
                ("Real Points", t => Statistics.TurnsData[t].CombinedPoints.Real.ToString()),
                ("Potential Points", t => Statistics.TurnsData[t].CombinedPoints.Potential.ToString()),

                ("From Cities", t => Statistics.TurnsData[t].CombinedPoints.FromCities.ToString()),
                ("From Roads", t => Statistics.TurnsData[t].CombinedPoints.FromRoads.ToString()),
                ("From Monastries", t => Statistics.TurnsData[t].CombinedPoints.FromMonastries.ToString()),
                ("From Farms", t => Statistics.TurnsData[t].CombinedPoints.FromFarms.ToString()),

                ("Finished Projects", t => Statistics.TurnsData[t].FinishedProjects.ToString()),

                ("Finished Cities", t => Statistics.TurnsData[t].FinishedCities.ToString()),
                ("Finished Roads", t => Statistics.TurnsData[t].FinishedRoads.ToString()),
                ("Finished Monastries", t => Statistics.TurnsData[t].FinishedMonastries.ToString()),
                ("Farms", t => Statistics.TurnsData[t].Farms.ToString()),

                ("Open Projects", t => Statistics.TurnsData[t].OpenProjects.ToString()),

                ("Knights", t => Statistics.TurnsData[t].Knights.ToString()),
                ("Highwaymen", t => Statistics.TurnsData[t].Highwaymen.ToString()),
                ("Monks", t => Statistics.TurnsData[t].Monks.ToString()),
                ("Farmers", t => Statistics.TurnsData[t].Farmers.ToString()),
            };
            List<string> Rows = new List<string>();
            for (int i = 0; i < Statistics.Turns; i++)
            {
                Rows.Add(String.Join(",", Columns.ConvertAll<string>(it => it.generator(i))));
            }
            Rows.Add(String.Join(",", Columns.ConvertAll<string>(it => it.name)));
            Rows.Reverse();
            string ret = String.Join("\n", Rows);
            return ret;
        }
        public GameInfo(Game game, bool IncludeActions)
        {
            Assert(game.Engine.Turn > 0);
            
            game.Agents.ForEach
            (
                it =>
                {
                    PlayerIDsByName.Add(it.Name, it.Player.ID);
                    PlayerNamesByID.Add(it.Player.ID, it.Name);
                }
            );

            Seed = game._seed;
            Date = DateTime.Now;
            Mode = game.Mode;
            Players = game.Agents.Count;
            Turn = game.Engine.Turn;
            State = game.Engine.CurrentState.ToString();
            var winners = game.Engine.GetWinners();
            Winners = winners.ConvertAll<string>(it => PlayerNamesByID[it.ID]).ToArray();
            WinningPoints = winners[0].EndScore;
            PlayerNames = game.Agents.ConvertAll<string>(it => it.Name).ToArray();
            PlayerDescriptors = game.Agents.ConvertAll<string>(it => it.ToString()).ToArray();
            PlayerTypes = game.Agents.ConvertAll<PlayerType>(it => it.Type).ToArray();
            PlayerTypeNames = PlayerTypes.ToList().ConvertAll<string>(it => it.ToString()).ToArray();

            Statistics = game.Engine.GatherStatistics(IncludeActions);
        }
    }
    public GameInfo SaveStatistics(bool exportCSV)
    {
        GameInfo info = new GameInfo(this, true);

        string directory = ConcatPaths(Constants.DataPaths.STATISTICS_PATH, info.Mode.ToString(), info.Date.ToString("s"));
        Utils.EnsurePathExists(directory);
        string path = ConcatPaths(directory, "GameInfo.json");
        Assert(!FileExists(path));

        SerializeToFile(path, info, true);

        if (exportCSV)
        {
            path = ConcatPaths(directory, "GameInfo.csv");
            Assert(!FileExists(path));
            string data = info.GenerateCSV();
            Utils.WriteFile(path, data);
        }

        return info;
    }
}
