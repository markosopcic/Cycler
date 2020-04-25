﻿using AutoMapper;
using Cycler.Controllers.Models;
using Cycler.Data.Models;

namespace Automapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserModel>();
            CreateMap<RegisterModel, User>();
        }
    }
}