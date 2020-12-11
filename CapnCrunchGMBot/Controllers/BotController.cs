using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace CapnCrunchGMBot.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BotController : ControllerBase
    {
        private  IGroupMeService _groupMeService;

        public BotController(IGroupMeService groupMeService)
        {
            _groupMeService = groupMeService;
        }

        [HttpGet("standings/{year}")]
        public async Task<List<TeamStandings>> PostStandings(int year)
        {
           return await _groupMeService.PostStandingsToGroup(year);
        }

        [HttpGet("pendingTrades/{year}")]
        public async Task<List<PendingTrade>> PostTradeOffers(int year)
        {
            return await _groupMeService.PostTradeOffersToGroup(year);
        }
    }
}
