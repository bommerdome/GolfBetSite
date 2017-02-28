using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GolfBets.Models;
using System.Data;

namespace GolfBets.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(GameModel model)
        {
            List<List<int>> scoreCard = new List<List<int>>(); //(zero index) Hole, Par Val, P1 Score, P2 Score... 
            scoreCard.Add(model.parValues);
            foreach(PlayersModel player in model.players)
            {
                scoreCard.Add(player.holeScores);
            }

            List<List<int>> results = calculateScores(scoreCard,model.numberOfHoles);




            return View(model);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Here's a look at all the games our application supports!";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        public List<List<int>> calculateScores(List<List<int>> scoreCard, int holesPlayed)
        {
            List<List<int>> finalPlayerScores = new List<List<int>>(); //ParValues, P1Scores, P2Scores,P3Scores

            finalPlayerScores.Add(scoreCard[0]);

            int totalPar = scoreCard[0].Sum();
            int playerOneTotalStrokes = scoreCard[1].Sum();
            int playerTwoTotalStrokes = scoreCard[2].Sum();

            List<int> playerOneScorePerHole = new List<int>();
            List<int> playerTwoScorePerHole = new List<int>();
            for (int i = 0 ; i<holesPlayed; i++) //NEED PARAMETER FOR HOW MANY HOLES BEING PLAYED (9/18) --DONE
            {
                playerOneScorePerHole.Add(scoreCard[1][i] - scoreCard[0][i]);
                playerTwoScorePerHole.Add(scoreCard[2][i] - scoreCard[0][i]);
            }
            int playerOneTotalScore = playerOneScorePerHole.Sum();
            int playerTwoTotalScore = playerTwoScorePerHole.Sum();

            return finalPlayerScores;
        }

    }
}
