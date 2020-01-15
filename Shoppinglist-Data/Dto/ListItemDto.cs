using System;
using System.Collections.Generic;
using System.Text;
using Digitalist_Data.Models;

namespace Digitalist_Data.Dto
{
    public class ListItemDto
    {
        public int Id { get;  }
        public string Itemname { get;  }
        public bool IsChecked { get; }

        public ListItemDto(ListItem item)
        {
            Id = item.Id;
            Itemname = item.Itemname;
            IsChecked = item.IsChecked;
        }
    }
}
