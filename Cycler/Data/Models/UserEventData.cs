using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Cycler.Data.Models
{
    public class UserEventData
    {
        [BsonId]
        public ObjectId UserId { get; set; }
        public TimeSpan Duration { get; set; }
        public long Meters { get; set; }
        public List<Location> Locations { get; set; }
    }
}