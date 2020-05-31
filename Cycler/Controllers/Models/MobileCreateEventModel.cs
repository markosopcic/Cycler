using System.Collections.Generic;

namespace Cycler.Controllers.Models
{
    public class MobileCreateEventModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public long StartTimeMillis { get; set; }
        public List<string> FriendIdsToInvite { get; set; }
    }
}