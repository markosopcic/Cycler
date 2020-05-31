using System;
using Cycler.Data.Models;
using MongoDB.Bson;

namespace Cycler.Views.Models
{
    public class InvitationViewModel
    {   
        public string EventId { get; set; }
        public string InvitationId { get; set; }
        public string EventName { get; set; }
        public string EventDescription { get; set; }
        public string EventStartTime { get; set; }
        public User InvitedBy { get; set; }
        public bool CanInvite { get; set; }
        public bool Accepted { get; set; }
        public string InvitationTime { get; set; }
    }
}