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


        

        public IEnumerable<Event> GetEventsForUser(ObjectId userId)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }
            
            return  context.Event.
                Find(e => e.OwnerId == userId 
                          || e.AcceptedUsers.Any(e => e ==userId ) )
                .Project<Event>(Builders<Event>.Projection
                    .Exclude(e => e.Coordinates))
                .ToList();
            }

 



        public Event GetEvent(ObjectId eventId)
        {
            if (eventId == null)
            {
                throw new ArgumentNullException(nameof(eventId));
            }

            return context.Event.Find(e => e.Id == eventId).Project<Event>(Builders<Event>.Projection
                .Exclude(e => e.Coordinates)).FirstOrDefault();
        }
    }
}