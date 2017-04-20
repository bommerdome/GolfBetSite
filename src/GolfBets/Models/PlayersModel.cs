using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GolfBets.Models
{
    public class PlayersModel
    {
        public string playerName { get; set; }

        public List<int> strokePerHole { get; set; }

        public List<int> scorePerHole { get; set; }

        public List<int> puttsPerHole { get; set; }

        public int frontNineTotalPutts { get; set; }
        public int backNineTotalPutts { get; set; }
        public int totalPutts { get; set; }

        public int frontNineTotalStrokes { get; set; }
        public int backNineTotalStrokes { get; set; }
        public int totalStrokes { get; set; }
        public int totalScore { get; set; }


        //OUTPUTS FOR SCORECARD RESULTS

        //SKINS
        public int front9StrokePlaySkins { get; set; }
        public int back9StrokePlaySkins { get; set; }
        public int totalStrokePlaySkins { get; set; }
        public int totalSkins { get; set; }

        public int skinsOwed { get; set; }
        public int skinsMoneyOwed { get; set; }
        public int skinsMoneyWon { get; set; }

        //Nassau
        public int nassauMoneyWon { get; set; }
        public int nassauMoneyOwed { get; set; }

        //GENERAL
        public List<int> holesWonOutright { get; set; }
        public Dictionary<string,int> betTracker { get; set; } //STRING: Other Player Name   -   VALUE: amount owed to player
        public int totalAmountOwed { get; set; }
        public int totalMoneyWon { get; set; }
    }
}
