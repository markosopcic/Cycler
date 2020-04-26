using System.Collections.Generic;
using Cycler.Data.Models;
using MongoDB.Bson;

namespace Cycler.Data.Repositories.Interfaces
{
    public interface IEventRepository
    {
        public Event AddEvent(Event e);
        
        bool AcceptInvitation(ObjectId userId, ObjectId eventId);

        public IEnumerable<Event> GetEventsForUser(ObjectId userId);

        public bool InviteUserToEvent(ObjectId invitedId, ObjectId inviterId, ObjectId eventId, bool canInvite);
    }
    

}