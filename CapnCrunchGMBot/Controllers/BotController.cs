using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CapnCrunchGMBot.Models;
using Microsoft.AspNetCore.Mvc;

namespace CapnCrunchGMBot.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BotController : ControllerBase
    {
        private IGroupMeService _groupMeService;

        public BotController(IGroupMeService groupMeService)
        {
            _groupMeService = groupMeService;
        }

        [HttpGet("standings/{year}")]
        public async Task<List<TeamStandings>> PostStandings(int year)
        {
            try
            {
                return await _groupMeService.PostStandingsToGroup(year);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                throw;
            }
        }

        [HttpGet("pendingTrades/{year}")]
        public async Task<List<PendingTrade>> PostTradeOffers(int year)
        {
            return await _groupMeService.PostTradeOffersToGroup(year);
        }

        [HttpGet("tradeBait")]
        public async Task PostTradeRumor()
        {
            await _groupMeService.PostTradeRumor();
        }

        [HttpGet("completedTrades/{year}")]
        public async Task PostCompletedTrades(int year)
        {
            await _groupMeService.PostCompletedTradeToGroup();
        }
        
        [HttpPost("auctionError")]
        public async Task PostCompletedTrades([FromBody] string message)
        {
            await _groupMeService.BotPost(message);
        }
    }
}
