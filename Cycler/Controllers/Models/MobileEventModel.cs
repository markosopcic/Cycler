using System.Collections.Generic;

namespace Cycler.Controllers.Models
{
    public class MobileEventModel
    {
        
        public long Meters { get; set; }
        public long Duration { get; set; }
        public string UserId { get; set; }
        public string EventId { get; set; }
        public List<Location> Locations { get; set; }
        public long StarTimeMillis { get; set; }
        public string Name { get; set; }
        public string OwnerId { get; set; }
    }

    public class Location
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public long TimeMillis { get; set; }
    }
}