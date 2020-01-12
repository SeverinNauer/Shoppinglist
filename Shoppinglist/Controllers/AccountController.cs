using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using Digitalist_Data.Helpers;
using Digitalist_Data.Models;
using Digitalist_Data.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Digitalist.Controllers
{
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }


        [HttpPost]
        [Route("[controller]/create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Create([FromBody]LoginModel model)
        {
            var user = new User(model.Username, model.Password);
            var result = _userService.AddToDb(user);
            if (result.Type == ResultType.Success)
            {
                return Ok(result.Message);
            }

            return BadRequest(result.Message);
        }        
        
        [HttpPost]
        [Route("[controller]/login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Login([FromBody]LoginModel model)
        {
            var result = _userService.Authenticate(model.Username, model.Password);
            if (result.Type == ResultType.Success)
            {
                return Ok("Bearer " + result.ReturnObj);
            }

            return BadRequest(result.Message);
        }
    }

    public class LoginModel
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
