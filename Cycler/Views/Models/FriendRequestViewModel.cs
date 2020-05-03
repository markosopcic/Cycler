using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Cycler.Views.Models
{
    public class FriendRequestViewModel
    {
        public string Id { get; set; }
        public string Sender { get; set; }
        public DateTime TimeSent { get; set; }
        public string SenderName { get; set; }
    }
}