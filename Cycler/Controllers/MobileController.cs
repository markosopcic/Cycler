using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using AutoMapper;
using Cycler.Controllers.Models;
using Cycler.Data.Models;
using Cycler.Data.Repositories.Interfaces;
using Cycler.Extensions;
using Cycler.Views.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Cycler.Helpers.Utility;

namespace Cycler.Controllers
{
    [ApiController]
    [Authorize]
    public class MobileController : Controller
    {

        private IUserRepository userRepository;
        private IEventRepository eventRepository;
        private IFriendshipRepository friendshipRepository;
        private IInvitationRepository invitationRepository;
        private IMapper mapper;
        public MobileController(
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
        [AllowAnonymous]
        [HttpPost]
        [Route("/mobile/login")]
        public IActionResult Login([FromBody] AuthenticateModel model)
        {
            if (model == null || model.Password == null || model.Email == null)
                return BadRequest("Invalid values sent!");

            var user = userRepository.Login(model.Email, model.Password);
            if (user == null)
            {
                return Unauthorized("Invalid credentials!");
            }
            else
            {
                var userClaims = new List<Claim>()  
                {  
                    new Claim(ClaimTypes.NameIdentifier,  user.Id.ToString()),
                    new Claim(ClaimTypes.Name,  user.FirstName),
                    new Claim(ClaimTypes.Email,user.Email),
                    new Claim(ClaimTypes.Surname,user.LastName), 
                    new Claim("TimeOffset",model.TimeOffset?.ToString() ?? "0")
                };  
  
                var grandmaIdentity = new ClaimsIdentity(userClaims, "User Identity");  
  
                var userPrincipal = new ClaimsPrincipal(new[] { grandmaIdentity });  
                HttpContext.SignInAsync(userPrincipal).Wait();
                return Ok();
            }
        }
        [Route("/mobile/logout")]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync().Wait();
            return Ok();
        }
        
        [AllowAnonymous]
        [HttpPost]
        [Route("/mobile/register")]
        public IActionResult Register([FromBody]RegisterModel model)
        {
            if (model == null || model.Email == null || model.Password == null || model.ConfirmPassword == null ||
                model.FirstName == null || model.LastName == null || model.Password != model.ConfirmPassword)
            {
                return BadRequest("Invalid request. Check all fields and try again!");
            }
            // map model to entity
            try
            {
                // create user
                var user = userRepository.Register(model);
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
                return Ok();
            }
            catch (Exception ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }
        
        [Route("mobile/friend-requests")]
        public IActionResult FriendRequests()
        {
            var userId = User.Identity.GetUserId();
            var requests = friendshipRepository.GetUserRequests(userId);
            return Json(requests.Select(e => new FriendRequestViewModel
            {
                Id = e.Id.ToString(),
                Sender = e.Sender.ToString(),
                SenderName = userRepository.GetById(e.Sender).FullName,
                TimeSent = e.TimeSent.ToUserTime(User),
            }));
        }
        [Route("mobile/accept-friend-request")]
        public IActionResult AcceptFriendRequest([FromQuery]string fromUser,[FromQuery]bool accept)
        {
            if(fromUser == null) throw new ArgumentNullException(nameof(fromUser));
            var parsed = TryParseObjectId(fromUser);
            if(!parsed.HasValue) return BadRequest("Invalid user id!");
            friendshipRepository.AcceptFriendshipRequest(accept,User.Identity.GetUserId(),parsed.Value);
            return Ok();
        }

        [Route("/mobile/invitations")]
        public IActionResult EventInvitations()
        {
            var user = User.Identity.GetUserId();
            var events = invitationRepository.GetUserEventInvitations(user);
            return Json(events.Select(e =>
            {
                var invitation = invitationRepository.GetByUserEvent(User.Identity.GetUserId(), e.Id);
                var user = userRepository.GetById(invitation.InviterId);
                return new InvitationViewModel
                {
                    EventId = invitation.EventId.ToString(),
                    InvitationId = invitation.InvitationId.ToString(),
                    EventName = e.Name, EventDescription = e.Description, 
                    EventStartTime = e.StartTime,
                    InvitationTime = invitation.InvitationTime, InvitedBy = new User{FirstName = user.FirstName,LastName = user.LastName,FullName = user.FullName}
                };
            }));
        }
        
        [Route("/mobile/accept-invitation")]
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
        
    }
}