using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Digitalist.Controllers;
using Digitalist_Data.Dto;
using Digitalist_Data.Helpers;
using Digitalist_Data.Models;
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
        public IActionResult Create([FromBody] NewListModel model)
        {
            var username = User?.Identity?.Name;
            var userResult = _userService?.GetUserForUsername(username);
            if (userResult?.Type == ResultType.Success)
            {
                var user = userResult.ReturnObj;
                var createResult = _shoppingListService?.CreateNewShoppinglist(model?.ListName, user);
                if (createResult?.Type == ResultType.Success)
                {
                    var list = createResult.ReturnObj;
                    return Ok($"List with name {list.Listname} was successfully added");
                }

                return BadRequest(createResult?.Message);
            }

            return BadRequest(userResult?.Message);
        }

        [HttpGet]
        [Authorize]
        [Route("[controller]/getAll")]
        public IActionResult GetAllListsForUser()
        {
            var username = User?.Identity?.Name;
            var userResult = _userService?.GetUserForUsername(username);
            if (userResult?.Type == ResultType.Success)
            {
                var user = userResult.ReturnObj;
                var listsResult = _shoppingListService?.GetAllListsForUser(user);
                if (listsResult?.Type == ResultType.Success)
                {
                    var retList = new List<ShoppingListDto>();
                    retList.AddRange(listsResult.ReturnObj?
                        .Select(list => new ShoppingListDto(list)));
                    return Ok(retList);
                }

                return BadRequest(listsResult?.Message);
            }

            return BadRequest(userResult?.Message);
        }

        [HttpPost]
        [Authorize]
        [Route("[controller]/addItem")]
        public IActionResult AddItemToShoppingList([FromBody] NewItemModel model)
        {
            var username = User?.Identity?.Name;
            var userResult = _userService?.GetUserForUsername(username);
            if (userResult?.Type == ResultType.Success)
            {
                var listResult = _shoppingListService?.GetListForId(model?.ListId ?? -1);
                if (listResult?.Type == ResultType.Success)
                {
                    var list = listResult.ReturnObj;
                    if (list?.UserId != userResult?.ReturnObj?.Id)
                    {
                        return Unauthorized("The List does not belong to this user");
                    }

                    var item = new ListItem()
                    {
                        IsChecked = false,
                        Itemname = model?.Itemname,
                        ShoppingListId = list.Id
                    };
                    var addResult = _shoppingListService?.AddItemToList(list.Id, item);
                    if (addResult?.Type == ResultType.Success)
                    {
                        return Ok(new ShoppingListDto(addResult.ReturnObj));
                    }

                    return BadRequest(addResult?.Message);
                }

                return BadRequest(listResult.Message);
            }

            return BadRequest(userResult?.Message);
        }

        [HttpGet]
        [Authorize]
        [Route("[controller]/get")]
        public IActionResult GetListForId(int listId)
        {
            var username = User?.Identity?.Name;
            var userResult = _userService?.GetUserForUsername(username);
            if (userResult?.Type == ResultType.Success)
            {
                var listResult = _shoppingListService.GetListForId(listId);
                if (listResult.Type == ResultType.Success)
                {
                    var list = listResult.ReturnObj;
                    if (list.UserId == userResult.ReturnObj.Id)
                    {
                        return Ok(new ShoppingListDto(list));
                    }

                    return Unauthorized("List does not belong to the User");
                }

                return BadRequest(listResult.Message);
            }
            return BadRequest(userResult.ReturnObj);
        }
        [HttpGet]
        [Authorize]
        [Route("[controller]/getAsFile")]
        public IActionResult GetListAsFile(int listId)
        {
            var username = User?.Identity?.Name;
            var userResult = _userService?.GetUserForUsername(username);
            if (userResult?.Type == ResultType.Success)
            {
                var listResult = _shoppingListService.GetListForId(listId);
                if (listResult.Type == ResultType.Success)
                {
                    var list = listResult.ReturnObj;
                    if (list.UserId == userResult.ReturnObj.Id)
                    {
                        StringBuilder builder = new StringBuilder();
                        builder.Append(list.Listname);
                        builder.Append("\n\n");
                        foreach (var item in list.ListItems)
                        {
                            builder.Append(item.Itemname);
                            builder.Append("\n");
                        }

                        var str = builder.ToString();
                        var bytes = Encoding.UTF8.GetBytes(str);
                        return File(bytes, "application/octet-stream", list.Listname + ".txt");
                    }

                    return Unauthorized("List does not belong to the User");
                }

                return BadRequest(listResult.Message);
            }
            return BadRequest(userResult.ReturnObj);
        }

        [HttpPost]
        [Authorize]
        [Route("[controller]/delete")]
        public IActionResult DeleteList([FromBody]ListModel model)
        {
            var username = User?.Identity?.Name;
            var userResult = _userService?.GetUserForUsername(username);
            if (userResult?.Type == ResultType.Success)
            {
                var deleteResult = _shoppingListService.DeleteList(model.ListId, userResult.ReturnObj);
                if (deleteResult.Type == ResultType.Success)
                {
                    return Ok(deleteResult.Message);
                }

                return BadRequest(deleteResult.Message);
            }
            return BadRequest(userResult.Message);
        }

        [HttpPost]
        [Authorize]
        [Route("[controller]/deleteItem")]
        public IActionResult DeleteItem([FromBody]DeleteItemModel model)
        {
            var username = User?.Identity?.Name;
            var userResult = _userService?.GetUserForUsername(username);
            if (userResult?.Type == ResultType.Success)
            {
                var deleteResult = _shoppingListService.DeleteItemFromList(model.ListId, userResult.ReturnObj, model.ItemId);
                if (deleteResult.Type == ResultType.Success)
                {
                    return Ok(new ShoppingListDto(deleteResult.ReturnObj));
                }
                return BadRequest(deleteResult.Message);
            }
            return BadRequest(userResult.Message);
        }

        [HttpPut]
        [Authorize]
        [Route("[controller]/changeListname")]
        public IActionResult ChangeListName([FromBody] UpdateListModel model)
        {
            var username = User?.Identity?.Name;
            var userResult = _userService?.GetUserForUsername(username);
            if (userResult?.Type == ResultType.Success)
            {
                var listResult = _shoppingListService.RenameList(model.ListId, userResult.ReturnObj, model.ListName);
                if (listResult.Type == ResultType.Success)
                {
                    return Ok(new ShoppingListDto(listResult.ReturnObj));
                }
                return BadRequest(listResult.Message);
            }
            return BadRequest(userResult.Message);
        }

        [HttpPut]
        [Authorize]
        [Route("[controller]/changeItemIsChecked")]
        public IActionResult ChangeItemIsChecked([FromBody] ChangeItemIsCheckedModel model)
        {
            var username = User?.Identity?.Name;
            var userResult = _userService?.GetUserForUsername(username);
            if (userResult?.Type == ResultType.Success)
            {
                var listResult = _shoppingListService.ChangeItemIsChecked(model.ListId, userResult.ReturnObj, model.ItemId, model.IsChecked);
                if (listResult.Type == ResultType.Success)
                {
                    return Ok(new ShoppingListDto(listResult.ReturnObj));
                }
                return BadRequest(listResult.Message);
            }
            return BadRequest(userResult.Message);
        }

        [HttpPut]
        [Authorize]
        [Route("[controller]/changeListIsFavourite")]
        public IActionResult ChangeListIsFavourite([FromBody] ChangeListIsFavouriteModel model)
        {
            var username = User?.Identity?.Name;
            var userResult = _userService?.GetUserForUsername(username);
            if (userResult?.Type == ResultType.Success)
            {
                var listResult = _shoppingListService.ChangeListIsFavourite(model.ListId, userResult.ReturnObj, model.IsFavourite);
                if (listResult.Type == ResultType.Success)
                {
                    return Ok(new ShoppingListDto(listResult.ReturnObj));
                }
                return BadRequest(listResult.Message);
            }
            return BadRequest(userResult.Message);
        }

        [HttpPut]
        [Authorize]
        [Route("[controller]/changeItemName")]
        public IActionResult ChangeItemName([FromBody] ChangeItemnameModel model)
        {
            var username = User?.Identity?.Name;
            var userResult = _userService?.GetUserForUsername(username);
            if (userResult?.Type == ResultType.Success)
            {
                var listResult = _shoppingListService.ChangeItemName(model.ListId, userResult.ReturnObj, model.ItemId, model.Itemname);
                if (listResult.Type == ResultType.Success)
                {
                    return Ok(new ShoppingListDto(listResult.ReturnObj));
                }
                return BadRequest(listResult.Message);
            }
            return BadRequest(userResult.Message);
        }

        public class NewListModel
        {
            [Required(ErrorMessage = "Listname is required")]
            public string ListName { get; set; }
        }

        public class NewItemModel
        {
            [Required(ErrorMessage = "ListId is required")]
            public int ListId { get; set; }

            [Required(ErrorMessage = "Itemname is required")]
            public string Itemname { get; set; }
        }

        public class ListModel
        {
            [Required(ErrorMessage = "ListId is required")]
            public int ListId { get; set; }
        }

        public class DeleteItemModel
        {
            [Required(ErrorMessage = "ListId is required")]
            public int ListId { get; set; }

            [Required(ErrorMessage = "ItemId is required")]
            public int ItemId { get; set; }
        }

        public class UpdateListModel
        {
            [Required(ErrorMessage = "ListId is required")]
            public int ListId { get; set; }
            [Required(ErrorMessage = "Listname is required")]
            public string ListName { get; set; }
        }

        public class ChangeItemIsCheckedModel
        {
            [Required(ErrorMessage = "ListId is required")]
            public int ListId { get; set; }
            [Required(ErrorMessage = "ItemId is required")]
            public int ItemId { get; set; }
            public bool IsChecked { get; set; }
        }
        public class ChangeListIsFavouriteModel
        {
            [Required(ErrorMessage = "ListId is required")]
            public int ListId { get; set; }
            public bool IsFavourite { get; set; }
        }
        public class ChangeItemnameModel
        {
            [Required(ErrorMessage = "ListId is required")]
            public int ListId { get; set; }
            [Required(ErrorMessage = "ItemId is required")]
            public int ItemId { get; set; }
            public string Itemname { get; set; }
        }
    }
}