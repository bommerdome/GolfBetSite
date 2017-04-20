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

        public string gameSelection { get; set; }

        [Display(Name="Skins")]
        public bool skinsSelected { get; set; }
        [Display(Name="Nassau")]
        public bool nassauSelected { get; set; }
        [Display(Name ="Bet on Putts?")]
        public bool nassauPuttSelected { get; set; }
        [Display(Name="Round Robin")]
        public bool roundRobinSelected { get; set; }

        [Display(Name = "Skin Wager:")]
        public int skinWager { get; set; }
        [Display(Name = "Nassau Wager:")]
        public int nassauWager { get; set; }
        [Display(Name = "Round Robin Wager:")]
        public int roundRobinWager { get; set; }

    }
}
