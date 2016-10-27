using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coslen.RogueTiler.Domain.Utilities
{
    public static class DictionaryExtensions
    {
        public static Dictionary<T, List<T>> Generate<T>(int length, Func<int, T> builder)
        {
            Dictionary<T, List<T>> result = new Dictionary<T, List<T>>();
            
            for(int index = 0; index < length; index++)
            {
                result.Add(builder(index), new List<T>());
            }

            return result;
        }
    }
}
