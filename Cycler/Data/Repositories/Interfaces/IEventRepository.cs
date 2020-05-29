using System.Collections.Generic;
using Cycler.Data.Models;
using Cycler.Views.Models;
using MongoDB.Bson;

namespace Cycler.Data.Repositories.Interfaces
{
    public interface IEventRepository
    {
        public Event AddEvent(Event e);
        


        public IEnumerable<Event> GetEventsForUser(ObjectId userId);



        public Event GetEvent(ObjectId eventId);

        public IEnumerable<Event> GetActiveEvents(ObjectId userId);

        public void AddLocationsForEvent(ObjectId eventId, UserEventData eventData);

        public List<EventUserModel> GetUsersForEvent(ObjectId eventId);

        public IEnumerable<UserEventData> GetUserEventData(ObjectId eventId,List<ObjectId> users);

        public IEnumerable<Event> GetEventFeed(ObjectId userId, int skip = 0, int take = 10);

    }
    

}