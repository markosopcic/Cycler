﻿using AutoMapper;
using Cycler.Controllers.Models;
using Cycler.Data.Models;
 using Cycler.Views.Models;

 namespace Automapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserModel>();
            CreateMap<RegisterModel, User>();
            CreateMap<User, UserViewModel>().AfterMap((src, dest) => { dest.NumOfFriends = src.Friends.Count;
                dest.Id = src.Id.ToString();
            });
        }
    }
}