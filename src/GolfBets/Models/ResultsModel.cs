using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GolfBets.Models
{
    public class ResultsModel
    { 
        public List<int> holeMultiplierValue { get; set; }
        public List<string> holeWinner { get; set; }


        public KeyValuePair<string,int> frontNineStrokesWinner { get; set; }
        public KeyValuePair<string, int> frontNinePuttsWinner { get; set; }

        public KeyValuePair<string, int> backNineStrokesWinner { get; set; }
        public KeyValuePair<string, int> backNinePuttsWinner { get; set; }

        public KeyValuePair<string, int> overallStrokesWinner { get; set; }
        public KeyValuePair<string, int> overallPuttsWinner { get; set; }

        public List<string> parThreePinWinners { get; set; } 

        public Dictionary<string,int> playersWithSkinsOwed { get; set; }
        public Dictionary<string,int> playersWithAmountOwed { get; set; }
 

        public int winnerTotalSkins { get; set; }
    }
}
