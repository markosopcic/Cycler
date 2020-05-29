using System;
using System.Collections.Generic;
using System.Linq;
using Cycler.Controllers.Models;
using Cycler.Data.Models;
using Cycler.Data.Repositories.Interfaces;
using Cycler.Extensions;
using Cycler.Views.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using static Cycler.Helpers.Utility;

namespace Cycler.Controllers
{
    
    [Authorize]
    public class EventController : Controller
    {

        private IEventRepository eventRepository;
        private IInvitationRepository invitationRepository;

        public EventController(IEventRepository eventRepository,IInvitationRepository invitationRepository)
        {
            this.eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
            this.invitationRepository =
                invitationRepository ?? throw new ArgumentNullException(nameof(eventRepository));
        }

        [Route("Event/Details/{eventId}")]
        public IActionResult Details([FromRoute] string eventId)
        {
            if(eventId == null) throw new ArgumentNullException(nameof(eventId));
            var parsedId = TryParseObjectId(eventId);
            if (!parsedId.HasValue)
            {
                return RedirectToAction("Index");
            }
            var e = eventRepository.GetEvent(parsedId.Value);
            var users = eventRepository.GetUsersForEvent(e.Id);
            return View(new EventViewModel
            {
                Id = e.Id.ToString(),
                Name = e.Name,
                Description = e.Description,
                Private = e.Private,
                Finished =  e.Finished,
                StartTime = e.StartTime.ToUniversalTime(),
                EndTime = e.EndTime?.ToUniversalTime(),
                Accepted = invitationRepository.CountAccepted(parsedId.Value),
                Invited = invitationRepository.CountInvited(parsedId.Value),
                OwnerId = e.OwnerId.ToString(),
                UserIds = users,
                UserEventData = e.UserEventData
            });


        }
        
        public IActionResult Create()
        {
            return View("CreateEvent");
        }


        public IActionResult CreateEvent([FromForm] EventModel model)
        {
            var e = eventRepository.AddEvent(new Event
            {
                StartTime = model.StartTime.UtcFromUser(User),
                EndTime = null,
                Private = model.IsPrivate,
                OwnerId = User.Identity.GetUserId(),
                Name = model.Name,
                Description =  model.Description
            });
            foreach (var modelInvitedUserID in model.InvitedUserIDs)
            {
                var parsed = TryParseObjectId(modelInvitedUserID);
                if (parsed.HasValue)
                {
                    invitationRepository.InviteUserToEvent(parsed.Value, User.Identity.GetUserId(), e.Id, true);
                }

            }
            
            return RedirectToAction("Index");
        }

        public IActionResult AcceptInvitation([FromQuery] string InvitationId,[FromQuery]bool accept)
        {
            var parsedId = TryParseObjectId(InvitationId);
            if (InvitationId == null || parsedId == null)
            {
                return BadRequest("Invitation id cannot be null");
            }

            if (invitationRepository.AcceptInvitation(parsedId.Value,accept))
            {
                return Ok();
            }

            return BadRequest("Invalid invitation id!");
        }
        
        public IActionResult Index()
        {
            var events = eventRepository.GetEventsForUser(User.Identity.GetUserId());
            var result = events.Select(e => new EventViewModel
                {
                    Id = e.Id.ToString(),
                    Name = e.Name,
                    Description = e.Description,
                    Private =  e.Private,
                    StartTime = e.StartTime.ToUserTime(User),
                    EndTime =  e.EndTime?.ToUserTime(User),
                    Accepted = invitationRepository.CountAccepted(e.Id),
                    Invited = invitationRepository.CountInvited(e.Id),
                    OwnerId = e.OwnerId.ToString()
                }).OrderByDescending(e => e.StartTime);
            return View(result);
        }
    }
}