using System;
using System.Collections.Generic;
using System.Security.Claims;
using AutoMapper;
using Cycler.Controllers.Models;
using Cycler.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

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
        
    }
}