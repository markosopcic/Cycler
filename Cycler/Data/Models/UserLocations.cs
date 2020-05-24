using System.Collections.Generic;
using MongoDB.Bson;

namespace Cycler.Data.Models
{
    public class UserLocations
    {
        public ObjectId UserId { get; set; }
        public List<Location> Locations { get; set; }
    }
}