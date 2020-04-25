﻿using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Cycler.Data.Models
{
    public class Event
    {
        [BsonId]
        public ObjectId Id { get; set; }
        
        public ObjectId OwnerId { get; set; }
        
        public bool Private { get; set; }
        
        public IEnumerable<ObjectId> AcceptedUsers { get; set; }
        
        public IEnumerable<ObjectId> InvitedUsers { get; set; }
        [BsonDateTimeOptions]
        public DateTime StartTime { get; set; }
        [BsonDateTimeOptions]
        public DateTime EndTime { get; set; }
        
        public bool Finished { get; set; }
        
        public Dictionary<ObjectId,IEnumerable<Location>> Coordinates { get; set; }
    }
}