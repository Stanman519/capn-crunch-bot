using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CapnCrunchGMBot.Interfaces;
using CapnCrunchGMBot.Models;

namespace CapnCrunchGMBot.Services
{
    public interface IRumorService
    {
        public string GetSources();
        string ListPlayers(List<Player> players, bool hasEarlyPicks);
        string AddBaitAction();
        string ListPlayer(Player player, bool hasEarlyPicks);
        bool CheckForFirstRounders(string offeringIds);
        bool CheckForMultiplePlayers(string idArray);
        Task<string> ListTradeInfoWithMultiplePlayers(string assets);
        Task<string> ListTradeInfoWithSinglePlayer(string asset);
    }

    public class RumorService : IRumorService
    {
        private readonly IMflApi _mflApi;
        Random rnd = new Random();

        public RumorService(IMflApi mflApi)
        {
            _mflApi = mflApi;
        }

        public string GetSources()
        {
            var value = "";
            int sourceKey = rnd.Next(1, 9);
            sources.TryGetValue(sourceKey, out value);
            return value;
        }

        public string ListPlayers(List<Player> players, bool hasEarlyPicks)
        {
            var ret = "";
            if (players.Count == 0 && hasEarlyPicks) return " is trying to make a deal with some of his early picks.";
            if (players.Count == 0)
                return
                    " is looking to wheel and deal but we don't have the details on who they are shopping. Stay tuned.";
            for (int i = 0; i < players.Count; i++)
            {
                var nameArray = players[i].name.Split(",");
                var name = nameArray[1].Trim() + " " + nameArray[0];
                ret += name;
                if (players.Count >= 3 && i < players.Count - 2) ret += ", ";
                if (i == players.Count - 2) ret += " & ";
            }
            if (hasEarlyPicks) ret += "." + GetPickTalk();
            return ret;
        }

        public string GetPickTalk()
        {
            var value = "";
            int sourceKey = rnd.Next(1, 5);
            pickTalk.TryGetValue(sourceKey, out value);
            return value;
        }

        public string ListPlayer(Player player, bool hasEarlyPicks)
        {
            var ret = "";
            var nameArray = player.name.Split(",");
            var name = nameArray[1].Trim() + " " + nameArray[0] + " ";
            ret += name;
            return ret;
        }

        public string AddBaitAction()
        {
            var value = "";
            int sourceKey = rnd.Next(1, 7);
            baitVerbs.TryGetValue(sourceKey, out value);
            return value;
        }

        public bool CheckForMultiplePlayers(string ids)
        {
            if (!ids.Contains(",")) return false;
            var idArr = ids.Split(",");
            var onlyPlayers = idArr.Where(a => !a.Contains("_")).ToList();
            if (onlyPlayers.Count > 1) return true;
            return false;
        }
        public bool CheckForFirstRounders(string offeringIds)
        {
            var splitIds = offeringIds.Split(",");
            var picksOnly = splitIds.Where(a => a.Contains("_"));
            foreach (var str in picksOnly)
            {
                var pickDetails = str.Split("_");
                if (pickDetails[0].ToUpper() == "DP" && (pickDetails[1] == "0" || pickDetails[1] == "1")) return true;
                if (pickDetails[0].ToUpper() == "FP" && (pickDetails[^1] == "1" ||
                                                         pickDetails[^1] == "2")) return true;
            }
            return false;
        }

        public async Task<string> ListTradeInfoWithMultiplePlayers(string assets)
        {
            var ret = "";
            var salary = "";
            var years = "";
            var splitIds = assets.Split(",");
            var playerList = new List<Player>();

            var picksOnly = splitIds.Where(a => a.Contains("_")).ToList();
            var onlyPlayersList = splitIds.Where(a => !a.Contains("_")).ToList();
            // if trailing comma, delete last
            if (onlyPlayersList[^1] == "") 
                onlyPlayersList.RemoveAt(onlyPlayersList.Count - 1);
            var onlyPlayers = String.Join(",", onlyPlayersList);
            if (onlyPlayersList.Count > 1) 
                playerList = (await _mflApi.GetPlayersDetails(onlyPlayers)).players.player;
            if (onlyPlayersList.Count == 1)
                playerList.Add((await _mflApi.GetPlayerDetails(onlyPlayers)).players.player);

            var salaries = await _mflApi.GetPlayerSalaries();
            
            foreach (var player in playerList)
            {
                var nameArray = player.name.Split(",");
                var name = nameArray[1].Trim() + " " + nameArray[0];
                var contract = salaries.salaries.leagueUnit.player.FirstOrDefault(_ => _.id == player.id);
                salary = contract?.salary;
                years = contract?.contractYear;
                ret += $"{name} (${salary}, {years} yrs left)\n";
            }
            
            foreach (var pick in picksOnly)
            {
                var pickString = "";
                var pickDetails = pick.Split("_");
                if (pickDetails[0].ToUpper() == "DP")
                {
                    var round = Int32.Parse(pickDetails[1]) + 1;
                    var pickNum = Int32.Parse(pickDetails[2]) + 1;
                    pickString = $"The {round}.{pickNum} draft pick \n";
                }
                if (pickDetails[0].ToUpper() == "FP")
                    pickString = $"A {pickDetails[2]} round {pickDetails[3]} draft pick \n";

                ret += pickString;
            }
            return ret;
        }       
        public async Task<string> ListTradeInfoWithSinglePlayer(string assets)
        {
            var ret = "";
            var salary = "";
            var years = "";
            var splitAssets = assets.Split(",");
            foreach (var asset in splitAssets)
            {
                if (!assets.Contains("_"))
                {
                    var res = await _mflApi.GetPlayerDetails(assets);
                    var salaries = await _mflApi.GetPlayerSalaries();
                    var player = res.players.player;
                    var nameArray = player.name.Split(",");
                    var name = nameArray[1].Trim() + " " + nameArray[0];
                    var contract = salaries.salaries.leagueUnit.player.FirstOrDefault(_ => _.id == player.id);
                    salary = contract?.salary;
                    years = contract?.contractYear;
                    ret += $"{name} (${salary}, {years} yrs left) \n";
                    return ret;
                }
                var pickDetails = asset.Split("_");
                if (pickDetails[0].ToUpper() == "DP")
                {
                    var round = Int32.Parse(pickDetails[1]) + 1;
                    var pickNum = Int32.Parse(pickDetails[2]) + 1;
                    ret += $"The {round}.{pickNum} draft pick \n";
                }
                if (pickDetails[0].ToUpper() == "FP")
                    ret += $"A {pickDetails[2]} round {pickDetails[3]} draft pick \n";
            }
            return ret;
        }

        private Dictionary<int, string> sources = new Dictionary<int, string>()
        {
            {1, "My sources are telling me "},
            {2, "Sources close to the situation say "},
            {3, "Franchise execs say "},
            {4, "Per source, "},
            {5, "Per Adam Schefter "},
            {6, "One league source: "},
            {7, "I'm told that "},
            {8, "There's a growing assumption around the league that "},
            {9, "Rumors are circulating that "}
        };
        private Dictionary<int, string> baitVerbs = new Dictionary<int, string>()
        {

            {1, "is shopping "},
            {2, "is looking to trade "},
            {3, "is keen on moving "},
            {4, "is trying to make a deal involving "},
            {5, "is contacting other execs to gauge interest on "},
            {6, "has made it publicly known he'd like to make a deal involving "},
            {7, "has reached out to other owners trying to shop "}
        };
        private Dictionary<int, string> pickTalk = new Dictionary<int, string>()
        {
            {1, " Word is that he's also open to moving some valuable picks. "},
            {2, " I'm also told that early draft picks are on the table. "},
            {3, " From what I've heard, he isn't afraid to move some early round picks either. "},
            {4, " Execs are also saying that early picks could be acquired from the org. "},
            {5, " There's a growing sense that they're also shopping some valuable picks. "},
        };
    }
}