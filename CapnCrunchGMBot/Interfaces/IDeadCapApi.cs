using System.Collections.Generic;
using System.Threading.Tasks;
using CapnCrunchGMBot.Models;
using RestEase;

namespace CapnCrunchGMBot.Interfaces
{
    public interface IDeadCapApi
    {
        [Get("mfl/standings/{year}")]
        Task<List<TeamStandings>> GetMflStandings([Path] int year);
        
        //get pending trades
        [Get("mfl/pendingTrades/{year}")]
        Task<List<PendingTrade>> GetMflPendingTrades([Path] int year);
    }
}