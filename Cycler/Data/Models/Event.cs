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
        
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public List<ObjectId> AcceptedUsers { get; set; } = new List<ObjectId>();

        [BsonDateTimeOptions]
        public DateTime StartTime { get; set; }
        [BsonDateTimeOptions]
        public DateTime? EndTime { get; set; }
        
        public bool Finished { get; set; }
        
        public List<UserLocations> UserLocations { get; } = new List<UserLocations>();
    }
}