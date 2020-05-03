using MongoDB.Bson;

namespace Cycler.Data.Repositories.Interfaces
{
    public interface IFriendshipRepository
    {
        bool SendFriendRequest(ObjectId fromUser, ObjectId toUser);
        void AcceptFriendshipRequest(bool accept, ObjectId user, ObjectId from);
        public bool FriendshipRequestSent(ObjectId from, ObjectId to);
    }
}