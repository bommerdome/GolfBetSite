using System;
using System.Collections.Generic;
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

        public int front9StrokePlaySkins { get; set; }
        public int back9StrokePlaySkins { get; set; }
        public int totalStrokePlaySkins { get; set; }
        public int totalSkins { get; set; }

        public List<int> holesWonOutright { get; set; }
        public bool isWinner { get; set; }
    }
}
