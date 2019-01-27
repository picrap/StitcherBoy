// Stitcher Boy - a small library to help building post-build tasks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT
namespace StitcherBoy.Utility
{
    using System;
    using System.Collections.Generic;

    internal static class EnumerableExtensions
    {
        /// <summary>
        /// Applies an action to all items.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="items">The items.</param>
        /// <param name="action">The action.</param>
        public static void ForAll<TItem>(this IEnumerable<TItem> items, Action<TItem> action)
        {
            foreach (var item in items)
                action(item);
        }
    }
}
