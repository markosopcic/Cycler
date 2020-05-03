using System;
using System.Linq;
using Cycler.Data.Repositories.Interfaces;
using Cycler.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Cycler.Controllers
{
    [Route("/api")]
    [ApiController]
    [Authorize]
    public class ApiController : Controller
    {
        private IUserRepository userRepository { get; }
        
        public ApiController(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        [Route("search-users")]
        public IActionResult SearchUsers(string term)
        {
            if(term == null) throw new ArgumentNullException(nameof(term));
            return Json(userRepository.SearchUsers(term).Where( e => e.Id != User.Identity.GetUserId()).Select(e => new {Id = e.Id.ToString(),FullName = e.FullName,ResultType = "User"}));
        }
    }
}