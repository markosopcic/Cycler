using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Cycler.Views.Models
{
    public class UserViewModel
    {
        
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime DateJoined { get; set; }
        public DateTime LastLogin { get; set; }
        public int NumOfFriends { get; set; }
        
        public bool FriendshipRequestSent { get; set; }
        
        public string FriendshipRequestId { get; set; }
        
        public bool FriendshipRequestReceived { get; set; }
        
        public bool isFriend { get; set; }
    }
}