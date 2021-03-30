using System.Net.Http;
using System.Threading.Tasks;
using CapnCrunchGMBot.Models;
using Microsoft.AspNetCore.Mvc;
using RestEase;

namespace CapnCrunchGMBot.Interfaces
{
    public interface IMflApi
    {
        [Get("export?TYPE=tradeBait&L=13894&APIKEY=ahBv1sGSvuWux1OmOVrJaDYeFLox&INCLUDE_DRAFT_PICKS=true&JSON=1")]
        Task<HttpResponseMessage> GetTradeBait();

        [Get("export?TYPE=players&L=13894&APIKEY=ahBv1sGSvuWux1OmOVrJaDYeFLox&DETAILS=&SINCE=&PLAYERS={players}&JSON=1")]
        Task<PlayersParent> GetPlayersDetails([Path] string players);
        
        [Get("export?TYPE=players&L=13894&APIKEY=ahBv1sGSvuWux1OmOVrJaDYeFLox&DETAILS=&SINCE=&PLAYERS={player}&JSON=1")]
        Task<PlayerParent> GetPlayerDetails([Path] string player);
        
        [Get("export?TYPE=transactions&L=13894&APIKEY=ahBv1sGSvuWux1OmOVrJaDYeFLox&W=&TRANS_TYPE=TRADE&FRANCHISE=&DAYS=1&COUNT=&JSON=1")]
        Task<HttpResponseMessage> GetRecentTrade();

        [Get("export?TYPE=salaries&L=13894&APIKEY=ahBv1sGSvuWvx1GmOVrJaDYeFLox&JSON=1")]
        Task<SalariesRoot> GetPlayerSalaries();
    }
}