using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Cycler.Data.Models
{
    public class FriendshipRequest
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public ObjectId Sender { get; set; }
        public ObjectId Receiver { get; set; }
        [BsonDateTimeOptions]
        public DateTime TimeSent { get; set; }
        
        [BsonDateTimeOptions]
        public DateTime? TimeAccepted { get; set; }
        
        public bool Accepted { get; set; }
        
    }
}