using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Parser
{
    class Program
    {
        const bool use_co = true; //Use common opponents method
        const bool use_oncourt = true;
        const bool mine = true;
        const int from_year = 2004; //Initial year of data

        /*  Command line arguments
            use_oncourt = false featuresoutput oddsdirectory predictioncsv datadirectory
            use_oncourt = true featuresoutput oddsdirectory predictioncsv tours_atp players_atp matches_atp
        */
        static void Main(string[] args)
        {
            List<Tournament> t = new List<Tournament>(); //Tournaments
            List<Player> p = new List<Player>(); //Players
            List<Match> m = new List<Match>(); //Matches

            #region parser-github
            if (!use_oncourt)
            {
                string[] paths = Directory.EnumerateFiles(args[3]).ToArray();
                int number = 0;
                foreach (string path in paths)
                {
                    //Check initial year
                    if (int.Parse(path.Substring(path.LastIndexOf("\\") + 1).Substring(12, 4)) < from_year) continue;
                    StreamReader reader = new StreamReader(path); reader.ReadLine();
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        string[] split = line.Split(',');
                        if (ParseLevel(split[4]) == Level.D) continue; //No DAVIS
                                                                       //Tournament
                        int t_year = int.Parse(split[0].Substring(0, 4));
                        int t_id = int.Parse(split[0].Substring(5));
                        Tournament tournament = t.Find(x => x.ID == t_id && x.Year == t_year);
                        if (tournament == null) //Il torneo non è ancora stato inserito
                        {
                            tournament = new Tournament(t_id, t_year);
                            tournament.Surface = ParseSurface(split[2]);
                            tournament.Name = split[1].ToLower().Trim();
                            //if (tournament.Name == "canada masters") tournament.City = "toronto";
                            tournament.DrawSize = int.Parse(split[3]);
                            tournament.Level = ParseLevel(split[4]); if (tournament.Level == Level.D) continue;
                            tournament.StartingDay = DateTime.ParseExact(split[5], "yyyyMMdd", CultureInfo.InvariantCulture);
                            t.Add(tournament);
                        }
                        //Player1 (winner)
                        int p1_id = int.Parse(split[7]);
                        Player player1 = p.Find(x => x.ID == p1_id);
                        if (player1 == null) //Questo giocatore non è ancora stato inserito
                        {
                            player1 = new Player(p1_id);
                            player1.Name = split[10];
                            player1.Hand = split[11].ToCharArray()[0];
                            player1.Height = (split[12] != "") ? int.Parse(split[12]) : -1;
                            player1.Country = split[13];
                            player1.BornDate = tournament.StartingDay - new TimeSpan(int.Parse(split[14].Remove(split[14].IndexOf('.'))) * 360, 0, 0, 0);
                            p.Add(player1);
                        }
                        //Player2 (loser)
                        int p2_id = int.Parse(split[17]);
                        Player player2 = p.Find(x => x.ID == p2_id);
                        if (player2 == null) //Questo giocatore non è ancora stato inserito
                        {
                            player2 = new Player(p2_id);
                            player2.Name = split[20];
                            player2.Hand = split[21].ToCharArray()[0];
                            player2.Height = (split[22] != "") ? int.Parse(split[22]) : -1;
                            player2.Country = split[23];
                            player2.BornDate = (split[24] != "") ? tournament.StartingDay - new TimeSpan(int.Parse(split[24].Remove(split[24].IndexOf('.'))) * 360, 0, 0, 0) : new DateTime(1990, 1, 1);
                            p.Add(player2);
                        }
                        //Match
                        Match match = new Match(number);
                        match.Player1 = player1; match.Player2 = player2; match.MatchWinner = 1;
                        match.Tournament = tournament; match.Surface = tournament.Surface;
                        match.Date = match.Tournament.StartingDay;
                        match.p1_Seed = (split[8] != "") ? int.Parse(split[8]) : -1; match.p2_Seed = (split[18] != "") ? int.Parse(split[18]) : -1;
                        match.p1_Entry = ParseEntry(split[9]); match.p2_Entry = ParseEntry(split[19]);
                        match.p1_Rank = (split[15] != "") ? int.Parse(split[15]) : 0; match.p2_Rank = (split[25] != "") ? int.Parse(split[25]) : 0;
                        match.p1_RankPoints = (split[16] != "") ? int.Parse(split[16]) : 0; match.p2_RankPoints = (split[26] != "") ? int.Parse(split[26]) : 0;
                        match.BestOfSets = int.Parse(split[28]);
                        match.Round = ParseRound(split[29]);
                        //Score
                        if (split[27].Contains("W/O") || split[27].Contains("DEF")) match.WalkOver = true;
                        else if (split[27] == "") continue;
                        else
                        {
                            string[] score = split[27].Trim().Split(' '); //Nel formato: 5-7 7-6(4) 7-6(5)
                            match.Score = new List<Set>();
                            for (int i = 0; i < score.Length; i++)
                            {
                                if (score[i] == "RET")
                                {
                                    match.Retired = true; break;
                                }
                                string[] set_score = score[i].Split('-');
                                int games1 = int.Parse(set_score[0]);
                                int games2 = 0; int tie2 = 0;
                                if (set_score[1].Contains("("))
                                {
                                    games2 = int.Parse(set_score[1].Substring(0, set_score[1].IndexOf('(')));
                                    tie2 = int.Parse(set_score[1].Substring(set_score[1].IndexOf('(') + 1, set_score[1].IndexOf(')') - set_score[1].IndexOf('(') - 1));
                                }
                                else
                                    games2 = int.Parse(set_score[1]);
                                Set set = new Set(games1, games2, (tie2 == 0) ? 0 : -1, tie2);
                                match.Score.Add(set);
                            }
                        }
                        //Statistics
                        if (!match.WalkOver)
                        {
                            Stats stats = new Stats();
                            stats.Duration = (split[30] != "") ? new TimeSpan(0, int.Parse(split[30]), 0) : new TimeSpan(0);
                            if (split[31] != "") //Se ci sono i dati per questo match
                            {
                                stats.p1_Aces = int.Parse(split[31]); stats.p2_Aces = int.Parse(split[40]);
                                stats.p1_DoubleFaults = int.Parse(split[32]); stats.p2_DoubleFaults = int.Parse(split[41]);
                                stats.p1_PointsOnServe = int.Parse(split[33]); stats.p2_PointsOnServe = int.Parse(split[42]);
                                stats.p1_FirstIn = int.Parse(split[34]); stats.p2_FirstIn = int.Parse(split[43]);
                                stats.p1_FirstWon = int.Parse(split[35]); stats.p2_FirstWon = int.Parse(split[44]);
                                stats.p1_SecondWon = int.Parse(split[36]); stats.p2_SecondWon = int.Parse(split[45]);
                                stats.p1_GamesOnServe = int.Parse(split[37]); stats.p2_GamesOnServe = int.Parse(split[46]);
                                stats.p1_BreakpointsSaved = int.Parse(split[38]); stats.p2_BreakpointsSaved = int.Parse(split[47]);
                                stats.p1_BreakpointsFaced = int.Parse(split[39]); stats.p2_BreakpointsFaced = int.Parse(split[48]);
                                match.Statistics = stats;
                            }
                        }
                        m.Add(match); number++; //MATCH IS ADDED ONLY HERE
                    }
                    reader.Close();
                }
            }
            #endregion
            #region parser-oncourt
            if (use_oncourt)
            {
                StreamReader reader;
                //Read tournaments
                reader = new StreamReader(args[3]); reader.ReadLine();
                while(!reader.EndOfStream)
                {
                    string[] split = reader.ReadLine().Split(',');
                    int t_id = int.Parse(split[0]);
                    string[] t_name = split[1].Split('-');
                    int t_surface = int.Parse(split[2]);
                    string t_date = split[3];
                    DateTime t_datetime = DateTime.ParseExact(t_date, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                    int t_type = int.Parse(split[4]);
                    string t_country = split[6];
                    string t_prizemoney = split[7];
                    Tournament tournament = new Tournament(t_id, t_datetime.Year);
                    switch(t_surface)
                    {
                        case 1: tournament.Surface = Surface.Hard; break;
                        case 2: tournament.Surface = Surface.Clay; break;
                        case 3: tournament.Surface = Surface.IndoorHard; break;
                        case 4: tournament.Surface = Surface.Hard; break;
                        case 5: tournament.Surface = Surface.Grass; break;
                        case 6: tournament.Surface = Surface.Hard; break;
                    }
                    switch(t_type)
                    {
                        case 2: tournament.Level = Level.A; break;
                        case 3: tournament.Level = Level.M; break;
                        case 4: tournament.Level = Level.G; break;
                        default: continue;
                    }
                    if (t_name.Length > 1) { tournament.Name = t_name[0].Trim().ToLower(); tournament.City = t_name[1].Trim().ToLower(); }
                    else tournament.City = t_name[0].Trim().ToLower();
                    tournament.StartingDay = t_datetime;
                    tournament.Country = t_country;
                    t.Add(tournament);
                }
                reader.Close();
                //Read players
                reader = new StreamReader(args[4]); reader.ReadLine();
                while(!reader.EndOfStream)
                {
                    string[] split = reader.ReadLine().Split(',');
                    int p_id = int.Parse(split[0]);
                    string p_name = split[1];
                    DateTime p_date = (split[2] != "") ? DateTime.ParseExact(split[2], "MM/dd/yyyy", CultureInfo.InvariantCulture) : DateTime.Now;
                    string p_country = split[3];
                    Player player = new Player(p_id);
                    player.BornDate = p_date;
                    player.Country = p_country;
                    player.Name = p_name;
                    p.Add(player);
                }
                reader.Close();
                //Read matches
                reader = new StreamReader(args[5]); reader.ReadLine(); int k = 0;
                while(!reader.EndOfStream)
                {
                    string[] split = reader.ReadLine().Split(',');
                    //Player player1 = p.Find(x => x.ID == int.Parse(split[0]));
                    //Player player2 = p.Find(x => x.ID == int.Parse(split[1]));
                    //Tournament tournament = t.Find(x => x.ID == int.Parse(split[2]));
                    Player player1 = p.RicercaDicotomica(x => x.ID, int.Parse(split[0]));
                    Player player2 = p.RicercaDicotomica(x => x.ID, int.Parse(split[1]));
                    Tournament tournament = t.RicercaDicotomica(x => x.ID, int.Parse(split[2]));
                    if (tournament == null || tournament.Year < from_year) continue;
                    Stats stats = new Stats();
                    stats.p1_FirstIn = (split[4] != "") ? int.Parse(split[4]) : 0;
                    stats.p1_PointsOnServe = (split[5] != "") ? int.Parse(split[5]) : 0;
                    stats.p1_Aces = (split[6] != "") ? int.Parse(split[6]) : 0;
                    stats.p1_DoubleFaults = (split[7] != "") ? int.Parse(split[7]) : 0;
                    stats.p1_UnforcedErrors = (split[8] != "") ? int.Parse(split[8]) : 0;
                    stats.p1_FirstWon = (split[9] != "") ? int.Parse(split[9]) : 0;
                    stats.p1_SecondWon = (split[11] != "") ? int.Parse(split[11]) : 0;
                    stats.p1_Winners = (split[13] != "") ? (int.Parse(split[13]) - stats.p1_Aces) : 0;
                    stats.p1_BreakpointsFaced = (split[33] != "") ? int.Parse(split[33]) : 0;
                    stats.p1_BreakpointsSaved = (split[32] != "") ? (stats.p1_BreakpointsFaced - int.Parse(split[32])) : 0;
                    stats.p1_NetApproaches = (split[16] != "") ? int.Parse(split[16]) : 0;
                    stats.p1_NetApproachesWon = (split[17] != "") ? int.Parse(split[17]) : 0;
                    stats.p1_MaxServeSpeed = (split[19] != "") ? int.Parse(split[19]) : 0;
                    stats.p1_Avg1stServeSpeed = (split[20] != "") ? int.Parse(split[20]) : 0;
                    stats.p1_Avg2ndServeSpeed = (split[21] != "") ? int.Parse(split[21]) : 0;
                    stats.p2_FirstIn = (split[22] != "") ? int.Parse(split[22]) : 0;
                    stats.p2_PointsOnServe = (split[23] != "") ? int.Parse(split[23]) : 0;
                    stats.p2_Aces = (split[24] != "") ? int.Parse(split[24]) : 0;
                    stats.p2_DoubleFaults = (split[25] != "") ? int.Parse(split[25]) : 0;
                    stats.p2_UnforcedErrors = (split[26] != "") ? int.Parse(split[26]) : 0;
                    stats.p2_FirstWon = (split[27] != "") ? int.Parse(split[27]) : 0;
                    stats.p2_SecondWon = (split[29] != "") ? int.Parse(split[29]) : 0;
                    stats.p2_Winners = (split[31] != "") ? (int.Parse(split[31]) - stats.p2_Aces) : 0;
                    stats.p2_BreakpointsFaced = (split[15] != "") ? int.Parse(split[15]) : 0;
                    stats.p2_BreakpointsSaved = (split[14] != "") ? (stats.p2_BreakpointsFaced - int.Parse(split[14])) : 0;
                    stats.p2_NetApproaches = (split[34] != "") ? int.Parse(split[34]) : 0;
                    stats.p2_NetApproachesWon = (split[35] != "") ? int.Parse(split[35]) : 0;
                    stats.p2_MaxServeSpeed = (split[37] != "") ? int.Parse(split[37]) : 0;
                    stats.p2_Avg1stServeSpeed = (split[38] != "") ? int.Parse(split[38]) : 0;
                    stats.p2_Avg2ndServeSpeed = (split[39] != "") ? int.Parse(split[39]) : 0;
                    stats.Duration = (split[44] != "") ? TimeSpan.ParseExact(split[44], @"hh\:mm\:ss", CultureInfo.InvariantCulture) : new TimeSpan(0);
                    Match match = new Match(k);
                    match.Player1 = player1; match.Player2 = player2; match.Tournament = tournament; match.MatchWinner = 1;
                    match.Statistics = stats;
                    m.Add(match);
                    k++;
                }
                reader.Close(); Console.WriteLine("Finished reading matches at " + DateTime.Now.ToString());
                //Read score
                reader = new StreamReader(args[6]); reader.ReadLine();
                m = m.OrderBy(x => x.Tournament.ID).ToList();
                while(!reader.EndOfStream)
                {
                    int p1_id, p2_id, t_id;
                    string[] split = reader.ReadLine().Split(',');
                    p1_id = int.Parse(split[0]);
                    p2_id = int.Parse(split[1]);
                    t_id = int.Parse(split[2]);
                    DateTime date = (split[5] != "") ? DateTime.ParseExact(split[5], "MM/dd/yyyy", CultureInfo.InvariantCulture) : DateTime.MinValue;
                    if (date != DateTime.MinValue && date.Year < from_year) continue;
                    List<Match> matches_in_tournament = m.RicercaDicotomicaAll(x => x.Tournament.ID, t_id);
                    //List<Match> matches = m.FindAll(x => x.Tournament.ID == t_id && x.Player1.ID == p1_id && x.Player2.ID == p2_id);
                    if (matches_in_tournament == null) continue;
                    List<Match> matches = matches_in_tournament.FindAll(x => x.Player1.ID == p1_id && x.Player2.ID == p2_id);
                    if (matches.Count == 1)
                    {
                        Match match = matches[0];
                        match.Date = date;
                        if (split[4].Contains("w/o") || split[4].Contains("def")) { match.WalkOver = true; continue; }
                        string[] score = split[4].Trim().Split(' '); //Nel formato: 5-7 7-6(4) 7-6(5)
                        match.Score = new List<Set>();
                        for (int i = 0; i < score.Length; i++)
                        {
                            if (score[i] == "ret.")
                            {
                                match.Retired = true; break;
                            }
                            string[] set_score = score[i].Split('-');
                            int games1 = int.Parse(set_score[0]);
                            int games2 = 0; int tie2 = 0;
                            if (set_score[1].Contains("("))
                            {
                                games2 = int.Parse(set_score[1].Substring(0, set_score[1].IndexOf('(')));
                                tie2 = int.Parse(set_score[1].Substring(set_score[1].IndexOf('(') + 1, set_score[1].IndexOf(')') - set_score[1].IndexOf('(') - 1));
                            }
                            else
                                games2 = int.Parse(set_score[1]);
                            Set set = new Set(games1, games2, (tie2 == 0) ? 0 : -1, tie2);
                            match.Score.Add(set);
                        }
                        //Calcuate games on serve
                        foreach (Set set in match.Score)
                        {
                            match.Statistics.p1_GamesOnServe += set.p1_WonGames - ((set.p1_WonGames == 7 && set.p2_Tiebreak != 0) ? 1 : 0);
                            match.Statistics.p2_GamesOnServe += set.p2_WonGames - ((set.p2_WonGames == 7 && set.p2_Tiebreak != 0) ? 1 : 0);
                        }
                        match.Statistics.p1_GamesOnServe -= match.Statistics.p2_BreakpointsFaced - match.Statistics.p2_BreakpointsSaved;
                        match.Statistics.p2_GamesOnServe -= match.Statistics.p1_BreakpointsFaced - match.Statistics.p1_BreakpointsSaved;
                        match.Statistics.p1_GamesOnServe += match.Statistics.p1_BreakpointsFaced - match.Statistics.p1_BreakpointsSaved;
                        match.Statistics.p2_GamesOnServe += match.Statistics.p2_BreakpointsFaced - match.Statistics.p2_BreakpointsSaved;
                    }
                }
                reader.Close(); Console.WriteLine("Finished reading scores at " + DateTime.Now.ToString());
                //Read rank
                reader = new StreamReader(args[7]); reader.ReadLine();
                m = m.OrderBy(x => x.Tournament.StartingDay).ToList();
                while(!reader.EndOfStream)
                {
                    string[] split = reader.ReadLine().Split(',');
                    int p_id, rank, rank_points; DateTime date;
                    date = DateTime.ParseExact(split[0], "MM/dd/yyyy", CultureInfo.InvariantCulture);
                    p_id = int.Parse(split[1]);
                    rank = int.Parse(split[3]);
                    if (date.Year < from_year) continue;
                    rank_points = int.Parse(split[2]);
                    List<Match> matches = m.RicercaDicotomicaAll(x => x.Tournament.StartingDay, date);
                    if (matches == null) continue;
                    List<Match> match = matches.FindAll(x => x.Player1.ID == p_id || x.Player2.ID == p_id);
                    //Match match = m.Find(x => (x.Player1.ID == p_id || x.Player2.ID == p_id) && x.Tournament.StartingDay == date);
                    if (match != null)
                    {
                        foreach (Match mm in match)
                        {
                            if (mm.Player1.ID == p_id) { mm.p1_Rank = rank; mm.p1_RankPoints = rank_points; }
                            else { mm.p2_Rank = rank; mm.p2_RankPoints = rank_points; }
                        }
                    }
                }
                reader.Close(); Console.WriteLine("Finished reading ranks at " + DateTime.Now.ToString());
            }
            #endregion

            #region shuffle
            Random ri = new Random(50);
            for (int i = 0; i < m.Count; i++)
            {
                bool swap = (ri.NextDouble() > 0.5) ? false : true;
                swap = false;
                if (swap)
                    m[i].Swap();
            }
            //m.Shuffle();
            #endregion

            Console.WriteLine("Parsing data complete. Now parsing odds...");
            #region odds_parser
            //ParseOdds(m, args[1]);
            //List<int> numbers = new List<int>();
            //List<double> pred = new List<double>();
            //List<int> result = new List<int>();
            //ReadPredictions(args[2], ref numbers, ref pred, ref result);
            //Backtest(m, numbers, pred, result); Console.Read();
            #endregion
            Console.WriteLine("Parsing complete.");

            #region features
            StreamWriter writer1 = new StreamWriter(args[0]); //writer1.WriteLine("number,fs,w1sp,w2sp,wsp,wrp,tpw,tmw,aces,df,bp,complete,serveadv,ue,wis,na,a1s,a2s,fatigue,retired1,retired2,direct,output");
            string[] diff_feat = { "fs", "w1sp", "w2sp", "wsp", "wrp", "tpw", "tmw", "aces", "df", "bp", "complete", "serveadv", "ue", "wis", "na", "a1s", "a2s" };
            //Find all macthes for every player
            List<List<Match>> m_p = new List<List<Match>>();
            foreach (Player player in p)
            {
                List<Match> m_player = m.FindAll(x => x.Player1 == player || x.Player2 == player);
                m_player = m_player.OrderBy(x => x.Number).ToList();
                m_p.Add(m_player);
            }
            List<List<double>> inputs = new List<List<double>>();
            Parallel.ForEach(m, (match) =>
            {
                int output = (match.HasPlayerWon(match.Player1)) ? 1 : 0;
                List<double> input = new List<double>(); input.Add(match.Number);
                //List<Match> past1 = m.FindAll(x => (x.Player1 == match.Player1 || x.Player2 == match.Player1) && x.Number < match.Number);
                //List<Match> past2 = m.FindAll(x => (x.Player1 == match.Player2 || x.Player2 == match.Player2) && x.Number < match.Number);
                //List<Match> past1 = m_p[p.IndexOf(match.Player1)].FindAll(x => x.Number < match.Number);
                //List<Match> past2 = m_p[p.IndexOf(match.Player2)].FindAll(x => x.Number < match.Number);
                List<Match> past1 = m_p[p.IndexOf(match.Player1)].FindAllLessOrdered(x => x.Number, match.Number);
                List<Match> past2 = m_p[p.IndexOf(match.Player2)].FindAllLessOrdered(x => x.Number, match.Number);
                if (past1.Count > 0 && past2.Count > 0)
                {
                    if (use_co)
                    {
                        List<Player> co = CommonOpponents(past1.Union(past2).ToList(), match.Player1, match.Player2);
                        //List<Player> co = CommonOpponents(past1, past2, match.Player1, match.Player2);
                        foreach (string feature in diff_feat)
                        {
                            double f1 = FeaturizeCO(match, past1, match.Player1, co, feature);
                            double f2 = FeaturizeCO(match, past2, match.Player2, co, feature);
                            double f = f1 - f2;
                            if (!mine) input.Add(f); else { input.Add(f1); input.Add(f2); }
                        }
                    }
                    else if (!use_co)
                    {
                        foreach (string feature in diff_feat)
                        {
                            double f1 = Featurize(match, past1, match.Player1, feature);
                            double f2 = Featurize(match, past2, match.Player2, feature);
                            double f = f1 - f2;
                            if (!mine) input.Add(f); else { input.Add(f1); input.Add(f2); }
                        }
                    }
                    //Fatigue
                    double fatigue1 = Fatigue(match, past1, match.Player1);
                    double fatigue2 = Fatigue(match, past2, match.Player2);
                    double fatigue = fatigue1 - fatigue2;
                    input.Add(fatigue);
                    //Retired
                    bool retired1 = Retired(past1, match.Player1); input.Add((retired1) ? 1 : 0);
                    bool retired2 = Retired(past2, match.Player2); input.Add((retired2) ? 1 : 0);
                    //Direct
                    double direct = Direct(past1, past2, match.Player1, match.Player2);
                    input.Add(direct);

                    //WRITE TO FILE
                    if (input[1] == 0) { } //If record is empty, don't write it
                    else {
                        inputs.Add(input);
                        //for (int i = 0; i < input.Count; i++) { writer1.Write(input[i]); writer1.Write(","); }
                        //writer1.Write(output); writer1.Write("\n");
                    }
                }
            });
            foreach (Match match in m)
            {
                List<double> input = inputs.Find(x => x[0] == match.Number);
                if (input != null)
                {
                    int output = (match.HasPlayerWon(match.Player1)) ? 1 : 0;
                    for (int i = 0; i < input.Count; i++) { writer1.Write(input[i].ToString(CultureInfo.InvariantCulture)); writer1.Write(","); }
                    writer1.Write(output); writer1.Write("\n");
                }
            }
            writer1.Close();
            #endregion

        }

        static Surface ParseSurface(string txt)
        {
            if (txt.ToLower() == "hard") return Surface.Hard;
            else if (txt.ToLower() == "grass") return Surface.Grass;
            else if (txt.ToLower() == "clay") return Surface.Clay;
            else if (txt.ToLower() == "carpet") return Surface.Hard;
            else throw new Exception("Cannot parse this string into a supported Surface");
        }

        static Level ParseLevel(string txt)
        {
            if (txt.ToLower() == "a") return Level.A;
            else if (txt.ToLower() == "m") return Level.M;
            else if (txt.ToLower() == "g") return Level.G;
            else if (txt.ToLower() == "c") return Level.C;
            else if (txt.ToLower() == "d") return Level.D;
            else if (txt.ToLower() == "f") return Level.F;
            else throw new Exception("Cannot parse this string into a supported Level");
        }

        static Entry ParseEntry(string txt)
        {
            if (txt.ToLower() == "wc") return Entry.WC;
            else if (txt.ToLower() == "ll") return Entry.LL;
            else if (txt.ToLower() == "pr") return Entry.PR;
            else if (txt.ToLower() == "q") return Entry.Q;
            else return Entry.NS;
        }

        static Round ParseRound(string txt)
        {
            if (txt.ToLower() == "f") return Round.F;
            else if (txt.ToLower() == "qf") return Round.QF;
            else if (txt.ToLower() == "r128") return Round.R128;
            else if (txt.ToLower() == "r16") return Round.R16;
            else if (txt.ToLower() == "r32") return Round.R32;
            else if (txt.ToLower() == "r64") return Round.R64;
            else if (txt.ToLower() == "sf") return Round.SF;
            else if (txt.ToLower() == "rr") return Round.RR;
            else if (txt.ToLower() == "br") return Round.BR;
            else throw new Exception("Cannot parse this string into a supported Round");
        }

        /// <summary>
        /// Calculates a feature for a given match.
        /// </summary>
        /// <param name="match">The current match.</param>
        /// <param name="matches">Past matches of player p.</param>
        /// <param name="p">The considered player.</param>
        /// <param name="stat">The statistics to consider.</param>
        /// <returns></returns>
        static double Featurize(Match match, List<Match> matches, Player p, string stat)
        {
            double sum = 0; double sum_w = 0;
            foreach(Match m in matches)
            {
                if (!m.WalkOver && m.Statistics != null)
                {
                    int years = (match.Date - m.Date).Days / 365;
                    double w_t = Math.Min(Math.Pow(0.8, years), 0.8); //Time wieghting
                    double w_s = SurfaceWeight(match.Surface, m.Surface); //Surface weighting
                    sum += GetStat(m, p, stat) * w_t * w_s;
                    sum_w += w_t * w_s;
                }
            }
            return (sum_w != 0) ? sum / sum_w : 0;
        }

        /// <summary>
        /// Gets a certain statistics in a given match for a given player
        /// </summary>
        /// <param name="m">The match.</param>
        /// <param name="p">The player</param>
        /// <param name="stat">The statistics we're interested in</param>
        /// <returns></returns>
        static double GetStat(Match m, Player p, string stat)
        {
            switch (stat)
            {
                case "aces":
                    {
                        return (m.Player1 == p) ? m.Statistics.p1_AcesPerGame : m.Statistics.p2_AcesPerGame;
                    }
                case "fs":
                    {
                        return (m.Player1 == p) ? m.Statistics.p1_FirstServeInPercentage : m.Statistics.p2_FirstServeInPercentage;
                    }
                case "w1sp":
                    {
                        return (m.Player1 == p) ? m.Statistics.p1_WinningOnFirstServePercentage : m.Statistics.p2_WinningOnFirstServePercentage;
                    }
                case "w2sp":
                    {
                        return (m.Player1 == p) ? m.Statistics.p1_WinningOnSecondServePercentage : m.Statistics.p2_WinningOnSecondServePercentage;
                    }
                case "wsp":
                    {
                        return (m.Player1 == p) ? m.Statistics.p1_OverallWinningOnServePercentage : m.Statistics.p2_OverallWinningOnServePercentage;
                    }
                case "wrp":
                    {
                        return (m.Player1 == p) ? m.Statistics.p1_ReturnWinningPercentage : m.Statistics.p2_ReturnWinningPercentage;
                    }
                case "tpw":
                    {
                        return (m.Player1 == p) ? m.Statistics.p1_PointsWonPercentagePerGame : m.Statistics.p2_PointsWonPercentagePerGame;
                    }
                case "tmw":
                    {
                        return (m.HasPlayerWon(p) == true) ? 1 : 0;
                    }
                case "df":
                    {
                        return (m.Player1 == p) ? m.Statistics.p1_DoubleFaultsPercentagePerGame : m.Statistics.p2_DoubleFaultsPercentagePerGame;
                    }
                case "bp":
                    {
                        return (m.Player1 == p) ? m.Statistics.p1_BreakPointsWonPercentage : m.Statistics.p2_BreakPointsWonPercentage;
                    }
                case "complete":
                    {
                        return (m.Player1 == p) ? m.Statistics.p1_Completeness : m.Statistics.p2_Completeness;
                    }
                case "serveadv":
                    {
                        return (m.Player1 == p) ? m.Statistics.p1_AdvantageOnServe : m.Statistics.p2_AdvantageOnServe;
                    }
                case "ue":
                    {
                        return (m.Player1 == p) ? m.Statistics.p1_AvgUnforcedErrorsPerGame : m.Statistics.p2_AvgUnforcedErrorsPerGame;
                    }
                case "wis":
                    {
                        return (m.Player1 == p) ? m.Statistics.p1_AvgWinnersPerGame : m.Statistics.p2_AvgWinnersPerGame;
                    }
                case "na":
                    {
                        return (m.Player1 == p) ? m.Statistics.p1_PercentageNetApproachesWon : m.Statistics.p2_PercentageNetApproachesWon;
                    }
                case "a1s":
                    {
                        return (m.Player1 == p) ? m.Statistics.p1_Avg1stServeSpeed : m.Statistics.p2_Avg1stServeSpeed;
                    }
                case "a2s":
                    {
                        return (m.Player1 == p) ? m.Statistics.p1_Avg2ndServeSpeed : m.Statistics.p2_Avg2ndServeSpeed;
                    }

            }
            return 0;
        }

        static double SurfaceWeight(Surface a, Surface b)
        {
            double[,] matrix = new double[4, 4] { { 1, 0.28, 0.35, 0.24 }, { 0.28, 1, 0.31, 0.14 }, { 0.35, 0.31, 1, 0.25 }, { 0.24, 0.14, 0.25, 1 } };
            return matrix[(int)a,(int)b];
        }

        /// <summary>
        /// Finds common opponents between two players.
        /// </summary>
        /// <param name="matches">Past mactches played by players a and b.</param>
        /// <param name="a">Player a.</param>
        /// <param name="b">Player b.</param>
        /// <returns>A list of players that are common opponents of a and b</returns>
        static List<Player> CommonOpponents(List<Match> matches, Player a, Player b)
        {
            List<Player> a_opponents = new List<Player>();
            foreach(Match match in matches)
            {
                if (match.Player1 == a)
                    a_opponents.Add(match.Player2);
                else if (match.Player2 == a)
                    a_opponents.Add(match.Player1);
            }
            List<Player> b_opponents = new List<Player>();
            foreach (Match match in matches)
            {
                if (match.Player1 == b)
                    b_opponents.Add(match.Player2);
                else if (match.Player2 == b)
                    b_opponents.Add(match.Player1);
            }
            return a_opponents.Intersect(b_opponents).ToList();
        }

        static List<Player> CommonOpponents(List<Match> a_matches, List<Match> b_matches, Player a, Player b)
        {
            List<Player> a_opponents = new List<Player>();
            foreach (Match match in a_matches)
            {
                if (match.Player1 == a)
                    a_opponents.Add(match.Player2);
                else if (match.Player2 == a)
                    a_opponents.Add(match.Player1);
            }
            List<Player> b_opponents = new List<Player>();
            foreach (Match match in b_matches)
            {
                if (match.Player1 == b)
                    b_opponents.Add(match.Player2);
                else if (match.Player2 == b)
                    b_opponents.Add(match.Player1);
            }
            return a_opponents.Intersect(b_opponents).ToList();
        }


        /// <summary>
        /// Calculates a feature for a given match, using the Common Opponent method.
        /// </summary>
        /// <param name="match">The current match.</param>
        /// <param name="matches">Past matches of player p.</param>
        /// <param name="p">The considered player.</param>
        /// <param name="co">List of common opponents.</param>
        /// <param name="stat">The statistics to consider.</param>
        /// <returns></returns>
        static double FeaturizeCO(Match match, List<Match> matches, Player p, List<Player> co, string stat)
        {
            double sum = 0;
            foreach(Player o in co)
            {
                List<Match> pVSo = matches.FindAll(x => x.Player1 == o || x.Player2 == o);
                sum += Featurize(match, pVSo, p, stat);
            }
            sum = (co.Count != 0) ? sum / co.Count : 0;
            return sum;
        }

        /// <summary>
        /// Calculates the fatigue feature for a given match for a player.
        /// </summary>
        /// <param name="match">The match we are going to use the fatigue for</param>
        /// <param name="matches">Past matches of the player</param>
        /// <param name="p">The player we're interested in</param>
        /// <returns>A real-valued number that estimates player's fatigue before playing the match</returns>
        static double Fatigue(Match match, List<Match> matches, Player p)
        {
            double fatigue = 0;
            List<Match> matches_in_current_tournament = matches.FindAll(x => x.Tournament == match.Tournament);
            for(int i = 0; i < matches_in_current_tournament.Count; i++)
            {
                if(!matches_in_current_tournament[i].WalkOver && matches_in_current_tournament[i].Statistics != null)
                    fatigue += Math.Pow(0.75, i) * (matches_in_current_tournament[i].Statistics.p1_GamesOnServe + matches_in_current_tournament[i].Statistics.p2_GamesOnServe);
            }
            return fatigue;
        }

        /// <summary>
        /// Calculates the retired feature at a certain point in time.
        /// </summary>
        /// <param name="matches">Past matches of the player.</param>
        /// <param name="p">The player we're interested in</param>
        /// <returns></returns>
        static bool Retired(List<Match> matches, Player p)
        {
            Match last_match = matches[matches.Count - 1];
            if ((last_match.Retired || last_match.WalkOver) && (last_match.Player1 == p && last_match.MatchWinner == 2 ||
                last_match.Player2 == p && last_match.MatchWinner == 1)) return true;
            else return false;
        }

        /// <summary>
        /// Calculates the direct feature.
        /// </summary>
        /// <param name="a_macthes">Past matches of player a.</param>
        /// <param name="b_macthes">Past matches of player b.</param>
        /// <param name="a">Player a.</param>
        /// <param name="b">Player b.</param>
        /// <returns>A real-valued number that estimates the importance of head to head matches.</returns>
        static double Direct(List<Match> a_matches, List<Match> b_matches, Player a, Player b)
        {
            List<Match> h2h = a_matches.Intersect(b_matches).ToList();
            if (h2h.Count == 0) return 0; //No head to heads
            else
            {
                int a_won = 0;
                foreach (Match match in h2h)
                {
                    if (match.Player1 == a && match.MatchWinner == 1 || match.Player2 == a && match.MatchWinner == 2)
                        a_won++;
                }
                return (double)a_won / h2h.Count - (double)(h2h.Count - a_won) / h2h.Count;
            }
        }

        /// <summary>
        /// Parse odds from files downloaded from tennis-data.co.uk onto a list of matches.
        /// </summary>
        /// <param name="m">List of matches to found odds for.</param>
        /// <param name="directory">Directory where the downloaded files are stored.</param>
        static void ParseOdds(List<Match> m, string directory)
        {
            string[] paths = Directory.EnumerateFiles(directory).ToArray();
            int k = 0; int n = 0;
                Parallel.ForEach(paths, (path) =>
                {
                //Each file corresponds to one year
                StreamReader reader = new StreamReader(path); string first = reader.ReadLine();
                    bool points = first.Contains("WPts");
                    while (!reader.EndOfStream)
                    {
                    //Parse line
                    string line = reader.ReadLine();
                        string[] split = line.Split(',');
                        string t_location = split[1].Trim().ToLower();
                        if (t_location == "st. polten") t_location = "st. poelten";
                        if (t_location == "ho chi min city") t_location = "ho chi minh city";
                        if (t_location == "portschach") t_location = "poertschach";
                        if (t_location == "oeiras") t_location = "estoril";
                        string t_name = split[2].ToLower().Trim();
                        if (t_name == "french open") t_name = "roland garros";
                        if (t_name == "rogers cup") t_name = "canada masters";
                        if (t_name == "toronto tms") t_name = "canada masters";
                        if (t_name == "masters cup" && t_location == "london") t_name = "tour finals";
                        DateTime t_date;
                        try { t_date = DateTime.ParseExact(split[3], "MM/dd/yy", CultureInfo.InvariantCulture); }
                        catch { t_date = DateTime.ParseExact(split[3], "MM/dd/yyyy", CultureInfo.InvariantCulture); }
                        string winner_name = split[9];
                        string loser_name = split[10];
                        int winner_rank = (split[11] != "N/A") ? int.Parse(split[11]) : -1;
                        int winner_rank_points = (points && split[13] != "" && split[13] != "N/A") ? int.Parse(split[13]) : -1;
                        int loser_rank = (split[12] != "N/A") ? int.Parse(split[12]) : -1;
                        int loser_rank_points = (points && split[14] != "" && split[14] != "N/A") ? int.Parse(split[14]) : -1;
                        double winner_odd = 0;
                        double loser_odd = 0;
                        for (int i = (points) ? 28 : 26; i < split.Length; i += 2)
                        {
                            if (split[i] != "")
                            {
                                winner_odd = double.Parse(split[i]);
                                loser_odd = double.Parse(split[i + 1]);
                                break;
                            }
                        }

                    //Find all matches of this tournament
                    List<Match> t_m = m.FindAll(x => x.Tournament.Year == t_date.Year && (x.Tournament.Name == t_name || x.Tournament.Name.Replace("masters", "").Replace("'", "").Trim() == t_location.Replace("'", "") || x.Tournament.City == t_location));
                        if (t_m.Count == 0)
                        {
                        //Console.WriteLine("0 matches of this tournament found");
                    }
                        else
                        {
                            List<Match> target = t_m.FindAll(x => (x.WinnerRank() == winner_rank && x.LoserRank() == loser_rank) || (x.WinnerRankPoints() == winner_rank_points && x.LoserRankPoints() == loser_rank_points));
                            if (target.Count == 0)
                            {
                            //Try with names
                            target = t_m.FindAll(x => CompareSurname(x.WinnerSurname(), winner_name) && CompareSurname(x.LoserSurname(), loser_name));
                                if (target.Count == 0)  //Try with swapped winner and loser
                            {
                                    target = t_m.FindAll(x => CompareSurname(x.WinnerSurname(), loser_name) && CompareSurname(x.LoserSurname(), winner_name));
                                    double temp = winner_odd; winner_odd = loser_odd; loser_odd = temp;
                                }
                                if (target.Count == 0)
                                { //Console.WriteLine("Found 0 matches in same tournament");
                                n++;
                                }
                            }
                            if (target.Count > 2)
                            {
                            //Console.WriteLine("Found more than 2 match in same tournament"); n++;
                        }
                            else if (target.Count == 1 || target.Count == 2)
                            {
                                Match found = (target.Count == 1 || target[0].p1_Odd == 0) ? target[0] : target[1];
                                found.p1_Odd = (found.MatchWinner == 1) ? winner_odd : loser_odd; //Set odds for player 1
                            found.p2_Odd = (found.MatchWinner == 2) ? winner_odd : loser_odd; //Set odds for player 2
                            found.Tournament.City = t_location;
                                found.Date = t_date; k++;
                            }
                        }
                    }
                    reader.Close();
                });
            Console.WriteLine(k);
            Console.WriteLine(n);
        }

        static bool CompareSurname(string a, string b)
        {
            a = a.Trim().ToLower(); b = b.Trim().ToLower();
            if (a == b) return true;
            else
            {
                string[] a_split = a.Split(new char[] { ' ', '-' });
                string[] b_split = b.Split(new char[] { ' ', '-' });
                if (a_split.Intersect(b_split).Count() > 0) return true;
                else return false;
            }
        }

        static void Backtest(List<Match> matches, List<int> numbers, List<double> pred, List<int> result)
        {
            double money = 0; int k = 0;
            double amount = 1;
            matches = matches.OrderBy(x => x.Number).ToList();
            for(int i = 0; i < numbers.Count; i++)
            {
                Match match = matches.RicercaDicotomica(x => x.Number, numbers[i]);
                double prediction = pred[i];
                if (match.p1_Odd > 1)
                {
                    if (prediction > 0.5 && prediction > 1 / match.p1_Odd && match.MatchWinner == 1) money += (amount * match.p1_Odd - amount);
                    else if (prediction < 0.5 && (1 - prediction) > 1 / match.p2_Odd && match.MatchWinner == 2) money += (amount * match.p2_Odd - amount);
                    else if (prediction > 0.5 && prediction > 1 / match.p1_Odd && match.MatchWinner == 2) money -= amount;
                    else if (prediction < 0.5 && (1 - prediction) > 1 / match.p2_Odd && match.MatchWinner == 1) money -= amount;
                    k++;
                }
            }
            
            
            //Parallel.ForEach(matches, (m) =>
            //{
            //    double prediction = 0;
            //    prediction = pred.RicercaDicotomicaDic(m.Number);
            //    if (prediction != 0)
            //    {
            //        //if (prediction > 0.5 && m.MatchWinner == 1 && m.p1_Odd < m.p2_Odd) money += (amount * m.p1_Odd - amount);
            //        //else if ((1 - prediction) > 0.5 && m.MatchWinner == 2 && m.p2_Odd < m.p1_Odd) money += (amount * m.p2_Odd - amount);
            //        // if (prediction > 0.5 && m.MatchWinner == 2) money -= amount;
            //        //else if ((1 - prediction) > 0.5 && m.MatchWinner == 1) money -= amount;
            //        if (prediction > 0.5 && m.MatchWinner == 1) money++;
            //        else if (prediction < 0.5 && m.MatchWinner == 2) money++;
            //        k++;
            //    }
            //});

            Console.WriteLine("Alla fine hai " + money + " con " + k);
        }

        static void ReadPredictions(string path, ref List<int> numbers, ref List<double> pred, ref List<int> result)
        {
            StreamReader reader = new StreamReader(path);
            while(!reader.EndOfStream)
            {
                string[] split = reader.ReadLine().Split(',');
                numbers.Add(int.Parse(split[0]));
                pred.Add(double.Parse(split[1]));
                result.Add(int.Parse(split[2]));
            }
        }

    }
}
