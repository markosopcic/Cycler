﻿using System.Collections.Generic;
using Cycler.Controllers.Models;
using Cycler.Data.Models;

namespace Cycler.Data.Repositories.Interfaces
{
    public interface IUserRepository
    {
        User Login(string email, string password);

        User Register(RegisterModel user);

        IEnumerable<User> GetAll();

        bool Delete(string id);

        User GetById(string id);
    }
}