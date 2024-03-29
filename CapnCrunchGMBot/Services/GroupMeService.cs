using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using CapnCrunchGMBot.Interfaces;
using CapnCrunchGMBot.Models;
using CapnCrunchGMBot.Services;
using Newtonsoft.Json;
using RestEase;

namespace CapnCrunchGMBot
{
    public interface IGroupMeService
    {
        public Task<List<TeamStandings>> PostStandingsToGroup(int year);
        public Task<List<PendingTrade>> PostTradeOffersToGroup(int year);
        public Task PostTradeRumor();
        public Task PostCompletedTradeToGroup();
        Task BotPost(string text);
        public Task<List<TYTScore>> PostTYTTop5(int year);
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
            var standings = (await _myApi.GetMflStandings(year))
                .OrderByDescending(_ => _.VictoryPoints2)
                .ThenByDescending(_ => _.H2hWins2)
                .ThenByDescending(_ => _.PointsFor2)
                .ToList();
            var strForBot = "STANDINGS \n";
            standings.ForEach(s =>
            {
                strForBot = $"{strForBot}{owners[s.FranchiseId]}  ({s.VictoryPoints2} VP)  {s.H2hWins2}-{s.H2hLosses2}    {s.PointsFor2} pts\n";
            });
            await BotPost(strForBot);
            return standings;
        }

        public int GetAdjustedWins(int h2hWins, int vp)
        {
            return h2hWins + vp - (h2hWins * 2);
        }
        
        
        public async Task<List<TYTScore>> PostTYTTop5(int year)
        {
            var standings = await _myApi.GetMflStandings(year);
            var strForBot = "Tri-Year Trophy Presented by Taco Bell\nTOP 5\n";
            var tytScores = standings.Select(t => new TYTScore
            {
                Owner = owners[t.FranchiseId],
                Score = (t.H2hWins1 * 5 + t.PointsFor1) + ((GetAdjustedWins(t.H2hWins2, t.VictoryPoints2) * 5) + t.PointsFor2)
                                                        + ((GetAdjustedWins(t.H2hWins3, t.VictoryPoints3) * 5) + t.PointsFor3)
            }).OrderByDescending(t => t.Score)
                .Take(5)
                .ToList();

            tytScores.ForEach(s =>
            {
                strForBot = $"{strForBot}{s.Owner} - {s.Score}\n";
            });
            await BotPost(strForBot);
            return tytScores;
        }

        public async Task<List<PendingTrade>> PostTradeOffersToGroup(int year)
        {
            var tenMinDuration = new TimeSpan(0, 0, 10, 0);
            var trades = await _myApi.GetMflPendingTrades(year);
            var group = await _gmApi.GetMemberIds();
            var memberList = group.response.members;
            
            string strForBot = "";
           
            if (trades.Count > 0)
            {
                trades.ForEach(async t =>
                {
                    var timeDifference = t.timeStamp.TimeOfDay - DateTime.Now.AddMinutes(-11).TimeOfDay;
                    if (timeDifference.Ticks > 0 && timeDifference < tenMinDuration)
                    {
                        // get member id, then lookup their name;
                        var tagName = memberList.Find(m => m.user_id == memberIds[t.offeredTo]);
                        var tagString = $"@{tagName.nickname}";
                        strForBot = ", you have a pending trade offer!";
                        await BotPostWithTag(strForBot, tagString, tagName.user_id);
                    }
                });
            }
            return trades;
        }

        public async Task PostCompletedTradeToGroup()
        {
            var deserializer = new JsonResponseDeserializer();
            var info = new ResponseDeserializerInfo();
            var tradeRes = await _mfl.GetRecentTrade();
            var strForBot = "";
            var jsonString = await tradeRes.Content.ReadAsStringAsync();
            var owner1 = "";
            var owner2 = "";
            var assets1 = "";
            var assets2 = "";

            try
            {
                //Single
                var tradeSingle = deserializer.Deserialize<TradeTransactionSingle>(jsonString, tradeRes, info)
                    .transactions.transaction;
                DateTime tenMinAgo = DateTime.Now.AddMinutes(-11);
                var tradeTime = DateTimeOffset.FromUnixTimeSeconds(Int64.Parse(tradeSingle.timestamp));
                // check if trade was not in the last 10 minutes to bail early
                if (tradeTime < tenMinAgo)
                {
                    return;
                }
                owners.TryGetValue(Int32.Parse(tradeSingle.franchise), out owner1);
                owners.TryGetValue(Int32.Parse(tradeSingle.franchise2), out owner2);
                strForBot += $"{_rumor.GetSources()}{owner1} and {owner2} have completed a trade. \n";
                
                var multiplePlayers1 = _rumor.CheckForMultiplePlayers(tradeSingle.franchise1_gave_up);
                var multiplePlayers2 = _rumor.CheckForMultiplePlayers(tradeSingle.franchise2_gave_up);
                assets1 = multiplePlayers1 ? await _rumor.ListTradeInfoWithMultiplePlayers(tradeSingle.franchise1_gave_up) : await _rumor.ListTradeInfoWithSinglePlayer(tradeSingle.franchise1_gave_up);
                assets2 = multiplePlayers2 ? await _rumor.ListTradeInfoWithMultiplePlayers(tradeSingle.franchise2_gave_up) : await _rumor.ListTradeInfoWithSinglePlayer(tradeSingle.franchise2_gave_up);
                
                strForBot += $"{owner1} sends: \n{assets1} \n{owner2} sends: \n{assets2}";
                
                await BotPost(strForBot);
                return;
            }
            catch (Exception e) {Console.WriteLine("not a single trade");}

            try
            {
                //Multiple
                var multiTrade = deserializer.Deserialize<TradeTransactionMulti>(jsonString, tradeRes, info)
                    .transactions.transaction;
                var tenMinAgo = DateTime.Now.AddMinutes(-11);
                foreach (var trade in multiTrade)
                {
                    var tradeTime = DateTimeOffset.FromUnixTimeSeconds(Int64.Parse(trade.timestamp));
                    // check if trade was not in the last 10 minutes to bail early
                    if (tradeTime >= tenMinAgo)
                    {
                        owners.TryGetValue(Int32.Parse(trade.franchise), out owner1);
                        owners.TryGetValue(Int32.Parse(trade.franchise2), out owner2);
                        strForBot += $"{_rumor.GetSources()}{owner1} and {owner2} have completed a trade. \n";
                
                        var multiplePlayers1 = _rumor.CheckForMultiplePlayers(trade.franchise1_gave_up);
                        var multiplePlayers2 = _rumor.CheckForMultiplePlayers(trade.franchise2_gave_up);
                        assets1 = multiplePlayers1 ? await _rumor.ListTradeInfoWithMultiplePlayers(trade.franchise1_gave_up) : await _rumor.ListTradeInfoWithSinglePlayer(trade.franchise1_gave_up);
                        assets2 = multiplePlayers2 ? await _rumor.ListTradeInfoWithMultiplePlayers(trade.franchise2_gave_up) : await _rumor.ListTradeInfoWithSinglePlayer(trade.franchise2_gave_up);
                
                        strForBot += $"{owner1} sends: \n{assets1} \n{owner2} sends: \n{assets2}";
                    
                        await BotPost(strForBot);
                    }
                }
            }
            catch (Exception e) {Console.WriteLine("not a multi trade");}
        }

        public async Task PostTradeRumor()
        {
            var deserializer = new JsonResponseDeserializer();
            var info = new ResponseDeserializerInfo();
            var res = await _mfl.GetTradeBait();
            var strForBot = "";
            var jsonString = await res.Content.ReadAsStringAsync();
            
            try
            {
                var tradeBait = deserializer.Deserialize<TradeBaitParent>(jsonString, res, info).tradeBaits.tradeBait;
                strForBot += _rumor.GetSources();
                owners.TryGetValue(Int32.Parse(tradeBait.franchise_id), out var ownerName);
                strForBot += $"{ownerName} ";
                // check if this is a new post or not.
                var postDate = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(tradeBait.timestamp));
                if (postDate < DateTime.Now.AddMinutes(-11)) return;
                // add verbiage
                strForBot += _rumor.AddBaitAction();
                var hasEarlyPicks = _rumor.CheckForFirstRounders(tradeBait.willGiveUp);
                var multiplePlayers = _rumor.CheckForMultiplePlayers(tradeBait.willGiveUp);
                if (multiplePlayers)
                {
                    var players = (await _mfl.GetPlayersDetails(tradeBait.willGiveUp)).players.player;
                    strForBot += _rumor.ListPlayers(players, hasEarlyPicks);
                }
                else
                {
                    var player = (await _mfl.GetPlayerDetails(tradeBait.willGiveUp)).players.player;
                    strForBot += _rumor.ListPlayer(player, hasEarlyPicks);
                }
                await BotPost(strForBot);
                return;
            }
            catch (Exception e) {Console.WriteLine("not a single trade");}
            try
            {
                var tradeBaits = deserializer.Deserialize<TradeBaitsParent>(jsonString, res, info).tradeBaits.tradeBait;
                foreach (var post in tradeBaits)
                {
                    var postDate = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(post.timestamp));
                    if (postDate > DateTime.Now.AddDays(-1))
                    {
                        strForBot += _rumor.GetSources();
                        owners.TryGetValue(Int32.Parse(post.franchise_id), out var ownerName);
                        strForBot += $"{ownerName} ";
                        strForBot += _rumor.AddBaitAction();  // add verbage
                        var hasEarlyPicks = _rumor.CheckForFirstRounders(post.willGiveUp);
                        if (_rumor.CheckForMultiplePlayers(post.willGiveUp))
                        {
                            var players = (await _mfl.GetPlayersDetails(post.willGiveUp)).players.player;
                            strForBot += _rumor.ListPlayers(players, hasEarlyPicks);
                        }
                        else
                        {
                            var player = (await _mfl.GetPlayerDetails(post.willGiveUp)).players.player;
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
            var mention = new Mention {type = "mentions"};
            int[][] locis = new int[1][] {new[] {0, nickname.Length}};
            var mentionIds = new[] {memberId};
            mention.loci = locis;
            mention.user_ids = mentionIds;
            var mentionList = new List<Mention> {mention};
            message.attachments = mentionList;
            await _gmApi.SendMessage(message);
        }
    }
    
    
    
}