using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GolfBets.Models
{
    public class ResultsModel
    {
        public int frontNineTotalPar { get; set; }
        public int backNineTotalPar { get; set; }
        public int parTotal { get; set; }

        public GameModel game { get; set; }

        public List<int> holeMultiplierValue { get; set; }
        public List<string> holeWinner { get; set; }

        //NASSAU RESULTS 
        public KeyValuePair<string, int> frontNineStrokesWinner { get; set; }   //KEY - Player Name of Winner Value - amount won
        public KeyValuePair<string, int> frontNinePuttsWinner { get; set; }     //KEY - Player Name of Winner Value - amount won

        public KeyValuePair<string, int> backNineStrokesWinner { get; set; }    //KEY - Player Name of Winner Value - amount won
        public KeyValuePair<string, int> backNinePuttsWinner { get; set; }      //KEY - Player Name of Winner Value - amount won

        public KeyValuePair<string, int> overallStrokesWinner { get; set; }     //KEY - Player Name of Winner Value - amount won
        public KeyValuePair<string, int> overallPuttsWinner { get; set; }       //KEY - Player Name of Winner Value - amount won

        public List<int> parThreeHoles { get; set; }
        public Dictionary<string,int> parThreePinWinners { get; set; }

        public int winnerTotalSkins { get; set; }
    }
}
