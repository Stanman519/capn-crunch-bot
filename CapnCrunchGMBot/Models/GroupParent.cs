using System.Reflection.Metadata.Ecma335;

namespace CapnCrunchGMBot.Models
{
    public class GroupParent
    {
        public GroupModel response { get; set; }
        public GroupMetaData Meta { get; set; }
    }

    public class GroupMetaData
    {
        public int code { get; set; }
    }
}