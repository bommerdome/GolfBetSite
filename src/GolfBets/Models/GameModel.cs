using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GolfBets.Models
{
    public class GameModel
    {
        public List<PlayersModel> players { get; set; }

        public List<int> parValues { get; set; }

        public int numberOfHoles { get; set; }
        public int numberOfPlayers { get; set; }
        public int wagerAmount { get; set; }
        public string gameSelection { get; set; }

        //Skins
        public int skins { get; set; }

        //Nasau

        //RoundRobin
    }
}
