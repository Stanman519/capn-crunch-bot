using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using CapnCrunchGMBot.Interfaces;
using CapnCrunchGMBot.Models;
using Newtonsoft.Json;

namespace CapnCrunchGMBot
{
    public interface IGroupMeService
    {
        public Task<List<TeamStandings>> PostStandingsToGroup(int year);
        public Task<List<PendingTrade>> PostTradeOffersToGroup(int year);
    }
    
    public class GroupMeService : IGroupMeService
    {
        private IDeadCapApi _myApi;
        private IGroupMeApi _gmApi;
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
        
        public GroupMeService(IDeadCapApi api, IGroupMeApi gmApi)
        {
            _myApi = api;
            _gmApi = gmApi;
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