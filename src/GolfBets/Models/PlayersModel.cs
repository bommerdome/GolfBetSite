using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GolfBets.Models
{
    public class PlayersModel
    {
        public string playerName { get; set; }
        public List<int> holeScores { get; set; }

        public int skins { get; set; }
    }
}
