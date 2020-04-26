using System;
using System.Collections.Generic;

namespace Cycler.Controllers.Models
{
    public class EventModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsPrivate { get; set; }
        public DateTime StartTime { get; set; }
        public IEnumerable<string> InvitedUserIDs { get; set; }
    }
}