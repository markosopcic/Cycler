﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Cycler.Controllers.Models;
using Cycler.Data.Models;
using Cycler.Data.Repositories.Interfaces;
using Cycler.Helpers;
 using Microsoft.AspNetCore.Authentication;
 using Microsoft.AspNetCore.Authorization;

namespace Cycler.Controllers
{

    public class UserController : Controller
    {
        private IUserRepository userRepository;
        private IMapper mapper;
        private readonly AppSettings appSettings;
        public UserController(
            IUserRepository userRepository,
            IMapper mapper,
            IOptions<AppSettings> appSettings)
        {
            this.userRepository = userRepository;
            this.mapper = mapper;
            this.appSettings = appSettings.Value;
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
        
        
        
        
    }
}