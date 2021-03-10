using System.Threading.Tasks;
using CapnCrunchGMBot.Models;
using RestEase;

namespace CapnCrunchGMBot.Interfaces
{
    public interface IGroupMeApi
    {
        [Post("v3/bots/post")]
        Task SendMessage([Body]Message message);
        
        [Get("v3/groups/59795205?token=YjMEBw8kwXkJMKxDz2nd2o0iG9aC1GG4NjD9O1ih")]
        Task<GroupParent> GetMemberIds();
        
        //TODO: trade rumors
    }
}