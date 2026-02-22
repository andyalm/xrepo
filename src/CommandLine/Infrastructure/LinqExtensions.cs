using System;
using System.Collections.Generic;

namespace XRepo.CommandLine.Infrastructure
{
    static class LinqExtensions
    {
        public static void Each<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
            }
        }

        public static void EachWithIndex<T>(this IEnumerable<T> enumerable, Action<T, int> action)
        {
            int i = 0;
            foreach (var item in enumerable)
            {
                action(item, i);
                i += 1;
            }
        }
    }
}
