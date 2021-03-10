using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CapnCrunchGMBot.Models;

namespace CapnCrunchGMBot
{
    public interface IRumorService
    {
        public string GetSources();
        string ListPlayers(List<Player> players, bool hasEarlyPicks);
        string AddBaitAction();
        string ListPlayer(Player player, bool hasEarlyPicks);
        bool CheckForFirstRounders(string offeringIds);
        bool CheckForMultiplePlayers(string idArray);
    }
    public class RumorService : IRumorService
    {
        Random rnd = new Random();
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
                if (pickDetails[0].ToUpper() == "FP" && (pickDetails[pickDetails.Length - 1] == "1" ||
                                                         pickDetails[pickDetails.Length - 1] == "2")) return true;
            }
            return false;
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