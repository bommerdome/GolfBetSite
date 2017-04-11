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
            model.players.RemoveAll(m => m.strokePerHole.Count() == 0); //MODEL COMES WITH 4 PLAYERS NO MATTER WHAT, KILL PLAYERS NOT MEANT TO BE SET -- NEEDS FIXING

            if (isValidModel(model) != true)   //VIEWS AND MODELS MUST BE REFACTORED TO USE ModelState.IsValid, THIS IS A HACK KIND OF     TODO: REFACTOR
            {
                return View();
            }

            GameModel resultsModel = calculateTotalsAndScores(model);
            switch (model.gameSelection)
            {
                case "skins":
                    resultsModel = playSkins(resultsModel);
                    break;
                case "nasau":
                    //do something else
                    break;
                default:
                    //do a different thing, probably skins
                    break;
            }


            return View("Scorecard", resultsModel);


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

        [HttpPost]
        public IActionResult Scorecard(GameModel model)
        {
            return View();
        }


        public GameModel calculateTotalsAndScores(GameModel model)
        {
            model.parTotal = model.parValues.Sum();
            model.frontNineTotalPar = model.parValues.Take(9).Sum();
            model.backNineTotalPar = model.parValues.Skip(9).Sum();

            int y = 1;
            foreach (PlayersModel player in model.players) //INITIALIZE PLAYER ATTRIBUTES
            {
                player.frontNineTotalStrokes = player.strokePerHole.Take(9).Sum();
                player.backNineTotalStrokes = player.strokePerHole.Skip(9).Sum();
                player.totalStrokes = player.strokePerHole.Sum();
                player.totalScore = player.totalStrokes - model.parTotal;

                player.totalPutts = player.puttsPerHole.Sum();
                player.frontNineTotalPutts = player.puttsPerHole.Take(9).Sum();
                player.backNineTotalPutts = player.puttsPerHole.Skip(9).Sum();

                player.totalSkins = 0;
                player.front9StrokePlaySkins = 0;
                player.back9StrokePlaySkins = 0;
                player.totalStrokePlaySkins = 0;
                player.scorePerHole = new List<int>();

                for (int i = 0; i < model.numberOfHoles; i++) 
                {
                    player.scorePerHole.Add(player.strokePerHole[i] - model.parValues[i]);
                }
            }

            return model;
        }

        public GameModel playSkins(GameModel model)
        {
            //
            //CALCULATE EACH PLAYERS SKINS WON FOR INDIVIDUAL HOLE STROKE PLAY
            //
            int currentCarryOverValue = 1;
            model.results = new ResultsModel();
            model.results.holeWinner = new List<string>();

            for (int i = 0; i < model.numberOfHoles; i++) 
            {
                Dictionary<string,int> minimumScoresForHole = new Dictionary<string, int>();

                int lowScoreForHole = 0; //NOBODY WILL SCORE IF NOT GETTING A PAR(O) OR BETTER

                foreach (PlayersModel player in model.players)
                {

                    if (player.scorePerHole[i] == lowScoreForHole) //IF TIED TO CURRENT LOW, ADD TO SCORERS LIST
                    {
                        minimumScoresForHole.Add(player.playerName, player.scorePerHole[i]);
                    }
                    if (player.scorePerHole[i] < lowScoreForHole) //IF BEATING CURRENT LOW, KILL EVERYONE IN LIST AND START NEW LIST FOR NEW LOW SCORE
                    {
                        minimumScoresForHole.Clear();
                        minimumScoresForHole.Add(player.playerName, player.scorePerHole[i]);
                        lowScoreForHole = player.scorePerHole[i];
                    }

                }

                if (minimumScoresForHole.Count() == 1) // if there is only one distinct low score, award skins to winning player
                {
                    int bonusMultiplier = 0;
                    switch (minimumScoresForHole.First().Value)
                    {
                        case -1:                        //BIRDIE - WORTH DOUBLE
                            bonusMultiplier = 2;
                            break;
                        case -2:                        //EAGLE - WORTH TRIPLE
                            bonusMultiplier = 3;
                            break;
                        case -3:                        //ALBATROSS/HOLE IN ONE - WORTH TRIPLE
                            bonusMultiplier = 4;
                            break;
                        default:                        //BIRDIE - WORTH DOUBLE
                            bonusMultiplier = 1;
                            break;
                    }

                    foreach (PlayersModel player in model.players)//TODO: THIS NEEDS TO BE REFACTORED, CRAPPY WAY OF SELECTING WINNING PLAYER FROM EXISTING MODEL
                    {
                        if (player.playerName == minimumScoresForHole.First().Key.ToString())
                        {
                            if (i < 9)
                            {
                                model.results.holeWinner.Add(player.playerName);
                                player.front9StrokePlaySkins = player.front9StrokePlaySkins + (currentCarryOverValue * bonusMultiplier);
                                player.totalSkins = player.totalSkins + (currentCarryOverValue * bonusMultiplier);
                            }
                            if(i >= 9)
                            {
                                model.results.holeWinner.Add(player.playerName);
                                player.back9StrokePlaySkins = player.back9StrokePlaySkins + (currentCarryOverValue * bonusMultiplier);
                                player.totalSkins = player.totalSkins + (currentCarryOverValue * bonusMultiplier);
                            }
                            
                        }
                    }
                currentCarryOverValue = 1;                  //RESET CARRY OVER TO 1 BECAUSE PLAYER SCORED

                }
                else
                {
                    model.results.holeWinner.Add("wash");
                    currentCarryOverValue++;                //HOLE WAS WASH, HOLE SKIN "CARRY OVER" AND ADDED TO NEXT HOLE
                }
                
            }


            //CALCULATE STROKE WINNERS FOR FRONT 9, BACK 9, AND TOTAL
            //NOTE: SHOULD EXTRACT THIS FOR USE IN FUTURE GAMES
            #region STROKE WINNER CALCULATIONS
            Dictionary<string, int> frontStrokes = new Dictionary<string,int>();
            Dictionary<string, int> backStrokes = new Dictionary<string, int>();
            Dictionary<string, int> totalStrokes = new Dictionary<string, int>();
            int strokeTieCarryOver = 1;

            foreach(PlayersModel player in model.players)
            {
                frontStrokes.Add(player.playerName, player.frontNineTotalStrokes);
                backStrokes.Add(player.playerName, player.backNineTotalStrokes);
                totalStrokes.Add(player.playerName, player.totalStrokes);
            }

            int frontNineLowStrokes = frontStrokes.Values.Min();
            int backNineLowStrokes = backStrokes.Values.Min();
            int overallLowStrokes = totalStrokes.Values.Min();


            //FRONT 9 STROKES 
            if (model.players.Where(m => m.frontNineTotalStrokes == frontNineLowStrokes).Count() > 1)    //IF MORE THAN ONE PLAYER MATCH TOTAL LOW - WASH
            {
                model.results.frontNineStrokesWinner = new KeyValuePair<string, int>("Wash", strokeTieCarryOver);
                strokeTieCarryOver++;
            }
            else
            {
                model.results.frontNineStrokesWinner =
                    new KeyValuePair<string, int>(model.players.Where(m => m.frontNineTotalStrokes == frontNineLowStrokes).First().playerName, strokeTieCarryOver);

                model.players.Where(m => m.playerName == model.results.frontNineStrokesWinner.Key).First().totalSkins++;
            }

            if(model.numberOfHoles ==18)
            { 
                //BACK 9 STROKES 
                if (model.players.Where(m => m.backNineTotalStrokes == backNineLowStrokes).Count() > 1)  //IF MORE THAN ONE PLAYER MATCH TOTAL LOW - WASH
                {
                    model.results.backNineStrokesWinner = new KeyValuePair<string, int>("Wash", strokeTieCarryOver);
                    strokeTieCarryOver++;
                }
                else
                {
                    model.results.backNineStrokesWinner 
                        = new KeyValuePair<string, int>(model.players.Where(m => m.backNineTotalStrokes == backNineLowStrokes).First().playerName, strokeTieCarryOver);
                
                    model.players.Where(m => m.playerName == model.results.backNineStrokesWinner.Key).First().totalSkins =
                        model.players.Where(m => m.playerName == model.results.backNineStrokesWinner.Key).First().totalSkins + strokeTieCarryOver;
                    strokeTieCarryOver = 1;
                }

                //OVERALL STROKES 
                if (model.players.Where(m => m.totalStrokes == overallLowStrokes).Count() > 1)  //IF MORE THAN ONE PLAYER MATCH TOTAL LOW - WASH
                {
                    model.results.overallStrokesWinner = new KeyValuePair<string, int>("Wash", strokeTieCarryOver);
                }
                else
                {
                    model.results.overallStrokesWinner =
                        new KeyValuePair<string, int>(model.players.Where(m => m.totalStrokes == overallLowStrokes).First().playerName, strokeTieCarryOver);

                    model.players.Where(m => m.playerName == model.results.overallStrokesWinner.Key).First().totalSkins =
                        model.players.Where(m => m.playerName == model.results.overallStrokesWinner.Key).First().totalSkins + strokeTieCarryOver;
                    strokeTieCarryOver = 1;
                }
            }
            #endregion


            //CALCULATE PUTT WINNERS FOR FRONT 9, BACK 9, AND TOTAL
            #region OVERALL PUTTS WINNER CALCULATIONS
            Dictionary<string, int> frontPutts = new Dictionary<string, int>();
            Dictionary<string, int> backPutts = new Dictionary<string, int>();
            Dictionary<string, int> totalPutts = new Dictionary<string, int>();
            int puttTieCarryOver = 1;
            foreach (PlayersModel player in model.players)
            {
                frontPutts.Add(player.playerName, player.frontNineTotalPutts);
                backPutts.Add(player.playerName, player.backNineTotalPutts);
                totalPutts.Add(player.playerName, player.totalPutts);
            }
            int frontNineLowPutts = frontPutts.Values.Min();
            int backNineLowPutts = backPutts.Values.Min();
            int overallLowPutts = totalPutts.Values.Min();


            //FRONT 9 PUTTS 
            if (model.players.Where(m => m.frontNineTotalPutts == frontNineLowPutts).Count() > 1)    //IF MORE THAN ONE PLAYER MATCH TOTAL LOW - WASH
            {
                model.results.frontNinePuttsWinner = new KeyValuePair<string, int>("Wash", puttTieCarryOver);
                puttTieCarryOver++;
            }
            else
            {
                model.results.frontNinePuttsWinner =
                    new KeyValuePair<string, int>(model.players.Where(m => m.frontNineTotalPutts == frontNineLowPutts).First().playerName, puttTieCarryOver);
                model.players.Where(m => m.playerName == model.results.frontNinePuttsWinner.Key).First().totalSkins++;
            }

            if (model.numberOfHoles == 18)
            {

                //BACK 9 PUTTS 
                if (model.players.Where(m => m.backNineTotalPutts == backNineLowPutts).Count() > 1)  //IF MORE THAN ONE PLAYER MATCH TOTAL LOW - WASH
                {
                    model.results.backNinePuttsWinner = new KeyValuePair<string, int>("Wash", puttTieCarryOver);
                    puttTieCarryOver++;
                }
                else
                {
                    model.results.backNinePuttsWinner
                        = new KeyValuePair<string, int>(model.players.Where(m => m.backNineTotalPutts == backNineLowPutts).First().playerName, puttTieCarryOver);

                    model.players.Where(m => m.playerName == model.results.backNinePuttsWinner.Key).First().totalSkins =
                        model.players.Where(m => m.playerName == model.results.backNinePuttsWinner.Key).First().totalSkins + puttTieCarryOver;
                    puttTieCarryOver = 1;
                }

                //OVERALL PUTTS 
                if (model.players.Where(m => m.totalPutts == overallLowPutts).Count() > 1)  //IF MORE THAN ONE PLAYER MATCH TOTAL LOW - WASH
                {
                    model.results.overallPuttsWinner = new KeyValuePair<string, int>("Wash", puttTieCarryOver);
                }
                else
                {
                    model.results.overallPuttsWinner =
                        new KeyValuePair<string, int>(model.players.Where(m => m.totalPutts == overallLowPutts).First().playerName, puttTieCarryOver);

                    model.players.Where(m => m.playerName == model.results.overallPuttsWinner.Key).First().totalSkins =
                        model.players.Where(m => m.playerName == model.results.overallPuttsWinner.Key).First().totalSkins + puttTieCarryOver;
                    puttTieCarryOver = 1;
                }
            }
            #endregion

            //CALCULATE THE OVERALL WINNER OF THE SKINS GAME
            Dictionary<string, int> totalSkinsWonList = new Dictionary<string, int>();
            model.results.playersWithSkinsOwed = new Dictionary<string, int>();
            model.results.playersWithAmountOwed = new Dictionary<string, int>();
            foreach (PlayersModel player in model.players)
            {
                totalSkinsWonList.Add(player.playerName, player.totalSkins);
            }
            int mostSkinsWon = totalSkinsWonList.Values.Max();

            foreach (PlayersModel player in model.players.Where(m => m.totalSkins == mostSkinsWon))
            {
                model.results.playersWithSkinsOwed.Add(player.playerName, 0);
                model.results.playersWithAmountOwed.Add(player.playerName, 0);
            }
            foreach (PlayersModel player in model.players.Where(m=>m.totalSkins < mostSkinsWon))
            {
                model.results.playersWithSkinsOwed.Add(player.playerName, mostSkinsWon - player.totalSkins);
                model.results.playersWithAmountOwed.Add(player.playerName, (mostSkinsWon - player.totalSkins) * model.wagerAmount);
            }

            return model;
        }

        public bool isValidModel(GameModel model)
        {
            bool isValid = true;
            foreach (var modelValue in ModelState.Values) //CLEAR EXISTING MODELSTATE ERRORS 
            {
                modelValue.Errors.Clear();
            }

            if (model.numberOfPlayers == 0)
            {
                ModelState.AddModelError("", "SELECT NUMBER OF PLAYERS");
                isValid = false;
            }

            if(model.numberOfHoles == 0)
            {
                ModelState.AddModelError("", "SELECT NUMBER OF HOLES");
                isValid = false;
            }

            if (model.parValues.Count() < model.numberOfHoles)
            {
                ModelState.AddModelError("", "ALL PAR VALUES MUST BE ENTERED");
                isValid = false;
            }

            if(string.IsNullOrEmpty(model.gameSelection))
            {
                ModelState.AddModelError("", "SELECT A GAME");
                isValid = false;
            }

            if(model.gameSelection != "skins")
            {
                ModelState.AddModelError("", "SORRY THIS GAME IS NOT AVAIALABLE YET, COMING SOON!");
                isValid = false;
            }

            if(model.wagerAmount == 0)
            {
                ModelState.AddModelError("", "ENTER A WAGER AMOUNT");
                isValid = false;
            }

            bool duplicateName = false;
            foreach (PlayersModel player in model.players)
            {

                if (string.IsNullOrEmpty(player.playerName))
                {
                    ModelState.AddModelError("", "PLAYER NAME REQUIRED");
                    isValid = false;
                }

                //NO DUPLICATE PLAYER NAMES
                if(model.players.Where(x => x.playerName == player.playerName).Count() > 1 && duplicateName == false)
                {
                    duplicateName = true;
                    ModelState.AddModelError("", "PLAYERS MAY NOT HAVE SAME NAME");
                    isValid = false;
                }

                if(player.strokePerHole.Count() < model.numberOfHoles)
                {
                    ModelState.AddModelError("", "ENTER STROKE VALUE (Player :" + player.playerName + " Hole:" + (player.strokePerHole.Count() + 1) + ")");
                    isValid = false;
                }

                if(player.puttsPerHole.Count() < model.numberOfHoles)
                {
                    ModelState.AddModelError("", "ENTER PUTT VALUE (Player: " + player.playerName + " Hole:" + (player.puttsPerHole.Count() + 1) + ")");
                    isValid = false;
                }

                if (player.puttsPerHole.Count() == model.numberOfHoles && player.strokePerHole.Count() == model.numberOfHoles)
                {
                    for (int i = 0; i < model.numberOfHoles; i++)
                    {
                        if (player.puttsPerHole[i] >= player.strokePerHole[i])
                        {
                            ModelState.AddModelError("", "CANNOT HAVE MORE PUTTS THAN STROKES (" + player.playerName + ")");
                            isValid = false;
                        }
                    }
                }

            }

            return isValid;
        }

    }
}
