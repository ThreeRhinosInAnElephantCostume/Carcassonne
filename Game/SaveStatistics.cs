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
        string GenerateCSV(List<(string name, Func<int, string> generator)> columns)
        {
            List<string> Rows = new List<string>();
            for (int i = 0; i < Statistics.Turns; i++)
            {
                Rows.Add(String.Join(",", columns.ConvertAll<string>(it => it.generator(i))));
            }
            Rows.Add(String.Join(",", columns.ConvertAll<string>(it => it.name)));
            Rows.Reverse();
            string ret = String.Join("\n", Rows);
            return ret;
        }
        public string GenerateGeneralizedCSV()
        {
            var columns = new List<(string name, Func<int, string> generator)>()
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
                ("From Monasteries", t => Statistics.TurnsData[t].CombinedPoints.FromMonasteries.ToString()),
                ("From Farms", t => Statistics.TurnsData[t].CombinedPoints.FromFarms.ToString()),

                ("Finished Projects", t => Statistics.TurnsData[t].FinishedProjects.ToString()),

                ("Finished Cities", t => Statistics.TurnsData[t].FinishedCities.ToString()),
                ("Finished Roads", t => Statistics.TurnsData[t].FinishedRoads.ToString()),
                ("Finished Monasteries", t => Statistics.TurnsData[t].FinishedMonasteries.ToString()),
                ("Farms", t => Statistics.TurnsData[t].Farms.ToString()),

                ("Open Projects", t => Statistics.TurnsData[t].OpenProjects.ToString()),

                ("Knights", t => Statistics.TurnsData[t].Knights.ToString()),
                ("Highwaymen", t => Statistics.TurnsData[t].Highwaymen.ToString()),
                ("Monks", t => Statistics.TurnsData[t].Monks.ToString()),
                ("Farmers", t => Statistics.TurnsData[t].Farmers.ToString()),
            };
            return GenerateCSV(columns);
        }
        public string GeneratePersonalizedCSV()
        {
            var columns = new List<(string name, Func<int, string> generator)>()
            {
                ("Turn", t => t.ToString()),

                ("Winners", t =>
                    string.Join(" ", Statistics.TurnsData[t].WinningPlayers.ToList().ConvertAll<string>(it => PlayerNamesByID[it]))),

                ("Placed/Total Tiles", t => $"{Statistics.TurnsData[t].PlacedTiles}/{Statistics.TotalTiles}"),

                ("Total Points", t => Statistics.TurnsData[t].CombinedPoints.Total.ToString()),
                ("Real Points", t => Statistics.TurnsData[t].CombinedPoints.Real.ToString()),
                ("Potential Points", t => Statistics.TurnsData[t].CombinedPoints.Potential.ToString()),
            };
            for (int _i = 0; _i < Players; _i++)
            {
                int i = _i; // a copy for the lambdas
                var name = PlayerNames[i];
                columns.AddRange(new List<(string name, Func<int, string> generator)>()
                {
                    (name + "'s Total Points", t=> Statistics.TurnsData[t].PlayerData[i].points.Total.ToString()),
                    (name + "'s Real Points", t=> Statistics.TurnsData[t].PlayerData[i].points.Real.ToString()),
                    (name + "'s Potential Points", t=> Statistics.TurnsData[t].PlayerData[i].points.Potential.ToString()),
                    (name + "'s From Cities", t=> Statistics.TurnsData[t].PlayerData[i].points.FromCities.ToString()),
                    (name + "'s From Roads", t=> Statistics.TurnsData[t].PlayerData[i].points.FromRoads.ToString()),
                    (name + "'s From Monasteries", t=> Statistics.TurnsData[t].PlayerData[i].points.FromMonasteries.ToString()),
                    (name + "'s From Farms", t=> Statistics.TurnsData[t].PlayerData[i].points.FromFarms.ToString()),
                    (name + "'s Meeples Aval/Total",
                        t => $"{Statistics.TurnsData[t].PlayerData[i].AvailableMeeples}/{Statistics.TurnsData[t].PlayerData[i].TotalMeeples}"),
                    (name + "'s Knights", t=> Statistics.TurnsData[t].PlayerData[i].Knights.ToString()),
                    (name + "'s Highwaymen", t=> Statistics.TurnsData[t].PlayerData[i].Highwaymen.ToString()),
                    (name + "'s Monks", t=> Statistics.TurnsData[t].PlayerData[i].Monks.ToString()),
                    (name + "'s Farmers", t=> Statistics.TurnsData[t].PlayerData[i].Farmers.ToString()),
                });
            }
            return GenerateCSV(columns);
        }
        public string GenerateSummaryCSV()
        {
            // in this one, the first argument of the generato refers to the player's index in the playerIDs list
            var columns = new List<(string name, Func<int, object> generator)>()
            {
                ("Name", p => PlayerNames[p]),
                ("ID", p => Statistics.PlayerIDs[p].ToString()),
                ("Winner", p => Statistics.FinalTurn.WinningPlayers.Contains(Statistics.PlayerIDs[p]) ? "+" : "-"),

                ("Total Points", p => Statistics.FinalTurn.PlayerData[p].points.Total),
                ("Real Points", p => Statistics.FinalTurn.PlayerData[p].points.Real),
                ("Potential Points", p => Statistics.FinalTurn.PlayerData[p].points.Potential),

                ("From Cities", p => Statistics.FinalTurn.PlayerData[p].points.FromCities),
                ("From Roads", p => Statistics.FinalTurn.PlayerData[p].points.FromRoads),
                ("From Monasteries", p => Statistics.FinalTurn.PlayerData[p].points.FromMonasteries),
                ("From Farms", p => Statistics.FinalTurn.PlayerData[p].points.FromFarms),

                ("Total Placed Meeples", p => Statistics.FinalTurn.PlayerData[p].TotalPlacedMeeples),
                ("Total Placed Knights", p => Statistics.FinalTurn.PlayerData[p].TotalPlacedKnights),
                ("Total Placed Highwaymen", p => Statistics.FinalTurn.PlayerData[p].TotalPlacedHighwaymen),
                ("Total Placed Monks", p => Statistics.FinalTurn.PlayerData[p].TotalPlacedMonks),
                ("Total Placed Farmers", p => Statistics.FinalTurn.PlayerData[p].TotalPlacedFarmers),

                ("Involved Projects", p => Statistics.FinalTurn.PlayerData[p].InvolvedProjects),
                ("Finished Projects", p => Statistics.FinalTurn.PlayerData[p].FinishedProjects),
                ("Finished Cities", p => Statistics.FinalTurn.PlayerData[p].FinishedCities),
                ("Finished Roads", p => Statistics.FinalTurn.PlayerData[p].FinishedRoads),
                ("Finished Monasteries", p => Statistics.FinalTurn.PlayerData[p].FinishedMonasteries),
                ("Farms", p => Statistics.FinalTurn.PlayerData[p].Farms),
            };
            List<string> rows = new List<string>();
            List<object> sums = new List<object>();
            RepeatN(columns.Count, () => sums.Add(null));
            for (int i = 0; i < Players; i++)
            {
                var data = columns.ConvertAll<object>(it => it.generator(i));
                for (int ii = 0; ii < data.Count; ii++)
                {
                    if (data[ii] is int n)
                    {
                        if (sums[ii] == null)
                            sums[ii] = (int)0;
                        var cv = (int)sums[ii];
                        sums[ii] = cv + n;
                    }
                }
                rows.Add(String.Join(",", data.ConvertAll<string>(it => it.ToString())));
            }
            sums[0] = "TOTAL:";
            rows.Insert(0, String.Join(",", sums.ConvertAll<string>(it => (it == null) ? "#" : it.ToString())));
            rows.Insert(0, String.Join(",", columns.ConvertAll<string>(it => it.name)));
            return String.Join("\n", rows);
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
            string data;

            path = ConcatPaths(directory, "GeneralInfo.csv");
            Assert(!FileExists(path));
            data = info.GenerateGeneralizedCSV();
            Utils.WriteFile(path, data);

            path = ConcatPaths(directory, "PlayerTurnInfo.csv");
            Assert(!FileExists(path));
            data = info.GeneratePersonalizedCSV();
            Utils.WriteFile(path, data);

            path = ConcatPaths(directory, "PlayerEndInfo.csv");
            Assert(!FileExists(path));
            data = info.GenerateSummaryCSV();
            Utils.WriteFile(path, data);
        }

        return info;
    }
}
