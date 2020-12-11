using System.Collections.Generic;

namespace CapnCrunchGMBot.Models
{
    public class GroupModel
    {
        public string id { get; set; }
        public string name { get; set; }
        public List<MemberModel> members { get; set; }
    }
}