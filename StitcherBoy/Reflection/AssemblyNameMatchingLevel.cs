// Stitcher Boy - a small library to help building post-build tasks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT
namespace StitcherBoy.Reflection
{
    using System;

    /// <summary>
    /// Matching level for assembly compare
    /// </summary>
    [Flags]
    public enum AssemblyNameMatchingLevel
    {
        /// <summary>
        /// The name (this is always used)
        /// </summary>
        Name = 0,
        /// <summary>
        /// The version
        /// </summary>
        Version = 0x01,
        /// <summary>
        /// The signing key
        /// </summary>
        Key = 0x02,
    }
}