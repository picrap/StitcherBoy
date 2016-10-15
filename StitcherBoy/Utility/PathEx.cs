#region Arx One
// Stitcher Boy - a small library to help building post-build taks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT
#endregion

namespace StitcherBoy.Utility
{
    using System.IO;

    public static class PathEx
    {
        /// <summary>
        /// Changes the extension, given a full path, returns a related path with different extension
        /// Right, .NET path manipulation functions are POOR
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="newExtension">The new extension.</param>
        /// <returns></returns>
        public static string ChangeExtension(string path, string newExtension)
        {
            var directory = Path.GetDirectoryName(path);
            var fileName = Path.GetFileNameWithoutExtension(path);
            return Path.Combine(directory, fileName + newExtension);
        }
    }
}
