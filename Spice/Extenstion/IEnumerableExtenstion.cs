using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.Extenstion
{
    public static class IEnumerableExtenstion
    {
        public static IEnumerable<SelectListItem> ToSelectListItem<T>(this IEnumerable<T> items,int selectedValue)
        {
            return from item in items
                   select new SelectListItem
                   {
                       Text = item.GetPropertValue("Name"),
                       Value = item.GetPropertValue("Id"),
                       Selected = item.GetPropertValue("Id").Equals(selectedValue.ToString())
                   };
        }
    }
}
