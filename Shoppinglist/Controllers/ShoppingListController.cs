using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Digitalist.Controllers;
using Digitalist_Data.Helpers;
using Digitalist_Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Shoppinglist.Controllers
{
    [ApiController]
    public class ShoppingListController : ControllerBase
    {

        private readonly IShoppingListService _shoppingListService;
        private readonly IUserService _userService;

        public ShoppingListController(IShoppingListService shoppingListService, IUserService userService)
        {
            _shoppingListService = shoppingListService;
            _userService = userService;
        }

        [HttpPost]
        [Authorize]
        [Route("[controller]/create")]
        public IActionResult Create([FromBody]NewListModel model)
        {
            //var user = Digitalist_Data.Models.User.CreateNew(model.Username, model.Password);
            //var result = _userService.AddToDb(user);
            //if (result.Type == ResultType.Success)
            //{
            //    return Ok(result.Message);
            //}

            //return BadRequest(result.Message);
            var username = User.Identity.Name;
            var userResult = _userService.GetUserForUsername(username);
            if (userResult.Type == ResultType.Success)
            {
                var user = userResult.ReturnObj;
                var createResult = _shoppingListService.CreateNewShoppinglist(model.ListName, user);
                if (createResult.Type == ResultType.Success)
                {
                    var list = createResult.ReturnObj;
                }
            }
            return Ok("");
        }
    }

    public class NewListModel
    {
        [Required(ErrorMessage = "Listname is required")]
        public string ListName { get; set; }
    }
}