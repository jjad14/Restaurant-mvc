using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Restaurant.Extensions
{
    public static class IEnumerableExtension
    {
        // We will be getting an object of all of the categories.
        // We want to return a list or IEnumerable of select list item so we can use that in dropdown.
        public static IEnumerable<SelectListItem> ToSelectListItem<T>(this IEnumerable<T> items, int selectedValue)
        {
            // what we are doing is we are fetching an object and based on that object if it has an I.D.
            // and name property we'll convert that and return a select list item to display as a dropdown.
            return from item in items
                   select new SelectListItem
                   {
                       Text = item.GetPropertyValue("Name"),
                       Value = item.GetPropertyValue("Id"),
                       Selected = item.GetPropertyValue("Id").Equals(selectedValue.ToString())
                   };
        }
    }
}
