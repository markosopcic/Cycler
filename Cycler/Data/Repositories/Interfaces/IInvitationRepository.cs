using System.Collections.Generic;
using Cycler.Data.Models;
using MongoDB.Bson;

namespace Cycler.Data.Repositories.Interfaces
{
    public interface IInvitationRepository
    {
        bool AcceptInvitation(ObjectId invitationId,bool accept);
         bool InviteUserToEvent(ObjectId invitedId, ObjectId inviterId, ObjectId eventId, bool canInvite);
         int CountInvited(ObjectId eventId);
         int CountAccepted(ObjectId eventId);
         Invitation GetByUserEvent(ObjectId userId, ObjectId eventId);
         IEnumerable<Event> GetUserEventInvitations(ObjectId user);
    }
}