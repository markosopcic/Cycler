using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Cycler.Data.Repositories.Interfaces;
using Cycler.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Cycler.Models;
using Cycler.Views.Models;
using Microsoft.AspNetCore.Authorization;

namespace Cycler.Controllers
{

    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IUserRepository userRepository;
        public HomeController(ILogger<HomeController> logger,IUserRepository userRepository)
        {
            _logger = logger;
            this.userRepository = userRepository;

        }

        public IActionResult Index()
        {
            var activeFriends = userRepository.GetActiveFriends(User.Identity.GetUserId());
            var activeEvents = userRepository.GetActiveEvents(User.Identity.GetUserId());
            return View(new HomeViewModel{ActiveEvents =  activeEvents,ActiveFriends =  activeFriends});
        }

        public IActionResult Privacy()
        {
            return View();
        }
        
    }
}