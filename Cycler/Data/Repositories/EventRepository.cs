using System;
using System.Collections.Generic;
using System.Linq;
using Cycler.Data.Models;
using Cycler.Data.Repositories.Interfaces;
using Cycler.Views.Models;
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
                          || e.AcceptedUsers.Any(e => e == userId ) )
                .Project<Event>(Builders<Event>.Projection
                    .Include(e => e.Name)
                    .Include(e => e.Description)
                    .Include(e => e.OwnerId)
                    .Include(e => e.StartTime)
                    .Include(e => e.Id))
                .ToList();
            }

 



        public Event GetEvent(ObjectId eventId)
        {
            if (eventId == null)
            {
                throw new ArgumentNullException(nameof(eventId));
            }

            return context.Event.Find(e => e.Id == eventId).Project<Event>(Builders<Event>.Projection 
                .Include(e => e.Name)
                .Include(e => e.Description)
                .Include(e => e.OwnerId)
                .Include( e => e.Private)
                .Include( e => e.Finished)
                .Include(e => e.StartTime)
                .Include(e => e.EndTime)
                .Include(e => e.Id)
                .Include("UserEventData.Duration")
                .Include("UserEventData.Meters")
                .Include("UserEventData.UserId"))
                .FirstOrDefault();
        }

        public IEnumerable<Event> GetActiveEvents(ObjectId userId)
        {
            return context.Event
                .Find(e => (e.OwnerId == userId || e.AcceptedUsers.Any(e => e == userId)) && !e.Finished &&
                           e.StartTime >= DateTime.UtcNow.AddMinutes(-15) && e.StartTime <=DateTime.UtcNow.AddHours(8))
                .Project<Event>(Builders<Event>.Projection
                    .Include(e => e.Name)
                    .Include(e => e.Description)
                    .Include(e => e.OwnerId)
                    .Include(e => e.Private)
                    .Include(e => e.StartTime)
                    .Include(e => e.Id))
                    .ToList();
        }

        public void AddLocationsForEvent(ObjectId eventId, UserEventData eventData)
        {
            if (eventId == null) throw new ArgumentNullException(nameof(eventId));
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));

            context.Event.UpdateOne(e => e.Id == eventId,
                Builders<Event>.Update.AddToSet(e => e.UserEventData, eventData));
        }

        public List<EventUserModel> GetUsersForEvent(ObjectId eventId)
        {
            if(eventId == null) throw new ArgumentNullException(nameof(eventId));

            var e = context.Event.Find(e => e.Id == eventId)
                .Project<Event>(Builders<Event>.Projection.Include("UserEventData.UserId")).FirstOrDefault();
            if(e == null) return new List<EventUserModel>();
            var ids = e.UserEventData.Select(e => e.UserId).ToList();
            var users = context.User.Find(e => ids.Contains(e.Id)).Project<User>(Builders<User>.Projection
                .Include(e => e.Id).Include(e => e.FirstName).Include(e => e.LastName)).ToList();
            return users.Select(e => new EventUserModel
            {
                UserId = e.Id.ToString(),
                Name = e.FirstName + " " + e.LastName
            }).ToList();
        }

        public IEnumerable<UserEventData> GetUserEventData(ObjectId eventId,List<ObjectId> users)
        {
            if(eventId == null) throw new ArgumentNullException(nameof(eventId));
            return context.Event.Find(e => e.Id == eventId).Project(e => new Event
                {UserEventData = e.UserEventData.Where(e => users.Contains(e.UserId)).ToList()}).FirstOrDefault().UserEventData;
        }

        public IEnumerable<Event> GetEventFeed(ObjectId userId, int skip = 0, int take = 10)
        {
            if(userId == null) throw new ArgumentNullException(nameof(userId));

            var user = context.User.Find(e => e.Id == userId).FirstOrDefault();
            if (user == null) return null;
            return context.Event.Find(Builders<Event>.Filter.Eq(e => e.Finished,true) & (Builders<Event>.Filter.In(e => e.OwnerId,user.Friends) | Builders<Event>.Filter.AnyIn(e => e.AcceptedUsers, user.Friends))).Skip(skip)
                .Limit(take).Project<Event>(Builders<Event>.Projection
                    .Include(e => e.Description)
                    .Include(e => e.Name)
                    .Include(e => e.OwnerId)
                    .Include(e => e.AcceptedUsers)
                    .Include(e => e.StartTime)
                    .Include(e => e.EndTime)
                    .Include(e => e.Id)
                    .Include("UserEventData.Duration")
                    .Include("UserEventData.Meters")
                    .Include("UserEventData.UserId"))
                .ToList();
        }
    }
}