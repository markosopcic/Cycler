using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Cycler.Data.Models
{
    public class Invitation
    {
        [BsonId]
        public ObjectId InvitationId { get; set; }
        public ObjectId InviterId { get; set; }
        public ObjectId InvitedId { get; set; }
        public bool CanInvite { get; set; }
        public bool Accepted { get; set; }
        public DateTime? AcceptedTime { get; set; }
        public DateTime InvitationTime { get; set; }
    }
}