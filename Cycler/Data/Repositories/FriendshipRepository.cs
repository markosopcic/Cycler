﻿using System;
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

        public bool FriendshipRequestSent(ObjectId from, ObjectId to)
        {
            if(from == null) throw new ArgumentNullException(nameof(from));
            if(to == null) throw new ArgumentNullException(nameof(from));

            return context.FriendshipRequest.Find(e => e.Sender == from && e.Receiver == to).FirstOrDefault() != null;
        }
        
        public bool SendFriendRequest(ObjectId fromUser, ObjectId toUser)
        {
            var request = new FriendshipRequest {Receiver = toUser, Sender = fromUser, TimeSent = DateTime.Now};
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
                TimeSent = DateTime.Now
            });

            return true;
            
        }


        
        public void AcceptFriendshipRequest(bool accept, ObjectId user,ObjectId from)
        {
            var exists = context.FriendshipRequest.Find(e => e.Sender == from && e.Receiver == user && !e.Accepted)
                .First() != null;
            if (!exists) return;
            if (accept)
            {
                context.User.UpdateOne(e => e.Id == user,
                    Builders<User>.Update.AddToSet(p => p.Friends,from));
                context.User.UpdateOne(e => e.Id == from,
                    Builders<User>.Update.AddToSet(p => p.Friends,user));
                context.FriendshipRequest.FindOneAndUpdate(e => e.Sender == from && e.Receiver == user,
                    Builders<FriendshipRequest>.Update.Set(e => e.Accepted, true)
                        .Set(e => e.TimeAccepted, DateTime.Now));
            }
            else
            {
                context.FriendshipRequest.FindOneAndDelete(e => e.Sender == from && e.Receiver == user);
            }
            
        }
    }
}