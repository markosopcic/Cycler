﻿using System;
 using MongoDB.Bson.Serialization.Attributes;

 namespace Cycler.Data.Models
{
    public class Location
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        [BsonDateTimeOptions]
        public DateTime Time { get; set; }
    }
}