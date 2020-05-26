using System;
using System.Collections.Generic;
using System.Linq;
using Cycler.Data.Models;
using Cycler.Data.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Cycler.Data.Repositories
{
    public class FriendshipRepository:IFriendshipRepository
    {

        private MongoContext context;
        
        public FriendshipRepository(MongoContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public FriendshipRequest FindFriendshipRequest(ObjectId firstUser, ObjectId secondUser)
        {
            if(firstUser == null) throw new ArgumentNullException(nameof(firstUser));
            if(secondUser == null) throw new ArgumentNullException(nameof(firstUser));

            return context.FriendshipRequest.Find(e => (e.Sender == firstUser && e.Receiver == secondUser) || (e.Sender == secondUser && e.Receiver == firstUser)).FirstOrDefault();
        }

        public IEnumerable<FriendshipRequest> GetUserRequests(ObjectId userId)
        {
            if(userId == null) throw new ArgumentNullException(nameof(userId));

            return context.FriendshipRequest.Find(e => e.Receiver == userId && !e.Accepted).SortByDescending(f => f.TimeSent).ToEnumerable();
        }

        public bool SendFriendRequest(ObjectId fromUser, ObjectId toUser)
        {
            var request = new FriendshipRequest {Receiver = toUser, Sender = fromUser, TimeSent = DateTime.UtcNow};
            var receiver = context.User.Find(e => e.Id == toUser);
            var existingFriend =
                context.User.Find(e => e.Id == fromUser && e.Friends.Any(f => f == toUser)).FirstOrDefault() != null 
                && context.User.Find(e => e.Id == toUser && e.Friends.Any(f => f == fromUser)).FirstOrDefault() != null;

            var existingRequest = context.FriendshipRequest.Find(e =>
                (e.Sender == fromUser && e.Receiver == toUser) ||
                (e.Sender == toUser && e.Receiver == fromUser)).FirstOrDefault() != null;
            if (existingFriend || existingRequest)
            {
                return false;
            }
            context.FriendshipRequest.InsertOne(new FriendshipRequest
            {
                Receiver = toUser,
                Sender = fromUser,
                TimeSent = DateTime.UtcNow
            });

            return true;
            
        }


        
        public void AcceptFriendshipRequest(bool accept, ObjectId user,ObjectId from)
        {
            var exists = context.FriendshipRequest.Find(e => e.Sender == from && e.Receiver == user && !e.Accepted)
                .FirstOrDefault() != null;
            if (!exists) return;
            if (accept)
            {
                context.User.UpdateOne(e => e.Id == user,
                    Builders<User>.Update.AddToSet(p => p.Friends,from));
                context.User.UpdateOne(e => e.Id == from,
                    Builders<User>.Update.AddToSet(p => p.Friends,user));
                context.FriendshipRequest.FindOneAndUpdate(e => e.Sender == from && e.Receiver == user,
                    Builders<FriendshipRequest>.Update.Set(e => e.Accepted, true)
                        .Set(e => e.TimeAccepted, DateTime.UtcNow));
            }
            else
            {
                context.FriendshipRequest.FindOneAndDelete(e => e.Sender == from && e.Receiver == user);
            }
            
        }
    }
}