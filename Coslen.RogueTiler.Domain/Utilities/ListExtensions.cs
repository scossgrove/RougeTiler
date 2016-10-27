using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coslen.RogueTiler.Domain.Utilities
{
    public static class ListExtensions
    {
        public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }

        public static IList<T> Generate<T>(int length, Func<int, T> builder)
        {
            List<T> result = new List<T>();

            for(int index = 0; index < length; index++)
            {
                result.Add(builder(index));
            }

            return result;
        }
    }
}
