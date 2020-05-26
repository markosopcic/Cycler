﻿namespace Cycler.Controllers.Models
{
    public class LocationModel
    {
        public string Id { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        
        public bool UpdateOnlineStatus { get; set; }
    }
}