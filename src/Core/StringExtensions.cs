using System;
using System.Text;

namespace XRepo.Core
{
    public static class StringExtensions
    {
        public static string ToSnakeCase(this string pascalCase, string seperator = "_")
        {
            var builder = new StringBuilder();
            builder.Append(Char.ToLowerInvariant(pascalCase[0]));
            for (int i = 1; i < pascalCase.Length; i++)
            {
                var ch = pascalCase[i];
                if (Char.IsUpper(ch))
                    builder.Append(seperator);
                builder.Append(Char.ToLowerInvariant(ch));
            }
            return builder.ToString();
        }

        public static string ToCamelCase(this string pascalCase)
        {
            var builder = new StringBuilder();
            builder.Append(Char.ToLowerInvariant(pascalCase[0]));
            for (int i = 1; i < pascalCase.Length; i++)
            {
                builder.Append(pascalCase[i]);
            }
            return builder.ToString();
        }
    }
}