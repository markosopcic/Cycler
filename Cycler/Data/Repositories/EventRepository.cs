using System;
using System.Collections.Generic;
using System.Linq;
using Cycler.Data.Models;
using Cycler.Data.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Cycler.Data.Repositories
{
    public class EventRepository:IEventRepository
    {

        private MongoContext context { get; }
        
        public EventRepository(MongoContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }
        
        public Event AddEvent(Event e)
        { 
            context.Event.InsertOne(e);
            return e;
        }

        public bool AcceptInvitation(ObjectId userId, ObjectId eventId)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (eventId == null)
            {
                throw new ArgumentNullException(nameof(eventId));
            }

            var e = context.Event.Find(e => e.Id == eventId)
                .Project<Event>(Builders<Event>.Projection
                    .Include(e => e.Invitations.Where(e => e.InvitedId == userId))).FirstOrDefault();
            if (e == null)
            {
                return false;
            }
            //todo add update
            return true;
        }

        public IEnumerable<Event> GetEventsForUser(ObjectId userId)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }
            
            return  context.Event.
                Find(e => e.OwnerId == userId 
                          || e.Invitations.Any(e => e.InvitedId == userId && e.Accepted) )
                .Project<Event>(Builders<Event>.Projection
                    .Exclude(e => e.Coordinates)
                    .Exclude(e => e.Invitations))
                .ToList();
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


            var checkIfInviterCanInvite = context.Event
                .Find(r => r.Id == eventId
                           && (r.OwnerId == inviterId || r.Invitations.Any(e =>
                               e.InviterId == inviterId &&
                               e.Accepted && e.CanInvite)) && !r.Invitations.Any(f => f.InvitedId == invitedId)).FirstOrDefault();

            if (checkIfInviterCanInvite != null)
            {
                Invitation i = new Invitation
                {
                    Accepted = false, AcceptedTime = null,InvitationTime = DateTime.Now, CanInvite = canInvite, InvitedId = invitedId,
                    InviterId = inviterId
                };

                context.Event.UpdateOne(e => e.Id == eventId,
                    Builders<Event>.Update.AddToSet(p => p.Invitations,i));
                return true;
            }

            return false;
        }
    }
}