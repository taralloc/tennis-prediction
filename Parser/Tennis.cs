using System;
using System.Collections.Generic;
using System.Linq;

namespace Parser
{
    public enum Surface { Hard, Clay, Grass, IndoorHard };
    /// <summary>
    /// Round of the match in the tournament (RR = Round-Robin), BR?
    /// </summary>
    public enum Round { R128, R64, R32, R16, QF, SF, F, RR, BR};
    /// <summary>
    /// Level of the turnament: challenger, ATP, masters, grand slam, davis, finals
    /// </summary>
    public enum Level { C, A, M, G, D, F};
    /// <summary>
    /// Entry of the player in the tournament: protected ranking, wild card, qualified, lucky loser, not specified
    /// </summary>
    public enum Entry { PR, WC, Q, LL, NS };

    /// <summary>
    /// Stores information about a match.
    /// </summary>
    public class Match
    {
        /// <summary>
        /// The tournament in which this match was played. Same tournaments played in different years are considered
        /// different tournaments.
        /// </summary>
        public Tournament Tournament { get; set; }
        /// <summary>
        /// Starting date and time of the match.
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Surface of the court.
        /// </summary>
        public Surface Surface { get; set; }

        public Player Player1 { get; set; }
        public Player Player2 { get; set; }

        /// <summary>
        /// The player who won the match: 1 for Player1, 2 for Player2
        /// </summary>
        public int MatchWinner { get; set; }
        /// <summary>
        /// True if the losing player retired.
        /// </summary>
        public bool Retired { get; set; }
        /// <summary>
        /// True if the losing player walked over before the match started.
        /// </summary>
        public bool WalkOver { get; set; }
        /// <summary>
        /// A List of Set that contains the score of the match.
        /// </summary>
        public List<Set> Score { get; set; }
        /// <summary>
        /// Maximum number of sets for this game in this tournament.
        /// </summary>
        public int BestOfSets { get; set; }

        /// <summary>
        /// Round of this match in the tournament.
        /// </summary>
        public Round Round { get; set; }

        /// <summary>
        /// ATP rank of Player1.
        /// </summary>
        public int p1_Rank { get; set; }
        /// <summary>
        /// ATP rank of Player2.
        /// </summary>
        public int p2_Rank { get; set; }
        /// <summary>
        /// ATP rank points of Player1.
        /// </summary>
        public int p1_RankPoints { get; set; }
        /// <summary>
        /// ATP rank points of Player2.
        /// </summary>
        public int p2_RankPoints { get; set; }

        /// <summary>
        /// Seed of Player1 for this tournament.
        /// </summary>
        public int p1_Seed { get; set; }
        /// <summary>
        /// Seed of Player2 for this tournament.
        /// </summary>
        public int p2_Seed { get; set; }
        /// <summary>
        /// Entry round of Player1 for this tournament.
        /// </summary>
        public Entry p1_Entry { get; set; }
        /// <summary>
        /// Entry round of Player2 for this tournament.
        /// </summary>
        public Entry p2_Entry { get; set; }

        /// <summary>
        /// Stats about the match.
        /// </summary>
        public Stats Statistics { get; set; }

        /// <summary>
        /// Unique number key for this match. Numbers should be monotonically increasing, from the oldest match to the most recent
        /// </summary>
        public int Number { get; }

        /// <summary>
        /// Pre-match odds for Player1.
        /// </summary>
        public double p1_Odd { get; set; }
        /// <summary>
        /// Pre-match odds for Player2.
        /// </summary>
        public double p2_Odd { get; set; }

        public Match(int number) { Number = number; }


        /// <summary>
        /// Determines whether a given player has won this match.
        /// </summary>
        /// <param name="p">The player we're interested in</param>
        /// <returns>True if player won</returns>
        public bool HasPlayerWon(Player p)
        {
            if (Player1 == p && MatchWinner == 1 || Player2 == p && MatchWinner == 2)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Swap Player1 and Player 2, including the score and statistics.
        /// </summary>
        public void Swap()
        {
            Player temp1 = Player1;
            Player1 = Player2;
            Player2 = temp1;
            MatchWinner = (MatchWinner == 1) ? 2 : 1;
            int temp2 = p1_Rank;
            p1_Rank = p2_Rank;
            p2_Rank = temp2;
            temp2 = p1_RankPoints;
            p1_RankPoints = p2_RankPoints;
            p2_RankPoints = temp2;
            temp2 = p1_Seed;
            p1_Seed = p2_Seed;
            p2_Seed = temp2;
            Entry temp3 = p1_Entry;
            p1_Entry = p2_Entry;
            p2_Entry = temp3;
            double temp4 = p1_Odd;
            p1_Odd = p2_Odd;
            p2_Odd = temp4;
            if(Score != null) for (int i = 0; i < Score.Count; i++) Score[i].Swap();
            if(Statistics != null) Statistics.Swap();
        }

        /// <summary>
        /// Returns winner's ATP rank.
        /// </summary>
        public int WinnerRank()
        {
            if (MatchWinner == 1) return p1_Rank;
            else return p2_Rank;
        }

        /// <summary>
        /// Returns loser's ATP rank.
        /// </summary>
        public int LoserRank()
        {
            if (MatchWinner == 1) return p2_Rank;
            else return p1_Rank;
        }

        /// <summary>
        /// Returns winner's ATP rank points.
        /// </summary>
        public int WinnerRankPoints()
        {
            if (MatchWinner == 1) return p1_RankPoints;
            else return p2_RankPoints;
        }

        /// <summary>
        /// Returns loser's ATP rank points.
        /// </summary>
        public int LoserRankPoints()
        {
            if (MatchWinner == 1) return p2_RankPoints;
            else return p1_RankPoints;
        }

        public string WinnerSurname()
        {
            return (MatchWinner == 1) ? Player1.Name : Player2.Name;
            if(MatchWinner == 1)
            {
                string[] split = Player1.Name.Split(' ');
                if (split.Length > 0) return split[1]; else return Player1.Name;
            }
            else
            {
                string[] split = Player2.Name.Split(' ');
                if (split.Length > 0) return split[1]; else return Player2.Name;
            }
        }

        public string LoserSurname()
        {
            return (MatchWinner == 2) ? Player1.Name : Player2.Name;
            if (MatchWinner == 1)
            {
                string[] split = Player2.Name.Split(' ');
                if (split.Length > 0) return split[1]; else return Player2.Name;
            }
            else
            {
                string[] split = Player1.Name.Split(' ');
                if (split.Length > 0) return split[1]; else return Player1.Name;
            }
        }


    }

    /// <summary>
    /// Stores information about a tennis set in a match between Player1 and Player2.
    /// </summary>
    public class Set
    {
        /// <summary>
        /// Number of won games by Player1.
        /// </summary>
        public int p1_WonGames { get; set; }
        /// <summary>
        /// Number of won games by Player2.
        /// </summary>
        public int p2_WonGames { get; set; }
        /// <summary>
        /// Number of tiebreak points won by Player1.
        /// </summary>
        public int p1_Tiebreak { get; set; }
        /// <summary>
        /// Number of tiebreak points won by Player2.
        /// </summary>
        public int p2_Tiebreak { get; set; }

        /// <summary>
        /// Stats about the set.
        /// </summary>
        public Stats Statistics { get; set; }

        /// <summary>
        /// Initialize a new Set object with empty values.
        /// </summary>
        public Set()
        {
            p1_WonGames = 0; p1_Tiebreak = 0;
            p2_WonGames = 0; p2_Tiebreak = 0;
            Statistics = null;
        }

        /// <summary>
        /// Initialize a new Set object with the score.
        /// </summary>
        /// <param name="Games1">Number of games won by Player1.</param>
        /// <param name="Games2">Number of games won by Player2.</param>
        /// <param name="Tiebreak1">Number of tiebreak points won by Player1.</param>
        /// <param name="Tiebreak2">Number of tiebreak points won by Player2.</param>
        public Set(int Games1, int Games2, int Tiebreak1, int Tiebreak2)
        {
            p1_WonGames = Games1; p1_Tiebreak = Tiebreak1;
            p2_WonGames = Games2; p2_Tiebreak = Tiebreak2;
            Statistics = null;
        }

        /// <summary>
        /// Initialize a new Set object with the score and stats.
        /// </summary>
        /// <param name="Games1">Number of games won by Player1.</param>
        /// <param name="Games2">Number of games won by Player2.</param>
        /// <param name="Tiebreak1">Number of tiebreak points won by Player1.</param>
        /// <param name="Tiebreak2">Number of tiebreak points won by Player2.</param>
        /// <param name="SetStats">Statistics about this set.</param>
        public Set(int Games1, int Games2, int Tiebreak1, int Tiebreak2, Stats SetStats)
        {
            p1_WonGames = Games1; p1_Tiebreak = Tiebreak1;
            p2_WonGames = Games2; p2_Tiebreak = Tiebreak2;
            Statistics = SetStats;
        }

        /// <summary>
        /// Swap Player1 and Player2.
        /// </summary>
        public void Swap()
        {
            int temp1 = p1_WonGames; p1_WonGames = p2_WonGames; p2_WonGames = temp1;
            temp1 = p1_Tiebreak; p1_Tiebreak = p2_Tiebreak; p2_Tiebreak = temp1;
        }
    }

    /// <summary>
    /// Store information about a tennis tournament.
    /// </summary>
    public class Tournament
    {
        /// <summary>
        /// ID of this tournament. Unique to this tournament.
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// Name of the tournament.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// City where the tournament is held.
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 3 letter-string of the country of this tournament.
        /// </summary>
        public string Country { get; set; }
        /// <summary>
        /// Latitude of the location where the tournament is.
        /// </summary>
        public double Latitude { get; set; }
        /// <summary>
        /// Longitude of the location where the tournament is.
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Year of this tournament.
        /// </summary>
        public int Year { get; set; }
        /// <summary>
        /// Day when the tournament starts.
        /// </summary>
        public DateTime StartingDay { get; set; }
        /// <summary>
        /// Day when the tournaments ends.
        /// </summary>
        public DateTime EndingDay { get; set; }

        /// <summary>
        /// Number of players in the tournament 
        /// </summary>
        public int DrawSize { get; set; }
        
        /// <summary>
        /// Level of the tournament according to the ATP classification.
        /// </summary>
        public Level Level { get; set; }

        /// <summary>
        /// The surface of this tournament.
        /// </summary>
        public Surface Surface { get; set; }

        /// <summary>
        /// Prize money for this entire tournament.
        /// </summary>
        public int PrizeMoney { get; set; }

        public Tournament(int id, int year)
        {
            ID = id; Year = year;
        }

    }

    /// <summary>
    /// Store information about a tennis player.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// Player's ID. Unique everywhere.
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// Player's name and surname.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Player hand: R or L.
        /// </summary>
        public char Hand { get; set; }
        /// <summary>
        /// When the player was born.
        /// </summary>
        public DateTime BornDate { get; set; }
        /// <summary>
        /// Player's country.
        /// </summary>
        public string Country { get; set; }
        /// <summary>
        /// Player's height in centimeters.
        /// </summary>
        public int Height { get; set; }

        public Player(int id)
        {
            ID = id;
        }
    }

    /// <summary>
    /// Stats for a set or a match.
    /// </summary>
    public class Stats
    {
        /// <summary>
        /// Duration of the set or match.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Number of aces done by Player1.
        /// </summary>
        public int p1_Aces { get; set; }
        /// <summary>
        /// Number of aces done by Player2.
        /// </summary>
        public int p2_Aces { get; set; }
        /// <summary>
        /// Number of double faults done by Player1.
        /// </summary>
        public int p1_DoubleFaults { get; set; }
        /// <summary>
        /// Number of double faults done by Player2.
        /// </summary>
        public int p2_DoubleFaults { get; set; }
        /// <summary>
        /// Number of points played on serve by Player1.
        /// </summary>
        public int p1_PointsOnServe { get; set; }
        /// <summary>
        /// Number of points played on serve by Player2.
        /// </summary>
        public int p2_PointsOnServe { get; set; }
        /// <summary>
        /// Number of first serve in for Player1.
        /// </summary>
        public int p1_FirstIn { get; set; }
        /// <summary>
        /// Number of first serve in for Player2.
        /// </summary>
        public int p2_FirstIn { get; set; }
        /// <summary>
        /// Number of points won by Player1 when his/her first serve was in.
        /// </summary>
        public int p1_FirstWon { get; set; }
        /// <summary>
        /// Number of points won by Player2 when his/her first serve was in.
        /// </summary>
        public int p2_FirstWon { get; set; }
        /// <summary>
        /// Number of points won by Player1 when his/her first serve was out, but the second serve was in.
        /// </summary>
        public int p1_SecondWon { get; set; }
        /// <summary>
        /// Number of points won by Player2 when his/her first serve was out, but the second serve was in.
        /// </summary>
        public int p2_SecondWon { get; set; }
        /// <summary>
        /// Number of games played on serve by Player1.
        /// </summary>
        public int p1_GamesOnServe { get; set; }
        /// <summary>
        /// Number of games played on serve by Player2.
        /// </summary>
        public int p2_GamesOnServe { get; set; }
        /// <summary>
        /// Number of breakpoints saved by Player1.
        /// </summary>
        public int p1_BreakpointsSaved { get; set; }
        /// <summary>
        /// Number of breakpoints saved by Player2.
        /// </summary>
        public int p2_BreakpointsSaved { get; set; }
        /// <summary>
        /// Number of breakpoints faced by Player1.
        /// </summary>
        public int p1_BreakpointsFaced { get; set; }
        /// <summary>
        /// Number of breakpoints faced by Player2.
        /// </summary>
        public int p2_BreakpointsFaced { get; set; }
        public int p1_MaxServeSpeed { get; set; }
        public int p2_MaxServeSpeed { get; set; }
        public int p1_Avg1stServeSpeed { get; set; }
        public int p2_Avg1stServeSpeed { get; set; }
        public int p1_Avg2ndServeSpeed { get; set; }
        public int p2_Avg2ndServeSpeed { get; set; }
        public int p1_Winners { get; set; }
        public int p2_Winners { get; set; }
        public int p1_UnforcedErrors { get; set; }
        public int p2_UnforcedErrors { get; set; }
        public int p1_NetApproaches { get; set; }
        public int p2_NetApproaches { get; set; }
        public int p1_NetApproachesWon { get; set; }
        public int p2_NetApproachesWon { get; set; }


        //Additional
        public double p1_AcesPerGame
        {
            get
            {
                if (p1_GamesOnServe == 0) return 0;
                else return (double)p1_Aces / p1_GamesOnServe;
            }
        }
        public double p2_AcesPerGame
        {
            get
            {
                if (p2_GamesOnServe == 0) return 0;
                else return (double)p2_Aces / p2_GamesOnServe;
            }
        }

        public double p1_FirstServeInPercentage
        {
            get
            {
                if (p1_PointsOnServe == 0) return 0;
                else return (double)p1_FirstIn / p1_PointsOnServe;
            }
        }
        public double p2_FirstServeInPercentage
        {
            get
            {
                if (p2_PointsOnServe == 0) return 0;
                else return (double)p2_FirstIn / p2_PointsOnServe;
            }
        }

        public double p1_WinningOnFirstServePercentage
        {
            get
            {
                if (p1_FirstIn == 0) return 0;
                else return (double)p1_FirstWon / p1_FirstIn;
            }
        }
        public double p2_WinningOnFirstServePercentage
        {
            get
            {
                if (p2_FirstIn == 0) return 0;
                else return (double)p2_FirstWon / p2_FirstIn;
            }
        }

        public double p1_WinningOnSecondServePercentage
        {
            get
            {
                if (p1_PointsOnServe - p1_FirstIn == 0) return 0;
                else return (double)p1_SecondWon / (p1_PointsOnServe - p1_FirstIn);
            }
        }
        public double p2_WinningOnSecondServePercentage
        {
            get
            {
                if (p2_PointsOnServe - p2_FirstIn == 0) return 0;
                else return (double)p2_SecondWon / (p2_PointsOnServe - p2_FirstIn);
            }
        }

        public double p1_OverallWinningOnServePercentage
        {
            get
            {
                return p1_WinningOnFirstServePercentage * p1_FirstServeInPercentage + p1_WinningOnSecondServePercentage * (1 - p1_FirstServeInPercentage);
            }
        }
        public double p2_OverallWinningOnServePercentage
        {
            get
            {
                return p2_WinningOnFirstServePercentage * p2_FirstServeInPercentage + p2_WinningOnSecondServePercentage * (1 - p2_FirstServeInPercentage);
            }
        }

        public double p1_ReturnWinningPercentage
        {
            get
            {
                if (p2_PointsOnServe == 0) return 0;
                else return (p2_PointsOnServe - p2_FirstWon - p2_SecondWon) / (double)p2_PointsOnServe;
            }
        }
        public double p2_ReturnWinningPercentage
        {
            get
            {
                if (p1_PointsOnServe == 0) return 0;
                else return (p1_PointsOnServe - p1_FirstWon - p1_SecondWon) / (double)p1_PointsOnServe;
            }
        }

        public double p1_PointsWonPercentagePerGame
        {
            get
            {
                if (p1_PointsOnServe + p2_PointsOnServe == 0 || p1_GamesOnServe + p2_GamesOnServe == 0) return 0;
                else return (((p1_FirstWon + p1_SecondWon) + (p2_PointsOnServe - p2_FirstWon - p2_SecondWon)) / (double)(p1_PointsOnServe + p2_PointsOnServe)) / (p1_GamesOnServe + p2_GamesOnServe);
            }
        }
        public double p2_PointsWonPercentagePerGame
        {
            get
            {
                if (p1_PointsOnServe + p2_PointsOnServe == 0 || p1_GamesOnServe + p2_GamesOnServe == 0) return 0;
                else return (((p2_FirstWon + p2_SecondWon) + (p1_PointsOnServe - p1_FirstWon - p1_SecondWon)) / (double)(p1_PointsOnServe + p2_PointsOnServe)) / (p1_GamesOnServe + p2_GamesOnServe);
            }
        }

        public double p1_DoubleFaultsPercentagePerGame
        {
            get
            {
                if (p1_GamesOnServe == 0 || p1_PointsOnServe == 0) return 0;
                else return ((double)p1_DoubleFaults / p1_PointsOnServe) / p1_GamesOnServe;
            }
        }
        public double p2_DoubleFaultsPercentagePerGame
        {
            get
            {
                if (p2_GamesOnServe == 0 || p2_PointsOnServe == 0) return 0;
                else return ((double)p2_DoubleFaults / p2_PointsOnServe) / p2_GamesOnServe;
            }
        }

        public double p1_BreakPointsWonPercentage
        {
            get
            {
                if (p2_BreakpointsFaced == 0) return 0;
                else return (p2_BreakpointsFaced - p2_BreakpointsSaved) / (double)p2_BreakpointsFaced;
            }
        }
        public double p2_BreakPointsWonPercentage
        {
            get
            {
                if (p1_BreakpointsFaced == 0) return 0;
                else return (p1_BreakpointsFaced - p1_BreakpointsSaved) / (double)p1_BreakpointsFaced;
            }
        }

        public double p1_Completeness
        {
            get
            {
                return p1_OverallWinningOnServePercentage * p1_ReturnWinningPercentage;
            }
        }
        public double p2_Completeness
        {
            get
            {

                return p2_OverallWinningOnServePercentage * p2_ReturnWinningPercentage;
            }
        }

        public double p1_AdvantageOnServe
        {
            get
            {
                return p1_OverallWinningOnServePercentage - p2_ReturnWinningPercentage;
            }
        }
        public double p2_AdvantageOnServe
        {
            get
            {
                return p2_OverallWinningOnServePercentage - p1_ReturnWinningPercentage;
            }
        }

        public double p1_AvgUnforcedErrorsPerGame
        {
            get
            {
                if (p1_GamesOnServe == 0) return 0;
                return p1_UnforcedErrors / p1_GamesOnServe;
            }
        }
        public double p2_AvgUnforcedErrorsPerGame
        {
            get
            {
                if (p2_GamesOnServe == 0) return 0;
                return p2_UnforcedErrors / p2_GamesOnServe;
            }
        }

        public double p1_AvgWinnersPerGame
        {
            get
            {
                if (p1_GamesOnServe == 0) return 0;
                return p1_Winners / p1_GamesOnServe;
            }
        }
        public double p2_AvgWinnersPerGame
        {
            get
            {
                if (p2_GamesOnServe == 0) return 0;
                return p2_Winners / p2_GamesOnServe;
            }
        }

        public double p1_PercentageNetApproachesWon
        {
            get
            {
                if (p1_NetApproaches == 0) return 0;
                return p1_NetApproachesWon / p1_NetApproaches;
            }
        }
        public double p2_PercentageNetApproachesWon
        {
            get
            {
                if (p2_NetApproaches == 0) return 0;
                return p2_NetApproachesWon / p2_NetApproaches;
            }
        }


        /// <summary>
        /// Swap Player1 and Player2.
        /// </summary>
        public void Swap()
        {
            int temp1 = p1_Aces; p1_Aces = p2_Aces; p2_Aces = temp1;
            temp1 = p1_DoubleFaults; p1_DoubleFaults = p2_DoubleFaults; p2_DoubleFaults = temp1;
            temp1 = p1_PointsOnServe; p1_PointsOnServe = p2_PointsOnServe; p2_PointsOnServe = temp1;
            temp1 = p1_FirstIn; p1_FirstIn = p2_FirstIn; p2_FirstIn = temp1;
            temp1 = p1_FirstWon; p1_FirstWon = p2_FirstWon; p2_FirstWon = temp1;
            temp1 = p1_SecondWon; p1_SecondWon = p2_SecondWon; p2_SecondWon = temp1;
            temp1 = p1_GamesOnServe; p1_GamesOnServe = p2_GamesOnServe; p2_GamesOnServe = temp1;
            temp1 = p1_BreakpointsSaved; p1_BreakpointsSaved = p2_BreakpointsSaved; p2_BreakpointsSaved = temp1;
            temp1 = p1_BreakpointsFaced; p1_BreakpointsFaced = p2_BreakpointsFaced; p2_BreakpointsFaced = temp1;
            temp1 = p1_MaxServeSpeed; p1_MaxServeSpeed = p2_MaxServeSpeed; p2_MaxServeSpeed = temp1;
            temp1 = p1_Avg1stServeSpeed; p1_Avg1stServeSpeed = p2_Avg1stServeSpeed; p2_Avg1stServeSpeed = temp1;
            temp1 = p1_Avg2ndServeSpeed; p1_Avg2ndServeSpeed = p2_Avg2ndServeSpeed; p2_Avg2ndServeSpeed = temp1;
            temp1 = p1_Winners; p1_Winners = p2_Winners; p2_Winners = temp1;
            temp1 = p1_UnforcedErrors; p1_UnforcedErrors = p2_UnforcedErrors; p2_UnforcedErrors = temp1;
            temp1 = p1_NetApproaches; p1_NetApproaches = p2_NetApproaches; p2_NetApproaches = temp1;
            temp1 = p1_NetApproachesWon; p1_NetApproachesWon = p2_NetApproachesWon; p2_NetApproachesWon = temp1;
        }

    }
}
