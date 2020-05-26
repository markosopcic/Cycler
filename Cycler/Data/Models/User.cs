﻿using System;
 using System.Collections.Generic;
 using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Cycler.Data.Models
{
    public class User
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string FirstName { get; set; }
        
        [BsonDateTimeOptions]
        public DateTime? LastActiveTrace { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public List<ObjectId> Friends { get; set; } = new List<ObjectId>();
        public string Email { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] Salt { get; set; }
        [BsonDateTimeOptions]
        public DateTime DateJoined { get; set; }
        [BsonDateTimeOptions]
        public DateTime LastLogin { get; set; }
    }
}