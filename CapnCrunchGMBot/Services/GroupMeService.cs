using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using CapnCrunchGMBot.Interfaces;
using CapnCrunchGMBot.Models;
using Newtonsoft.Json;
using RestEase;

namespace CapnCrunchGMBot
{
    public interface IGroupMeService
    {
        public Task<List<TeamStandings>> PostStandingsToGroup(int year);
        public Task<List<PendingTrade>> PostTradeOffersToGroup(int year);
        public Task PostTradeRumor();
    }
    
    public class GroupMeService : IGroupMeService
    {
        private IDeadCapApi _myApi;
        private IGroupMeApi _gmApi;
        private readonly IMflApi _mfl;
        private readonly IRumorService _rumor;
        private readonly IHttpClientFactory _clientFactory;
        
        private Dictionary<int, string> owners = new Dictionary<int, string>()
        {
            {1, "Ryan"},
            {2, "Tyler W"},
            {3, "Caleb"},
            {4, "Trent"},
            {5, "Taylor"},
            {6, "Logan"},
            {7, "Cory"},
            {8, "Jeremi"},
            {9, "Levi"},
            {10, "Aaron"},
            {11, "Juan"},
            {12, "Tyler S"}
        };
        
        private Dictionary<int, string> memberIds = new Dictionary<int, string>()
        {
            {1, "8206212"},
            {2, "36741"},
            {3, "8206213"},
            {4, "2513723"},
            {5, "482066"},
            {6, "34951757"},
            {7, "51268339"},
            {8, "36739"},
            {9, "30472260"},
            {10, "11902182"},
            {11, "36740"},
            {12, "2513725"}
        };
        
        public GroupMeService(IDeadCapApi api, IGroupMeApi gmApi, IMflApi mfl, IRumorService rumor)
        {
            _myApi = api;
            _gmApi = gmApi;
            _mfl = mfl;
            _rumor = rumor;
        }

        public async Task<List<TeamStandings>> PostStandingsToGroup(int year)
        {
            var standings = await _myApi.GetMflStandings(year);
            string strForBot = "STANDINGS \n";
            standings.ForEach(s =>
            {
                strForBot = $"{strForBot}{owners[s.FranchiseId]}   {s.H2hWins2}-{s.H2hLosses2}   {s.PointsFor2} \n";
            });

            await BotPost(strForBot);
            return standings;
        }

        public async Task<List<PendingTrade>> PostTradeOffersToGroup(int year)
        {
            var trades = await _myApi.GetMflPendingTrades(year);
            var group = await _gmApi.GetMemberIds();
            var memberList = group.response.members;
            
            string strForBot = "";
           
            if (trades.Count > 0)
            {
                trades.ForEach(async t =>
                {
                    // get member id, then lookup their name;
                    var tagName = memberList.Find(m => m.user_id == memberIds[t.offeredTo]);
                    var tagString = $"@{tagName.nickname}";
                    strForBot = ", you have a pending trade offer!";
                    await BotPostWithTag(strForBot, tagString, tagName.user_id);
                });
            }
            return trades;
        }

        public async Task PostTradeRumor()
        {
            var deserializer = new JsonResponseDeserializer();
            var info = new ResponseDeserializerInfo();
            var res = await _mfl.GetTradeBait();
            var group = await _gmApi.GetMemberIds();
            var memberList = group.response.members;
            string strForBot = "";
            var jsonString = await res.Content.ReadAsStringAsync();
            
            try
            {
                var tradeBait = deserializer.Deserialize<TradeBaitParent>(jsonString, res, info).tradeBaits.tradeBait;
                strForBot += _rumor.GetSources();
                var ownerName = "";
                owners.TryGetValue(Int32.Parse(tradeBait.franchise_id), out ownerName);
                strForBot += ownerName + " ";
                //check if this is a new post or not.
                var postDate = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(tradeBait.timestamp));
                if (postDate < DateTime.Now.AddDays(-1)) return;
                // add verbage
                strForBot += _rumor.AddBaitAction();
                var hasEarlyPicks = _rumor.CheckForFirstRounders(tradeBait.willGiveUp);
                var multiplePlayers = _rumor.CheckForMultiplePlayers(tradeBait.willGiveUp);
                if (multiplePlayers)
                {
                    var parent = await _mfl.GetPlayersDetails(tradeBait.willGiveUp);
                    var players = parent.players.player;
                    strForBot += _rumor.ListPlayers(players, hasEarlyPicks);
                }
                else
                {
                    var parent = await _mfl.GetPlayerDetails(tradeBait.willGiveUp);
                    var player = parent.players.player;
                    strForBot += _rumor.ListPlayer(player, hasEarlyPicks);
                }
                //await BotPost(strForBot);
                return;
            }
            catch (Exception e) {Console.WriteLine("not a single trade");}
            try
            {
                var tradeBaits = deserializer.Deserialize<TradeBaitsParent>(jsonString, res, info).tradeBaits.tradeBait;
                var ownerName = "";
                //go through each post in list - 
                foreach (var post in tradeBaits)
                {
                    var postDate = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(post.timestamp));
                    if (postDate < DateTime.Now.AddDays(-1))
                    {
                        strForBot += _rumor.GetSources();
                        owners.TryGetValue(Int32.Parse(post.franchise_id), out ownerName);
                        strForBot += ownerName + " ";
                        strForBot += _rumor.AddBaitAction();  // add verbage
                        var hasEarlyPicks = _rumor.CheckForFirstRounders(post.willGiveUp);
                        var multiplePlayers = _rumor.CheckForMultiplePlayers(post.willGiveUp);
                        if (multiplePlayers)
                        {
                            var parent = await _mfl.GetPlayersDetails(post.willGiveUp);
                            var players = parent.players.player;
                            strForBot += _rumor.ListPlayers(players, hasEarlyPicks);
                        }
                        else
                        {
                            var parent = await _mfl.GetPlayerDetails(post.willGiveUp); 
                            var player = parent.players.player;
                            strForBot += _rumor.ListPlayer(player, hasEarlyPicks);
                        }
                        await BotPost(strForBot);
                    }
                }
            }
            catch (Exception e) { Console.WriteLine("not a multi trade");}
        }

        public async Task BotPost(string text)
        {
            var message = new Message(text);
            await _gmApi.SendMessage(message);
        }

        public async Task BotPostWithTag(string text, string nickname, string memberId)
        {
            var rawText = $"{nickname}{text}";
            var message = new Message(rawText);
            var mention = new Mention();
            mention.type = "mentions";
            int[][] locis = new int[1][] {new[] {0, nickname.Length}};
            string[] mentionIds = new string[] {memberId};
            mention.loci = locis;
            mention.user_ids = mentionIds;
            var mentionList = new List<Mention>();
            mentionList.Add(mention);
            message.attachments = mentionList;
            await _gmApi.SendMessage(message);
        }
    }
    
    
    
}