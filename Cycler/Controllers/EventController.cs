using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Cycler.Controllers.Models;
using Cycler.Data.Models;
using Cycler.Data.Repositories.Interfaces;
using Cycler.Extensions;
using Cycler.Views.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace Cycler.Controllers
{
    
    [Authorize]
    public class EventController : Controller
    {
        
        private IEventRepository eventRepository { get; }

        public EventController(IEventRepository eventRepository)
        {
            this.eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
        }

        public IActionResult Create()
        {
            return View("CreateEvent");
        }


        public IActionResult CreateEvent([FromForm] EventModel model)
        {
            eventRepository.AddEvent(new Event
            {
                StartTime = model.StartTime,
                Private = model.IsPrivate,
                OwnerId = User.Identity.GetUserId(),
                Name = model.Name,
                Invitations = new List<Invitation>(),
                Description =  model.Description
            });
            return RedirectToAction("Index");
        }
        
        public IActionResult Index()
        {
            var events = eventRepository.GetEventsForUser(User.Identity.GetUserId());
            eventRepository.InviteUserToEvent(ObjectId.Parse("5ea481b92904ac28d8328b0f"),User.Identity.GetUserId(),  events.First().Id,
                true);
                var result = events.Select(e => new EventViewModel
                {
                    Name = e.Name,
                    Description = e.Description,
                    Private =  e.Private,
                    StartTime = e.StartTime,
                    EndTime =  e.EndTime,
                    Accepted = e.Invitations?.Count(f => f.Accepted) ?? 0,
                    Invited = e.Invitations?.Count(f => !f.Accepted) ?? 0
                }).OrderByDescending(e => e.StartTime);
            return View(result);
        }
    }
}