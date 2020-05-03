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



 
        
    }
}