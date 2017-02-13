using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
