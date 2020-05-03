using System.Collections.Generic;
using Cycler.Data.Models;
using MongoDB.Bson;

namespace Cycler.Data.Repositories.Interfaces
{
    public interface IEventRepository
    {
        public Event AddEvent(Event e);
        


        public IEnumerable<Event> GetEventsForUser(ObjectId userId);



        public Event GetEvent(ObjectId eventId);

    }
    

}