﻿using System;
using System.Collections.Generic;
using System.Linq;
using Cycler.Controllers.Models;
using Cycler.Data.Repositories.Interfaces;
using Cycler.Extensions;
using Cycler.Views.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MongoDB.Bson;

namespace Cycler.Controllers
{
    [Route("/api")]
    [ApiController]
    [Authorize]
    public class ApiController : Controller
    {
        private IUserRepository userRepository { get; }
        
        private IInvitationRepository invitationRepository { get; }
        private IEventRepository eventRepository { get; }
        public ApiController(IUserRepository userRepository,IEventRepository eventRepository,IInvitationRepository invitationRepository)
        {
            this.userRepository = userRepository;
            this.eventRepository = eventRepository;
            this.invitationRepository = invitationRepository;
        }

        [Route("search-users")]
        public IActionResult SearchUsers(string term)
        {
            if(term == null) throw new ArgumentNullException(nameof(term));
            return Json(userRepository.SearchUsers(term).Where( e => e.Id != User.Identity.GetUserId()).Select(e => new {Id = e.Id.ToString(),FullName = e.FirstName+" "+e.LastName,ResultType = "User"}));
        }

        [Route("get-users-for-event")]
        public IActionResult GetUsersForEvent(string eventId)
        {
            ObjectId eId;
            if (!ObjectId.TryParse(eventId,out eId))
            {
                return BadRequest();
            }

            return Json(eventRepository.GetUsersForEvent(eId));
        }

        [Route("GetHistoricalData")]
        [HttpPost]
        public IActionResult GetHistoricalData([FromBody] List<string> userIds = null, [FromQuery] string eventId = null)
        {
            ObjectId eId;
            if (userIds == null || userIds.Count == 0 || eventId == null || !ObjectId.TryParse(eventId,out eId))
            {
                return BadRequest();
            }

            var ids = new List<ObjectId>();
            foreach (var userid in userIds)
            {
                ObjectId user;
                if (ObjectId.TryParse(userid, out user))
                {
                    ids.Add(user);
                }
            }

            return Ok(eventRepository.GetUserEventData(eId, ids).ToDictionary(x => x.UserId.ToString(),x => x.Locations.Select(
                e => { e.Time = e.Time.ToUserTime(User);
                    return e;
                })));
        }

        [Route("get-event-feed")]
        public IActionResult GetEventFeed(int skip = 0, int take = 10)
        {
            var eventFeed = eventRepository.GetEventFeed(User.Identity.GetUserId(),skip, take).ToList().Select(e =>
            {
                e.StartTime = e.StartTime.ToUserTime(User);
                return e;
            }).ToList();
            var users = userRepository.GetUsersByIds(eventFeed.SelectMany(e => e.UserEventData).Select(e => e.UserId)
                .ToList());
            return PartialView("../Home/PartialFeed",new FeedViewModel{
                EventFeed = eventFeed,
                Users = users
            });
        }
        
        [Route("get-personal-event-feed")]
        public IActionResult GetPersonalEventFeed(int skip = 0, int take = 10)
        {
            var events = eventRepository.GetEventsForUser(User.Identity.GetUserId(),skip,take);
            var result = events.Select(e => new EventViewModel
            {
                Id = e.Id.ToString(),
                Name = e.Name,
                Description = e.Description,
                Private = e.Private,
                StartTime = e.StartTime.ToUserTime(User),
                EndTime = e.EndTime?.ToUserTime(User),
                Accepted = invitationRepository.CountAccepted(e.Id),
                Invited = invitationRepository.CountInvited(e.Id),
                OwnerId = e.OwnerId.ToString(),
                UserEventData = e.UserEventData
            });
            return PartialView("../Event/PartialIndexView",result);
        }
        
        [Route("search-friends")]
        public IActionResult SearchFriends(string term)
        {
            if(term == null) throw new ArgumentNullException(nameof(term));
            return Json(userRepository.SearchFriends(User.Identity.GetUserId(),term).Where( e => e.Id != User.Identity.GetUserId()).Select(e => new {Id = e.Id.ToString(),FullName = e.FirstName+" "+e.LastName,ResultType = "User"}));
        }
        
        [Route("search-friends-not-invited")]
        public IActionResult SearchFriendsNotInvited(string term,string eventId)
        {
            if(term == null) throw new ArgumentNullException(nameof(term));
            ObjectId eId;
            if (!ObjectId.TryParse(eventId,out  eId))
            {
                return BadRequest("Invalid EventID");
            }
            return Json(userRepository.SearchFriendsNotInvited(User.Identity.GetUserId(),term,eId).Where( e => e.Id != User.Identity.GetUserId()).Select(e => new {Id = e.Id.ToString(),FullName = e.FirstName+" "+e.LastName,ResultType = "User"}));
        }

        [HttpPost]
        [Route("invite-more-friends")]
        public IActionResult InviteMoreFriends([FromQuery] string eventId, [FromBody] IEnumerable<string> ids)
        {
            ObjectId eId;
            if (!ObjectId.TryParse(eventId, out eId))
            {
                return BadRequest("Invalid event id");
            }

            if (ids == null || ids.Count() == 0)
            {
                return BadRequest("No ids given!");
            }

            ObjectId tmp;
            eventRepository.InviteMoreFriends(User.Identity.GetUserId(),eId,ids.Where(e => ObjectId.TryParse(e,out tmp)).Select(e => ObjectId.Parse(e)));
            return Ok();
        }
    }
}