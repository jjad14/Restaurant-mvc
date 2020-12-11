using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Restaurant.Extensions
{
    public static class ReflectionExtension
    {
        // The return type should be a string because we will be fetching the values of Id and Name
        // So now basically this extension method gets the value of whatever property we pass here
        public static string GetPropertyValue<T>(this T item, string propertyName)
        {
            // return the value
            return item.GetType().GetProperty(propertyName).GetValue(item, null).ToString();
        }
    }
}
