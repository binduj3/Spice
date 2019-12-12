using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.Extenstion
{
    public static class ReflectionExtenstion
    {
        public static string GetPropertValue<T>(this T item,string propertyName)
        {
            return item.GetType().GetProperty(propertyName).GetValue(item,null).ToString();
        }
    }
}
