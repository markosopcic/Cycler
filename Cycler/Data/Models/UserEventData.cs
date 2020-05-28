using System;
using System.Collections.Generic;
using MongoDB.Bson;

namespace Cycler.Data.Models
{
    public class UserEventData
    {
        public ObjectId UserId { get; set; }
        public TimeSpan Duration { get; set; }
        public long Meters { get; set; }
        public List<Location> Locations { get; set; }
    }
}