using System.Collections.Generic;
using Cycler.Data.Models;
using MongoDB.Bson;

namespace Cycler.Data.Repositories.Interfaces
{
    public interface IFriendshipRepository
    {
        bool SendFriendRequest(ObjectId fromUser, ObjectId toUser);
        void AcceptFriendshipRequest(bool accept, ObjectId user, ObjectId from);
        public FriendshipRequest FindFriendshipRequest(ObjectId firstUser, ObjectId secondUser);

        public IEnumerable<FriendshipRequest> GetUserRequests(ObjectId userId);
    }
}