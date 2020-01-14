using System;
using System.Collections.Generic;
using System.Text;
using Digitalist.Helpers;
using Digitalist_Data.Helpers;
using Digitalist_Data.Models;
using Microsoft.Extensions.Options;

namespace Digitalist_Data.Services
{
    public interface IShoppingListService
    {
        IResult<ShoppingList> CreateNewShoppinglist(string listname, User user);
    }
    public class ShoppingListService : IShoppingListService
    {
        private readonly AppSettings _appSettings;

        public ShoppingListService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }
        public IResult<ShoppingList> CreateNewShoppinglist(string listname, User user)
        {
            var newList = new ShoppingList()
            {
                Listname = listname,
                UserId = user.Id
            };
            var result = AddToDb(newList);
            return result;
        }

        private IResult<ShoppingList> AddToDb(ShoppingList list)
        {
            using var context = new ListContext();
            try
            {
                var newEntry = context.ShoppingList.Add(list);
                context.SaveChanges();
                return new Result<ShoppingList>($"List {list.Listname} successfully added", ResultType.Success, newEntry.Entity);
            }
            catch (Exception ex)
            {
                return new Result<ShoppingList>($"The List could not be added because of an error: \n {ex.Message}", ResultType.Error);
            }
        }
    }
}
