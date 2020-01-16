using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digitalist_Data.Models;

namespace Digitalist_Data.Dto
{
    public class ShoppingListDto
    {
        public int Id { get;  }
        public string Listname { get;}
        public List<ListItemDto> Items { get;  }
        public bool IsFavourite { get; }

        public ShoppingListDto(ShoppingList list)
        {
            Id = list.Id;
            Listname = list.Listname;
            IsFavourite = list.IsFavourite;
            Items = list.ListItems.Select(item => new ListItemDto(item)).ToList();
        }
    }
}
