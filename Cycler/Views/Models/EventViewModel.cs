using System;
using System.Collections.Generic;
using Cycler.Data.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Cycler.Views.Models
{
    public class EventViewModel
    {
        
        public string Id { get; set; }
        public bool Private { get; set; }
        
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public int Invited { get; set; }
        
        public int Accepted { get; set; }
        
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        
        public bool Finished { get; set; }
        
        public string OwnerId { get; set; }
        
        public List<EventUserModel> UserIds { get; set; }
        
        public List<UserEventData> UserEventData { get; set; }
        
    }
}