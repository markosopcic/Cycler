using System;
using System.Collections.Generic;
using System.Linq;
using Cycler.Controllers.Models;
using Cycler.Data.Repositories.Interfaces;
using Cycler.Extensions;
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
        private IEventRepository eventRepository { get; }
        public ApiController(IUserRepository userRepository,IEventRepository eventRepository)
        {
            this.userRepository = userRepository;
            this.eventRepository = eventRepository;
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

            return Ok(eventRepository.GetUserEventData(eId, ids).ToDictionary(x => x.UserId.ToString(),x => x.Locations));
        }
    }
}