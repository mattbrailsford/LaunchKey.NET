using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaunchKey.Extensions
{
    internal static class StringExtensions
    {
        public static string StripNoneBase64Chars(this string input)
        {
            return input.Replace("\r", "")
                .Replace("\n", "")
                .Replace("\\", "");
        }
    }
}
