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
            model.players.RemoveAll(m => string.IsNullOrEmpty(m.playerName)); //MODEL COMES WITH 4 PLAYERS NO MATTER WHAT, KILL PLAYERS NOT MEANT TO BE SET -- NEEDS FIXING

            if (isValidModel(model) != true)   //VIEWS AND MODELS MUST BE REFACTORED TO USE ModelState.IsValid, THIS IS A HACK KIND OF     TODO: REFACTOR
            {
                return View();
            }

            ResultsModel resultsModel = calculateTotalsAndScores(model);
            if(model.skinsSelected == true)
            {
                resultsModel = playSkins(resultsModel);
            }
            if(model.nassauSelected == true)
            {
                resultsModel = playNassau(resultsModel);
            }

            resultsModel.game.players = calculateTotalWinnings(resultsModel.game.players);

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


        public ResultsModel calculateTotalsAndScores(GameModel model)
        {
            ResultsModel results = new ResultsModel()
            {
                game = model,
                parTotal = model.parValues.Sum(),
                frontNineTotalPar = model.parValues.Take(9).Sum(),
                backNineTotalPar = model.parValues.Skip(9).Sum(),
            };
            
            foreach (PlayersModel player in results.game.players) //CALCULATE PLAYER TOTALS
            {
                player.frontNineTotalStrokes = player.strokePerHole.Take(9).Sum();
                player.backNineTotalStrokes = player.strokePerHole.Skip(9).Sum();
                player.totalStrokes = player.strokePerHole.Sum();
                player.totalScore = player.totalStrokes - results.parTotal;

                player.totalPutts = player.puttsPerHole.Sum();
                player.frontNineTotalPutts = player.puttsPerHole.Take(9).Sum();
                player.backNineTotalPutts = player.puttsPerHole.Skip(9).Sum();

                player.totalSkins = 0;
                player.front9StrokePlaySkins = 0;
                player.back9StrokePlaySkins = 0;
                player.totalStrokePlaySkins = 0;

                player.scorePerHole = new List<int>(); //USE STROKES AND PAR VALUES TO CALCULATE ACTUAL SCORE FOR EACH HOLE
                for (int i = 0; i < model.numberOfHoles; i++) 
                {
                    player.scorePerHole.Add(player.strokePerHole[i] - model.parValues[i]);
                }

                player.betTracker = new Dictionary<string, int>();
            }

            return results;
        }

        public ResultsModel playSkins(ResultsModel results)
        {
            //
            //CALCULATE EACH PLAYERS SKINS WON FOR INDIVIDUAL HOLE STROKE PLAY
            //
            #region CALCULATE SKINS FOR EACH HOLE
            int currentCarryOverValue = 1;
            results.holeWinner = new List<string>();

            for (int i = 0; i < results.game.numberOfHoles; i++)  //ITERATE EACH HOLE 
            {
                Dictionary<string,int> minimumScoresForHole = new Dictionary<string, int>(); 

                int lowScoreForHole = 0; //NOBODY WILL SCORE IF NOT GETTING A PAR(O) OR BETTER

                foreach (PlayersModel player in results.game.players)
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

                    foreach (PlayersModel player in results.game.players)
                    {
                        if (player.playerName == minimumScoresForHole.First().Key.ToString())
                        {
                            if (i < 9)
                            {
                                results.holeWinner.Add(player.playerName);
                                player.front9StrokePlaySkins = player.front9StrokePlaySkins + (currentCarryOverValue * bonusMultiplier);
                                player.totalSkins = player.totalSkins + (currentCarryOverValue * bonusMultiplier);
                            }
                            if(i >= 9)
                            {
                                results.holeWinner.Add(player.playerName);
                                player.back9StrokePlaySkins = player.back9StrokePlaySkins + (currentCarryOverValue * bonusMultiplier);
                                player.totalSkins = player.totalSkins + (currentCarryOverValue * bonusMultiplier);
                            }
                            
                        }
                    }
                currentCarryOverValue = 1;                  //RESET CARRY OVER TO 1 BECAUSE PLAYER SCORED

                }
                else
                {
                    results.holeWinner.Add("wash");
                    currentCarryOverValue++;                //HOLE WAS WASH, HOLE SKIN "CARRY OVER" AND ADDED TO NEXT HOLE
                }
                
            }
            #endregion

            #region CALCULATE OVERALL SKINS WINNERS
            //CALCULATE THE OVERALL WINNER OF THE SKINS GAME
            Dictionary<string, int> totalSkinsWonList = new Dictionary<string, int>();
            results.playersWithSkinsOwed = new Dictionary<string, int>();
            results.playersWithAmountOwed = new Dictionary<string, int>();
            foreach (PlayersModel player in results.game.players)
            {
                totalSkinsWonList.Add(player.playerName, player.totalSkins);
            }
            int mostSkinsWon = totalSkinsWonList.Values.Max();

            //IF PLAYER TOTAL SKINS EQUALS MOST SKINS WON, SKINS AND AMOUNT OWED EQUAL ZERO
            foreach (PlayersModel player in results.game.players.Where(m => m.totalSkins == mostSkinsWon))
            {
                player.skinsOwed = 0;
                player.skinsMoneyOwed = 0;
            }

            //IF PLAYER TOTAL SKINS LESS THAN HIGHEST AMOUNT OF SKINS, DETERMINE NUMBER OF SKINS AND AMOUNT
            foreach (PlayersModel player in results.game.players.Where(m=>m.totalSkins < mostSkinsWon))
            {
                player.skinsOwed =  mostSkinsWon - player.totalSkins;
                player.skinsMoneyOwed = (mostSkinsWon - player.totalSkins) * results.game.skinWager;

                foreach (PlayersModel winners in results.game.players.Where(m => m.totalSkins == mostSkinsWon))
                {
                    if (!player.betTracker.ContainsKey(winners.playerName))
                    {
                        player.betTracker.Add(winners.playerName, (mostSkinsWon - player.totalSkins) * results.game.skinWager);
                    }
                    else
                    {
                        player.betTracker[winners.playerName] += (mostSkinsWon - player.totalSkins) * results.game.skinWager;
                    }

                }
            }

            return results;
            #endregion
        }

        public ResultsModel playNassau(ResultsModel results)
        {
            results.game.nassauWager = 1; //DUMMY DATA UNTIL UI IS HOOKED AND VALIDATED

            //CALCULATE STROKE WINNERS FOR FRONT 9, BACK 9, AND TOTAL
            #region STROKE WINNER CALCULATIONS
            Dictionary<string, int> frontStrokes = new Dictionary<string, int>();
            Dictionary<string, int> backStrokes = new Dictionary<string, int>();
            Dictionary<string, int> totalStrokes = new Dictionary<string, int>();
            int strokeTieCarryOver = 1;


            //GET THE LOWEST VALUE FOR COMBINED FRONT, BACK AND TOTAL SCORES
            foreach (PlayersModel player in results.game.players)
            {
                frontStrokes.Add(player.playerName, player.frontNineTotalStrokes);
                backStrokes.Add(player.playerName, player.backNineTotalStrokes);
                totalStrokes.Add(player.playerName, player.totalStrokes);
            }

            int frontNineLowStrokes = frontStrokes.Values.Min();
            int backNineLowStrokes = backStrokes.Values.Min();
            int overallLowStrokes = totalStrokes.Values.Min();

            //DETERMINE IF PLAYER WITH TOTAL SCORES MATCHING THE MINIMUMS HAVE TIED OR WON OUTRIGHT
            //FRONT 9 STROKES
            if (results.game.players.Where(m => m.frontNineTotalStrokes == frontNineLowStrokes).Count() > 1)    //IF MORE THAN ONE PLAYER MATCH TOTAL LOW - WASH
            {
                results.frontNineStrokesWinner = new KeyValuePair<string, int>("Wash", strokeTieCarryOver);
                strokeTieCarryOver++;
            }
            else
            {
                results.frontNineStrokesWinner =
                    new KeyValuePair<string, int>(results.game.players.Where(m => m.frontNineTotalStrokes == frontNineLowStrokes).First().playerName, strokeTieCarryOver * results.game.nassauWager);

                //GIVE EACH LOSING PLAYER A LISTING THAT THEY OWE THE WINNER
                foreach (PlayersModel player in results.game.players.Where(m =>m.playerName != results.frontNineStrokesWinner.Key))
                {
                    if (results.frontNineStrokesWinner.Key != "wash" && !player.betTracker.ContainsKey(results.frontNineStrokesWinner.Key))
                    {
                        player.betTracker.Add(results.frontNineStrokesWinner.Key, strokeTieCarryOver * results.game.nassauWager);
                    }
                    else
                    {
                        player.betTracker[results.frontNineStrokesWinner.Key] += strokeTieCarryOver * results.game.nassauWager;
                    }
                }
            }

            if (results.game.numberOfHoles == 18)
            {
                //BACK 9 STROKES 
                if (results.game.players.Where(m => m.backNineTotalStrokes == backNineLowStrokes).Count() > 1)  //IF MORE THAN ONE PLAYER MATCH TOTAL LOW - WASH
                {
                    results.backNineStrokesWinner = new KeyValuePair<string, int>("Wash", strokeTieCarryOver);
                    strokeTieCarryOver++;
                }
                else
                {
                    results.backNineStrokesWinner
                        = new KeyValuePair<string, int>(results.game.players.Where(m => m.backNineTotalStrokes == backNineLowStrokes).First().playerName, strokeTieCarryOver);
                    strokeTieCarryOver = 1;
                }

                
                foreach (PlayersModel player in results.game.players.Where(m => m.playerName != results.backNineStrokesWinner.Key))
                {
                    if (results.backNineStrokesWinner.Key != "wash" && !player.betTracker.ContainsKey(results.backNineStrokesWinner.Key))
                    {
                        player.betTracker.Add(results.backNineStrokesWinner.Key, strokeTieCarryOver * results.game.nassauWager);
                    }
                    else
                    {
                        player.betTracker[results.backNineStrokesWinner.Key] += strokeTieCarryOver * results.game.nassauWager;
                    }
                }

                //OVERALL STROKES 
                if (results.game.players.Where(m => m.totalStrokes == overallLowStrokes).Count() > 1)  //IF MORE THAN ONE PLAYER MATCH TOTAL LOW - WASH
                {
                    results.overallStrokesWinner = new KeyValuePair<string, int>("Wash", strokeTieCarryOver);
                }
                else
                {
                    results.overallStrokesWinner =
                        new KeyValuePair<string, int>(results.game.players.Where(m => m.totalStrokes == overallLowStrokes).First().playerName, strokeTieCarryOver);
                    strokeTieCarryOver = 1;
                }

                foreach (PlayersModel player in results.game.players.Where(m => m.playerName != results.overallStrokesWinner.Key))
                {
                    if (results.overallStrokesWinner.Key != "wash" && !player.betTracker.ContainsKey(results.overallStrokesWinner.Key))
                    {
                        player.betTracker.Add(results.overallStrokesWinner.Key, strokeTieCarryOver * results.game.nassauWager);
                    }
                    else
                    {
                        player.betTracker[results.overallStrokesWinner.Key] += strokeTieCarryOver * results.game.nassauWager;
                    }
                }
            }
            #endregion


            //CALCULATE PUTT WINNERS FOR FRONT 9, BACK 9, AND TOTAL
            #region OVERALL PUTTS WINNER CALCULATIONS
            Dictionary<string, int> frontPutts = new Dictionary<string, int>();
            Dictionary<string, int> backPutts = new Dictionary<string, int>();
            Dictionary<string, int> totalPutts = new Dictionary<string, int>();
            int puttTieCarryOver = 1;
            foreach (PlayersModel player in results.game.players)
            {
                frontPutts.Add(player.playerName, player.frontNineTotalPutts);
                backPutts.Add(player.playerName, player.backNineTotalPutts);
                totalPutts.Add(player.playerName, player.totalPutts);
            }
            int frontNineLowPutts = frontPutts.Values.Min();
            int backNineLowPutts = backPutts.Values.Min();
            int overallLowPutts = totalPutts.Values.Min();

            //DETERMINE IF PLAYER WITH TOTAL SCORES MATCHING THE MINIMUMS HAVE TIED OR WON OUTRIGHT
            //FRONT 9 PUTTS 
            if (results.game.players.Where(m => m.frontNineTotalPutts == frontNineLowPutts).Count() > 1)    //IF MORE THAN ONE PLAYER MATCH TOTAL LOW - WASH
            {
                results.frontNinePuttsWinner = new KeyValuePair<string, int>("Wash", puttTieCarryOver);
                puttTieCarryOver++;
            }
            else
            {
                results.frontNinePuttsWinner =
                    new KeyValuePair<string, int>(results.game.players.Where(m => m.frontNineTotalPutts == frontNineLowPutts).First().playerName, puttTieCarryOver);
            }

            foreach (PlayersModel player in results.game.players.Where(m => m.playerName != results.frontNinePuttsWinner.Key))
            {
                if (results.frontNinePuttsWinner.Key != "wash" && !player.betTracker.ContainsKey(results.frontNinePuttsWinner.Key))
                {
                    player.betTracker.Add(results.frontNinePuttsWinner.Key, strokeTieCarryOver * results.game.nassauWager);
                }
                else
                {
                    player.betTracker[results.frontNinePuttsWinner.Key] += strokeTieCarryOver * results.game.nassauWager;
                }
            }

            if (results.game.numberOfHoles == 18)
            {

                //BACK 9 PUTTS 
                if (results.game.players.Where(m => m.backNineTotalPutts == backNineLowPutts).Count() > 1)  //IF MORE THAN ONE PLAYER MATCH TOTAL LOW - WASH
                {
                    results.backNinePuttsWinner = new KeyValuePair<string, int>("Wash", puttTieCarryOver);
                    puttTieCarryOver++;
                }
                else
                {
                    results.backNinePuttsWinner
                        = new KeyValuePair<string, int>(results.game.players.Where(m => m.backNineTotalPutts == backNineLowPutts).First().playerName, puttTieCarryOver);
                    puttTieCarryOver = 1;
                }

                foreach (PlayersModel player in results.game.players.Where(m => m.playerName != results.backNinePuttsWinner.Key))
                {
                    if (results.backNinePuttsWinner.Key != "wash" && !player.betTracker.ContainsKey(results.backNinePuttsWinner.Key))
                    {
                        player.betTracker.Add(results.backNinePuttsWinner.Key, strokeTieCarryOver * results.game.nassauWager);
                    }
                    else
                    {
                        player.betTracker[results.backNinePuttsWinner.Key] += strokeTieCarryOver * results.game.nassauWager;
                    }
                }

                //OVERALL PUTTS 
                if (results.game.players.Where(m => m.totalPutts == overallLowPutts).Count() > 1)  //IF MORE THAN ONE PLAYER MATCH TOTAL LOW - WASH
                {
                    results.overallPuttsWinner = new KeyValuePair<string, int>("Wash", puttTieCarryOver);
                }
                else
                {
                    results.overallPuttsWinner =
                        new KeyValuePair<string, int>(results.game.players.Where(m => m.totalPutts == overallLowPutts).First().playerName, puttTieCarryOver);
                    puttTieCarryOver = 1;
                }

                foreach (PlayersModel player in results.game.players.Where(m => m.playerName != results.overallPuttsWinner.Key))
                {
                    if (results.overallPuttsWinner.Key != "wash" && !player.betTracker.ContainsKey(results.overallPuttsWinner.Key))
                    {
                        player.betTracker.Add(results.overallPuttsWinner.Key, strokeTieCarryOver * results.game.nassauWager);
                    }
                    else
                    {
                        player.betTracker[results.overallPuttsWinner.Key] += strokeTieCarryOver * results.game.nassauWager;
                    }
                }
            }
            #endregion


            return results;
        }

        public List<PlayersModel> calculateTotalWinnings(List<PlayersModel> players)
        {
            foreach(PlayersModel player1 in players)
            {
                foreach (PlayersModel player2 in players.Where(m => m.playerName != player1.playerName))
                {
                    //IF EACH PLAYER BEING CHECKED OWES EACHOTHER MONEY, ADJUST BET TRACKER SO THERE IS ONLY 1 WINNER
                    if(player2.betTracker.ContainsKey(player1.playerName) && player1.betTracker.ContainsKey(player2.playerName))
                    {
                        if (player1.betTracker[player2.playerName] < player2.betTracker[player1.playerName])
                        {
                            player1.betTracker[player2.playerName] = 0;
                            player2.betTracker[player1.playerName] += player2.betTracker[player1.playerName] - player1.betTracker[player2.playerName];
                            player1.totalMoneyWon += player2.betTracker[player1.playerName] - player1.betTracker[player2.playerName];
                        }
                        if (player1.betTracker[player2.playerName] == player2.betTracker[player1.playerName])
                        {
                            player1.betTracker.Remove(player2.playerName);
                            player2.betTracker.Remove(player1.playerName);
                        }
                    }
                }
            }

            return players;
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

            if(model.skinsSelected == false && model.nassauSelected == false)
            {
                ModelState.AddModelError("", "SELECT A GAME");
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

                if(model.numberOfPlayers != model.players.Count())
                {
                    ModelState.AddModelError("", "ENTER PLAYER NAME OR CORRECT SELECTED AMOUNT OF PLAYERS");
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
                            ModelState.AddModelError("", "CANNOT HAVE MORE PUTTS THAN STROKES (Player: " + player.playerName + " Hole: " + (i+1) + ")");
                            isValid = false;
                        }
                    }
                }

            }

            return isValid;
        }

    }
}
