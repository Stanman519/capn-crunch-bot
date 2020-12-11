using System.Collections.Generic;
using System.Threading.Tasks;
using CapnCrunchGMBot.Models;
using RestEase;

namespace CapnCrunchGMBot
{
    public interface IGroupMeApi
    {
       [Post("v3/bots/post")]
        Task SendMessage([Body]Message message);
        
        [Get("v3/groups/59795205?token=TNkXkjDPQ7jRs8r0hZKpfMRBaqWXk6AOyuKywGIE")]
        Task<GroupParent> GetMemberIds();
    }
}