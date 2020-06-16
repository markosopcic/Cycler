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


        

        public IEnumerable<Event> GetEventsForUser(ObjectId userId,int skip, int take)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }
            
            return  context.Event.
                Find(e => e.OwnerId == userId 
                          || e.AcceptedUsers.Any(e => e == userId ) )
                .Project<Event>(Builders<Event>.Projection
                    .Include(e => e.Description)
                    .Include(e => e.Name)
                    .Include(e => e.OwnerId)
                    .Include(e => e.AcceptedUsers)
                    .Include(e => e.StartTime)
                    .Include(e => e.Finished)
                    .Include(e => e.EndTime)
                    .Include(e => e.EndTime)
                    .Include(e => e.Id)
                    .Include(e => e.Private)
                    .Include("UserEventData.Duration")
                    .Include("UserEventData.Meters")
                    .Include("UserEventData.UserId")).SortByDescending(e => e.StartTime)
                .Skip(skip).Limit(take)
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
                    (e.StartTime > DateTime.UtcNow.AddHours(-12)  || e.StartTime > DateTime.UtcNow.AddMinutes(15)))
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

            var exists = context.Event.Find(Builders<Event>.Filter.Where(e => e.Id == eventId) 
                                            &  Builders<Event>.Filter.Eq("UserEventData.UserId", eventData.UserId))
                .FirstOrDefault();
            if (exists != null)
            {
                exists.UserEventData[0].Locations.AddRange(eventData.Locations);
                var currUserData = exists.UserEventData.First(e => e.UserId == eventData.UserId);
                context.Event.UpdateOne(Builders<Event>.Filter.Where(e => e.Id == eventId)
                                        & Builders<Event>.Filter.Eq("UserEventData.UserId", eventData.UserId)
                    , Builders<Event>.Update.Set("UserEventData.$.Duration",
                            exists.UserEventData.First(e => e.UserId == eventData.UserId).Duration + eventData.Duration)
                        .Set("UserEventData.$.Meters", exists.UserEventData.First(e => e.UserId == eventData.UserId).Meters + eventData.Meters)
                        .Set("UserEventData.$.Locations", exists.UserEventData.First(e => e.UserId == eventData.UserId).Locations));
            }
            else
            {
                context.Event.UpdateOne(e => e.Id == eventId,
                    Builders<Event>.Update.AddToSet(e => e.UserEventData, eventData));
            }

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
            return context.Event
                .Find(Builders<Event>.Filter.Eq(e => e.Finished,true) & (Builders<Event>.Filter.Eq(e => e.OwnerId,userId) | Builders<Event>.Filter.In(e => e.OwnerId,user.Friends) | Builders<Event>.Filter.AnyIn(e => e.AcceptedUsers, user.Friends)))
                .SortByDescending(e => e.StartTime)
                .Skip(skip)
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

        public void DeleteEvent(ObjectId userId, ObjectId eventId)
        {
            context.Invitation.DeleteMany(e => e.EventId == eventId);
            context.Event.DeleteOne(e => e.OwnerId == userId && e.Id == eventId);
        }


        public void CheckAndFinishEvent(ObjectId userId, ObjectId eventId)
        {
            context.Event.UpdateOne(e => e.Id == eventId && e.OwnerId == userId,
                Builders<Event>.Update.Set(e => e.Finished, true).Set(e => e.EndTime, DateTime.SpecifyKind(DateTime.UtcNow,DateTimeKind.Utc)));
        }
    }
}