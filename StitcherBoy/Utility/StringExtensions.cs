#region Arx One

// Stitcher Boy - a small library to help building post-build taks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT

#endregion

namespace StitcherBoy.Utility
{
    using System.Collections.Generic;

    /// <summary>
    /// Extensions to <see cref="string"/>
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Splits the arguments.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        public static IEnumerable<string> SplitArguments(this string s)
        {
            var lastIndex = 0;
            bool inQuote = false;
            for (int currentIndex = 0; ; currentIndex++)
            {
                if (currentIndex >= s.Length)
                {
                    if (lastIndex != currentIndex)
                        yield return s.Substring(lastIndex);
                    break;
                }
                if (char.IsWhiteSpace(s[currentIndex]) && !inQuote)
                {
                    if (lastIndex != currentIndex)
                        yield return s.Substring(lastIndex, currentIndex - lastIndex);
                    lastIndex = currentIndex + 1; // skip this space
                }
                if (s[currentIndex] == '\"')
                    inQuote = !inQuote;
            }
        }

        /// <summary>
        /// Trims the quotes, if any.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        public static string TrimQuotes(this string s)
        {
            if (s.StartsWith("\"") && s.EndsWith("\""))
                return s.Substring(1, s.Length - 2);
            return s;
        }
    }
}
