using System.Collections.Generic;

namespace Coslen.RogueTiler.Domain.Engine.Common
{
    public static class Extensions
    {
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }
    }
}