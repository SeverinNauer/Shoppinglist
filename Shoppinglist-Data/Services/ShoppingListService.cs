using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digitalist.Helpers;
using Digitalist_Data.Helpers;
using Digitalist_Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Digitalist_Data.Services
{
    public interface IShoppingListService
    {
        IResult<ShoppingList> CreateNewShoppinglist(string listname, User user);
        IResult<List<ShoppingList>> GetAllListsForUser(User user);
        IResult<ShoppingList> GetListForId(int listId);
        IResult<ShoppingList> AddItemToList(int listId, ListItem item);
        IResult<object> DeleteList(int listId, User user);
        IResult<ShoppingList> DeleteItemFromList(int listId, User user, int itemId);
        IResult<ShoppingList> RenameList(int listId, User user, string listName);
        IResult<ShoppingList> ChangeItemIsChecked(int listId, User user, int itemId, bool isChecked);
        IResult<ShoppingList> ChangeItemName(int listId, User user, int itemId, string itemName);
    }

    public class ShoppingListService : IShoppingListService
    {
        private readonly AppSettings _appSettings;

        public ShoppingListService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings?.Value;
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

        public IResult<List<ShoppingList>> GetAllListsForUser(User user)
        {
            using var context = new ListContext();
            try
            {
                var lists = context?.ShoppingList?.Where(list => list.UserId == user.Id).Include(list => list.ListItems).ToList();
                return new Result<List<ShoppingList>>("Lists found", ResultType.Success, lists);
            }
            catch (Exception ex)
            {
                return new Result<List<ShoppingList>>($"An Error in getting the Lists for the user occured: \n {ex.Message}", ResultType.Error);
            }
        }

        public IResult<ShoppingList> GetListForId(int listId)
        {
            using var context = new ListContext();
            try
            {
                var list = context?.ShoppingList?.Include(list => list.ListItems).FirstOrDefault(list => list.Id == listId);
                if (list != null)
                {
                    return new Result<ShoppingList>("List found", ResultType.Success, list);
                }
                return new Result<ShoppingList>("No List for this Id was found", ResultType.Error);
            }
            catch (Exception ex)
            {
                return new Result<ShoppingList>($"An Error in getting the Lists for the user occured: \n {ex.Message}", ResultType.Error);
            }
        }
        public IResult<ShoppingList> AddItemToList(int listId, ListItem item)
        {
            using var context = new ListContext();
            try
            {
                var list = context?.ShoppingList?.FirstOrDefault(list => list.Id == listId);
                if (list != null)
                {
                    context.ListItem?.Add(item);
                    context.SaveChanges();
                    list = context?.ShoppingList?.Include(list => list.ListItems).FirstOrDefault(list => list.Id == listId);
                    return new Result<ShoppingList>("item added", ResultType.Success, list);
                }
                return new Result<ShoppingList>("No List for this Id was found", ResultType.Error);
            }
            catch (Exception ex)
            {
                return new Result<ShoppingList>($"An Error in getting the Lists for the user occured: \n {ex.Message}", ResultType.Error);
            }
        }

        public IResult<object> DeleteList(int listId, User user)
        {
            using var context = new ListContext();
            try
            {
                var list = context?.ShoppingList?.FirstOrDefault(list => list.Id == listId && list.UserId == user.Id);
                if (list != null)
                {
                    context.ShoppingList.Remove(list);
                    context.SaveChanges();
                    return new Result("list deleted", ResultType.Success);
                }
                return new Result("No List for this Id and User was found", ResultType.Error);
            }
            catch (Exception ex)
            {
                return new Result($"An Error in Deleting the List occured: \n {ex.Message}", ResultType.Error);
            }
        }
        public IResult<ShoppingList> DeleteItemFromList(int listId, User user, int itemId)
        {
            using var context = new ListContext();
            try
            {
                var list = context?.ShoppingList?.Include(list => list.ListItems).FirstOrDefault(list => list.Id == listId && list.UserId == user.Id);
                if (list != null)
                {
                    var item = list.ListItems.FirstOrDefault(item => item.Id == itemId);
                    if (item != null)
                    {
                        list.ListItems.Remove(item);
                        context.Update(list);
                        context.SaveChanges();
                        return new Result<ShoppingList>("item deleted", ResultType.Success, list);
                    }
                    return new Result<ShoppingList>("No Item with this Id on the List found", ResultType.Error);
                }
                return new Result<ShoppingList>("No List for this Id and User was found", ResultType.Error);
            }
            catch (Exception ex)
            {
                return new Result<ShoppingList>($"An Error in Deleting the Item occured: \n {ex.Message}", ResultType.Error);
            }
        }

        public IResult<ShoppingList> RenameList(int listId, User user, string listName)
        {
            using var context = new ListContext();
            try
            {
                var list = context?.ShoppingList?.Include(list => list.ListItems).FirstOrDefault(list => list.Id == listId && list.UserId == user.Id);
                if (list != null)
                {
                    list.Listname = listName;
                    context.Update(list);
                    context.SaveChanges();
                    return new Result<ShoppingList>($"item Renamed to {listName}", ResultType.Success, list);
                }
                return new Result<ShoppingList>("No List for this Id and User was found", ResultType.Error);
            }
            catch (Exception ex)
            {
                return new Result<ShoppingList>($"An Error in Renaming the List occured: \n {ex.Message}", ResultType.Error);
            }
        }

        public IResult<ShoppingList> ChangeItemIsChecked(int listId, User user, int itemId, bool isChecked)
        {
            using var context = new ListContext();
            try
            {
                var list = context?.ShoppingList?.Include(list => list.ListItems).FirstOrDefault(list => list.Id == listId && list.UserId == user.Id);
                if (list != null)
                {
                    var item = list.ListItems.FirstOrDefault(xItem => xItem.Id == itemId);
                    if (item == null)
                    {
                        return new Result<ShoppingList>("No Item with this Id was found", ResultType.Error);
                    }
                    item.IsChecked = isChecked;
                    context.Update(item);
                    context.SaveChanges();
                    return new Result<ShoppingList>($"item checked state was set to {isChecked}", ResultType.Success, list);
                }
                return new Result<ShoppingList>("No List for this Id and User was found", ResultType.Error);
            }
            catch (Exception ex)
            {
                return new Result<ShoppingList>($"An Error in checking the state to {isChecked} occured: \n {ex.Message}", ResultType.Error);
            }
        }

        public IResult<ShoppingList> ChangeItemName(int listId, User user, int itemId, string itemName)
        {
            using var context = new ListContext();
            try
            {
                var list = context?.ShoppingList?.Include(list => list.ListItems).FirstOrDefault(list => list.Id == listId && list.UserId == user.Id);
                if (list != null)
                {
                    var item = list.ListItems.FirstOrDefault(xItem => xItem.Id == itemId);
                    if (item == null)
                    {
                        return new Result<ShoppingList>("No Item with this Id was found", ResultType.Error);
                    }
                    item.Itemname = itemName;
                    context.Update(item);
                    context.SaveChanges();
                    return new Result<ShoppingList>($"itemname was changed to {itemName}", ResultType.Success, list);
                }
                return new Result<ShoppingList>("No List for this Id and User was found", ResultType.Error);
            }
            catch (Exception ex)
            {
                return new Result<ShoppingList>($"An Error in renaming the item occured: \n {ex.Message}", ResultType.Error);
            }
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
