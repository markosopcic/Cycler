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
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using static Cycler.Helpers.Utility;
using Location = Cycler.Data.Models.Location;

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
        private IHubContext<LocationHub> locationHub;
        private IMapper mapper;
        public MobileController(
            IUserRepository userRepository,
            IEventRepository eventRepository,
            IFriendshipRepository friendshipRepository,
            IInvitationRepository invitationRepository,
            IMapper mapper,
            IHubContext<LocationHub> locationHub)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.mapper = mapper;
            this.eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
            this.friendshipRepository = friendshipRepository ?? throw new ArgumentNullException(nameof(friendshipRepository));
            this.invitationRepository = invitationRepository ?? throw new ArgumentNullException(nameof(invitationRepository));
            this.locationHub = locationHub ?? throw new ArgumentNullException(nameof(locationHub));
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
                return Ok(new {userId = user.Id.ToString()});
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
            try
            {
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
            return Json(requests.Select(e =>
            {
                var user = userRepository.GetById(e.Sender);
                
                return new FriendRequestViewModel
                {
                    Id = e.Id.ToString(),
                    Sender = e.Sender.ToString(),
                    SenderName = user.FirstName+ " "+ user.LastName,
                    TimeSent = e.TimeSent.ToUserTime(User).ToString("f"),
                };
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
                    EventStartTime = e.StartTime.ToUserTime(User).ToString("f"),
                    InvitationTime = invitation.InvitationTime.ToUserTime(User).ToString("f"), InvitedBy = new User{FirstName = user.FirstName,LastName = user.LastName,FullName = user.FullName}
                };
            }));
        }
        
        [Route("/mobile/accept-invitation")]
        public IActionResult AcceptInvitation([FromQuery] string invitationId,[FromQuery]bool accept)
        {
            var parsedId = TryParseObjectId(invitationId);
            if (invitationId == null || parsedId == null)
            {
                return BadRequest("Invitation id cannot be null");
            }

            if (invitationRepository.AcceptInvitation(parsedId.Value,accept))
            {
                return Ok();
            }

            return BadRequest("Invalid invitation id!");
        }

        [HttpPost]
        [Route("/mobile/send-location")]
        public IActionResult SendLocation([FromBody] LocationModel position)
        {
            var idToBroadcastTo = position.Id ?? User.Identity.GetUserId().ToString();
            locationHub.Clients.Group(idToBroadcastTo).SendCoreAsync("Position",
                new object[] {User.Identity.GetSpecificClaim(ClaimTypes.Name) + " "+ User.Identity.GetSpecificClaim(ClaimTypes.Surname),User.Identity.GetUserId().ToString(),position.Longitude, position.Latitude});
            
            if (position.Id == null && position.UpdateOnlineStatus)
            {
                userRepository.UpdateOnlineStatus(User.Identity.GetUserId());
            }
            return Ok();
        }

        [HttpGet]
        [Route("/mobile/get-active-events")]
        public IActionResult GetActiveEvents()
        {
            var activeEvents = eventRepository.GetActiveEvents(User.Identity.GetUserId());
            return Json(activeEvents.Select(e =>
            {
                var owner = userRepository.GetById(e.OwnerId);
                return new
                {
                    e.Description,
                    Id = e.Id.ToString(),
                    OwnerName = owner.FirstName+" "+owner.LastName,
                    OwnerId = owner.Id.ToString(),
                    e.Name,
                    StartTime = e.StartTime.ToUserTime(User).ToString("f")
                };
            }));
        }
        
        
        [Route("/mobile/profile/{userId}")]
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
            var model = new UserViewModel
            {
                NumOfFriends = user.Friends.Count,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DateJoined = user.DateJoined.ToString("f"),
                LastLogin = user.LastLogin.ToString("f")
            };
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

            return Json(model);
        }
        
        [Route("/mobile/SendFriendRequest/{friendId}")]
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

            return Ok();
        }

        [Route("/mobile/upload-event")]
        [HttpPost]
        public IActionResult UploadMobileEvent(MobileEventModel eventModel)
        {
            if (eventModel.UserId != User.Identity.GetUserId().ToString())
            {
                return Unauthorized();
            }
            
            ObjectId eventId;
            var timeStart = new DateTime
                (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var userLocations = new UserEventData
            {
                Duration = TimeSpan.FromSeconds(eventModel.Duration),
                Meters = eventModel.Meters,
                UserId = User.Identity.GetUserId(),
                Locations = eventModel.Locations.Select(e => new Location
                {
                    Latitude = e.Latitude, Longitude = e.Longitude,
                    Time = DateTime.SpecifyKind(timeStart.AddMilliseconds(e.TimeMillis),DateTimeKind.Utc)
                }).ToList()
            };
            if (eventModel.EventId == null || !ObjectId.TryParse(eventModel.EventId, out eventId))
            {
                var e = new Event
                {
                    Finished = true,
                    OwnerId = User.Identity.GetUserId(),
                    Name = eventModel.Name,
                    EndTime = timeStart.AddMilliseconds(eventModel.EndTimeMillis),
                    Private = true,
                    StartTime = timeStart.AddMilliseconds(eventModel.StarTimeMillis),
                    UserEventData = new List<UserEventData>()
                    {
                        userLocations
                        }

                };
                eventRepository.AddEvent(e);
                return Ok();
            }

            eventRepository.AddLocationsForEvent(eventId,userLocations);
            return Ok();

        }
        
        [Route("/mobile/get-event-feed")]
        public IActionResult GetEventFeed([FromQuery]int skip = 0,[FromQuery] int take = 10)
        {
            var eventFeed = eventRepository.GetEventFeed(User.Identity.GetUserId(),skip, take).ToList().Select(e =>
            {
                e.StartTime = e.StartTime.ToUserTime(User);
                return e;
            }).ToList();
            var users = userRepository.GetUsersByIds(eventFeed.SelectMany(e => e.UserEventData).Select(e => e.UserId)
                .ToList());
            var data = eventFeed.Select(e =>
            {
                var data = e.UserEventData.FirstOrDefault(ev => ev.UserId == e.OwnerId);
                return new
                {
                    e.Name, StartTime = e.StartTime.ToUserTime(User).ToString("f"), EndTime = e.EndTime?.ToUserTime(User).ToString("f"),
                    Duration = data?.Duration.TotalSeconds,
                    data?.Meters,
                    User = users.First(u => u.Id == e.OwnerId).FirstName +" "+ users.First(u => u.Id == e.OwnerId).LastName
                };
            });
            return Ok(data);
        }

        [Route("/mobile/user-profile")]
        public IActionResult UserProfileData()
        {
            var user = userRepository.GetById(User.Identity.GetUserId());
            return Ok(new
            {
                user.FirstName,
                user.LastName,
                user.Email,
                DateJoined = user.DateJoined.ToUserTime(User).ToString("f"),
                NumFriends = user.Friends.Count()
            });
        }


        [Route("/mobile/get-user-events")]
        public IActionResult GetUserEvents([FromQuery] int skip = 0, [FromQuery] int take = 10)
        {

                var events = eventRepository.GetEventsForUser(User.Identity.GetUserId(),skip,take);
                var users = userRepository.GetUsersByIds(events.SelectMany(e => e.UserEventData).Select(e => e.UserId)
                    .ToList());
                var result = events.Select(e =>
                {
                    var owner = userRepository.GetById(e.OwnerId);
                    return new MobileEventViewModel
                    {
                        OwnerName = owner.FirstName + " " + owner.LastName,
                        Id = e.Id.ToString(),
                        Name = e.Name,
                        Description = e.Description,
                        Private = e.Private,
                        Finished = e.Finished,
                        StartTime = e.StartTime.ToUserTime(User).ToString("f"),
                        EndTime = e.EndTime?.ToUserTime(User).ToString("f"),
                        Accepted = invitationRepository.CountAccepted(e.Id),
                        Invited = invitationRepository.CountInvited(e.Id),
                        OwnerId = e.OwnerId.ToString(),
                        UserEventData = e.UserEventData.Select(f =>
                            new MobileUserEventData
                            {
                                UserId = f.UserId.ToString(),
                                Meters = f.Meters,
                                DurationSeconds = (long) f.Duration.TotalSeconds,
                                UserName = users.First(e => e.Id == f.UserId).FirstName + " " +
                                           users.First(e => e.Id == f.UserId).LastName
                            }
                        ).ToList()
                    };
                }).OrderByDescending(x => x.StartTime);
                return Ok(result);
            
        }

        [Route("mobile/get-friends")]
        public IActionResult GetFriends()
        {
            return Ok(userRepository.GetFriends(User.Identity.GetUserId()).Select(e =>
                new {Id = e.Id.ToString(), FullName = e.FirstName + " " + e.LastName, ResultType = "User"}));
        }


        [HttpPost]
        [Route("/mobile/create-event")]
        public IActionResult CreateEvent(MobileCreateEventModel model)
        {
            if (model?.FriendIdsToInvite == null || model.Description == null || model.Name == null)
                return BadRequest();
            
            var timeStart = new DateTime
                (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            
            var e = eventRepository.AddEvent(new Event
            {
                StartTime = timeStart.AddMilliseconds(model.StartTimeMillis),
                EndTime = null,
                Private = false,
                OwnerId = User.Identity.GetUserId(),
                Name = model.Name,
                Description =  model.Description
            });
            foreach (var modelInvitedUserID in model.FriendIdsToInvite)
            {
                var parsed = TryParseObjectId(modelInvitedUserID);
                if (parsed.HasValue)
                {
                    invitationRepository.InviteUserToEvent(parsed.Value, User.Identity.GetUserId(), e.Id, true);
                }

            }
            return Ok();
        }

        [HttpGet]
        [Route("/mobile/finish-event")]
        public IActionResult FinishEvent([FromQuery] string eventId)
        {
            ObjectId eId;
            if (!ObjectId.TryParse(eventId, out eId))
            {
                return BadRequest();
            }
            eventRepository.CheckAndFinishEvent(User.Identity.GetUserId(),eId);
            return Ok();
        }
    }
}