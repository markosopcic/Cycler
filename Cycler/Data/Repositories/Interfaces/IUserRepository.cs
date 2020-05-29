﻿using System.Collections.Generic;
using Cycler.Controllers.Models;
using Cycler.Data.Models;
 using MongoDB.Bson;

 namespace Cycler.Data.Repositories.Interfaces
{
    public interface IUserRepository
    {
        User Login(string email, string password);

        User Register(RegisterModel user);
        

        bool Delete(ObjectId id);

        User GetById(ObjectId id);


        IEnumerable<User> SearchUsers(string term);

        void UpdateOnlineStatus(ObjectId userId);

        List<User> GetActiveFriends(ObjectId userId);

        List<Event> GetActiveEvents(ObjectId userId);

        List<User> GetUsersByIds(List<ObjectId> userIds);





    }
}