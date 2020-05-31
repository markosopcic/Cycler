using System;
using System.Collections.Generic;
using Cycler.Data.Models;

namespace Cycler.Views.Models
{
    public class MobileEventViewModel
    {
                
        public string Id { get; set; }
        public bool Private { get; set; }
        
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public int Invited { get; set; }
        
        public int Accepted { get; set; }
        public string OwnerName { get; set; }
        
        public string StartTime { get; set; }
        public string? EndTime { get; set; }
        
        public bool Finished { get; set; }
        
        public string OwnerId { get; set; }
        
        public List<EventUserModel> UserIds { get; set; }
        
        public List<MobileUserEventData> UserEventData { get; set; }

    }

    public class MobileUserEventData
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public long DurationSeconds { get; set; }
        public long Meters { get; set; }
    }
}