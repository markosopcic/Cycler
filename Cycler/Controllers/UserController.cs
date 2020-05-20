﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
 using System.Linq;
using System.Security.Claims;
using Cycler.Controllers.Models;
using Cycler.Data.Models;
 using Cycler.Data.Repositories.Interfaces;
 using Cycler.Extensions;
 using Cycler.Views.Models;
 using Microsoft.AspNetCore.Authentication;
 using Microsoft.AspNetCore.Authorization;
using static Cycler.Helpers.Utility;
 namespace Cycler.Controllers
{

    public class UserController : Controller
    {
        private IUserRepository userRepository;
        private IEventRepository eventRepository;
        private IFriendshipRepository friendshipRepository;
        private IInvitationRepository invitationRepository;
        private IMapper mapper;
        public UserController(
            IUserRepository userRepository,
            IEventRepository eventRepository,
            IFriendshipRepository friendshipRepository,
            IInvitationRepository invitationRepository,
            IMapper mapper)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.mapper = mapper;
            this.eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
            this.friendshipRepository = friendshipRepository ?? throw new ArgumentNullException(nameof(friendshipRepository));
            this.invitationRepository = invitationRepository ?? throw new ArgumentNullException(nameof(invitationRepository));
        }
        
        public IActionResult AcceptFriendRequest([FromQuery]string fromUser,[FromQuery]bool accept)
        {
            if(fromUser == null) throw new ArgumentNullException(nameof(fromUser));
            var parsed = TryParseObjectId(fromUser);
            if(!parsed.HasValue) return BadRequest("Invalid user id!");
            friendshipRepository.AcceptFriendshipRequest(accept,User.Identity.GetUserId(),parsed.Value);
            return Ok();
        }

        public IActionResult FriendRequests()
        {
            var userId = User.Identity.GetUserId();
            var requests = friendshipRepository.GetUserRequests(userId);
            return View(requests.Select(e => new FriendRequestViewModel
            {
                Id = e.Id.ToString(),
                Sender = e.Sender.ToString(),
                SenderName = userRepository.GetById(e.Sender).FullName,
                TimeSent = e.TimeSent.ToUserTime(User),
            }));
        }

        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Authenticate([FromForm]AuthenticateModel model)
        {
            var user = userRepository.Login(model.Email, model.Password);
            if (user == null)
            {
                ViewBag.HasMessage = true;
                ViewBag.Message = "Invalid credentials. Please try again.";
                return View("Login");
            }

            
            var userClaims = new List<Claim>()  
            {  
                new Claim(ClaimTypes.NameIdentifier,  user.Id.ToString()),
                new Claim(ClaimTypes.Name,  user.FirstName),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim(ClaimTypes.Surname,user.LastName), 
                new Claim("TimeOffset",model.TimeOffset?.ToString())
            };  
  
            var grandmaIdentity = new ClaimsIdentity(userClaims, "User Identity");  
  
            var userPrincipal = new ClaimsPrincipal(new[] { grandmaIdentity });  
            HttpContext.SignInAsync(userPrincipal).Wait();
            return RedirectToAction("Index","Home");
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync().Wait();
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromForm]RegisterModel model)
        {
            if (model == null || model.Email == null || model.Password == null || model.ConfirmPassword == null ||
                model.FirstName == null || model.LastName == null || model.Password != model.ConfirmPassword)
            {
                ViewBag.HasMessage = true;
                ViewBag.Message = "Invalid registration attempt. Please check that no field is empty and that your passwords match.";
                return View("Login");
            }
            // map model to entity
            try
            {
                // create user
                var user = userRepository.Register(model);
                if (user == null)
                {
                    ViewBag.HasMessage = true;
                    ViewBag.Message = "Email already registered.";
                    return View("Login");
                }
                var userClaims = new List<Claim>()  
                {  
                    new Claim(ClaimTypes.NameIdentifier,  user.Id.ToString()),
                    new Claim(ClaimTypes.Name,  user.FirstName),
                    new Claim(ClaimTypes.Email,user.Email),
                    new Claim(ClaimTypes.Surname,user.LastName), 
                };  
  
                var grandmaIdentity = new ClaimsIdentity(userClaims, "User Identity");  
  
                var userPrincipal = new ClaimsPrincipal(new[] { grandmaIdentity });  
                HttpContext.SignInAsync(userPrincipal).Wait();
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        
        [Route("User/SendFriendRequest/{friendId}")]
        public IActionResult SendFriendRequest([FromRoute]string friendId)
        {
            if (friendId == null)
            {
                throw new ArgumentNullException(nameof(friendId));
            }
            var userId = User.Identity.GetUserId();
            var parsedId = TryParseObjectId(friendId);
            if (!parsedId.HasValue) return NotFound(nameof(friendId));
            friendshipRepository.SendFriendRequest(userId, parsedId.Value);

            return RedirectToAction("Profile", new {userId = friendId});
        }

        public IActionResult Invitations()
        {
            var user = User.Identity.GetUserId();
            var events = invitationRepository.GetUserEventInvitations(user);
            return View(events.Select(e =>
            {
                var invitation = invitationRepository.GetByUserEvent(User.Identity.GetUserId(), e.Id);
                var user = userRepository.GetById(invitation.InviterId);
                return new InvitationViewModel
                    {
                        EventId = invitation.EventId.ToString(),
                        InvitationId = invitation.InvitationId.ToString(),
                        EventName = e.Name, EventDescription = e.Description, 
                        EventStartTime = e.StartTime.ToUserTime(User),
                        InvitationTime = invitation.InvitationTime.ToUserTime(User), InvitedBy = new User{FirstName = user.FirstName,LastName = user.LastName,FullName = user.FullName}
                    };
            }));
        }
        
        public IActionResult Events()
        {
            var user = User.Identity.GetUserId();
            return View(eventRepository.GetEventsForUser(user));
        }

        [Route("User/Profile/{userId}")]
        public IActionResult Profile([FromRoute] string userId)
        {
            if (userId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var parsed = TryParseObjectId(userId);
            if (!parsed.HasValue)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = userRepository.GetById(parsed.Value);
            var model = mapper.Map<UserViewModel>(user);
            if (user.Friends.Contains(User.Identity.GetUserId()))
            {
                model.isFriend = true;
            }
            else
            {
                var request =   friendshipRepository.FindFriendshipRequest(User.Identity.GetUserId(), user.Id);
                if (request == null)
                {
                    model.FriendshipRequestReceived = false;
                    model.FriendshipRequestSent = false;
                }
                else
                {
                    if (User.Identity.GetUserId() == request.Receiver)
                    {
                        model.FriendshipRequestReceived = true;
                        model.FriendshipRequestSent = false;
                        model.FriendshipRequestId = request.Id.ToString();
                    }
                    else
                    {
                        model.FriendshipRequestSent = true;
                        model.FriendshipRequestReceived = false;
                    }
                }
            }

            return View(model);
        }
        
        
        
        
    }
}