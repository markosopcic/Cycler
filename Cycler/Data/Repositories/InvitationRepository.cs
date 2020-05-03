using System;
using System.Collections.Generic;
using Cycler.Data.Models;
using Cycler.Data.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Cycler.Data.Repositories
{
    public class InvitationRepository:IInvitationRepository
    {
        private MongoContext context;
        
        public InvitationRepository(MongoContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }
        
        public bool AcceptInvitation(ObjectId invitationId,bool accept)
        {
            if(invitationId == null) throw new ArgumentNullException(nameof(invitationId));
            if (!accept)
            {
                context.Invitation.FindOneAndDelete(e => e.InvitationId == invitationId);
                return true;
            }
            var invitation = context.Invitation.FindOneAndUpdate(e => e.InvitationId == invitationId && !e.Accepted,
                Builders<Invitation>.Update
                    .Set(e => e.Accepted, true)
                    .Set(e => e.AcceptedTime, DateTime.Now));

            if (invitation == null) return false;
            var evt = context.Event.FindOneAndUpdate(e => e.Id == invitation.EventId,
                Builders<Event>.Update.AddToSet(f => f.AcceptedUsers, invitation.InvitedId));

            return true;
        }

        public bool InviteUserToEvent(ObjectId invitedId, ObjectId inviterId, ObjectId eventId,bool canInvite)
        {
            if (invitedId == null)
            {
                throw new ArgumentNullException(nameof(invitedId));
            }
            if (inviterId == null)
            {
                throw new ArgumentNullException(nameof(inviterId));
            }
            
            if (eventId == null)
            {
                throw new ArgumentNullException(nameof(eventId));
            }

            var isOwner = context.Event.Find(e => e.Id == eventId && e.OwnerId == inviterId).FirstOrDefault() != null;
            var canInviteToEvent = context.Invitation.Find(e => e.InvitedId == invitedId && e.Accepted && e.CanInvite)
                .FirstOrDefault() != null;

            if (isOwner || canInviteToEvent)
            {
                var i = new Invitation
                {
                    EventId = eventId,
                    Accepted = false, AcceptedTime = null,InvitationTime = DateTime.Now, 
                    CanInvite = canInvite, 
                    InvitedId = invitedId,
                    InviterId = inviterId
                };

                context.Invitation.InsertOne(i);
                return true;
            }

            return false;
        }

        public int CountInvited(ObjectId eventId)
        {
            if (eventId == null)
            {
                throw new ArgumentNullException(nameof(eventId));
            }

            return (int)context.Invitation.Find(e => e.EventId == eventId).CountDocuments();

        }

        public int CountAccepted(ObjectId eventId)
        {
            if (eventId == null)
            {
                throw new ArgumentNullException(nameof(eventId));
            }
            return (int)context.Invitation.Find(e => e.EventId == eventId && e.Accepted).CountDocuments();
        }

        public Invitation GetByUserEvent(ObjectId userId, ObjectId eventId)
        {
            if(userId  == null) throw new ArgumentNullException(nameof(userId));
            if(eventId == null) throw new ArgumentNullException(nameof(eventId));

            return context.Invitation.Find(e => e.EventId == eventId && e.InvitedId == userId).FirstOrDefault();
        }
        
        public IEnumerable<Event> GetUserEventInvitations(ObjectId user)
        {
            if(user == null) throw new ArgumentNullException(nameof(user));

            var events =  context.Invitation.Find(e => e.InvitedId == user && !e.Accepted).Project(e => e.EventId).ToList();
            
            return context.Event.Find(e => !e.Finished && events.Contains(e.Id))
                .Project(f => new Event
                {
                    StartTime = f.StartTime,
                    Id = f.Id,Name = f.Name,
                    Description = f.Description,
                    OwnerId = f.OwnerId
                }).ToList();
        }
    }
}