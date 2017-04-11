using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        public string parThreePinWinners { get; set; }

        public int wagerAmount { get; set; }

        public string gameSelection { get; set; }

        public ResultsModel results { get; set; }
        public int frontNineTotalPar { get; set; }
        public int backNineTotalPar { get; set; }
        public int parTotal { get; set; }

    }
}
