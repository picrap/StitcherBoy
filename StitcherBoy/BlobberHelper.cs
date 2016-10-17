#region Arx One
// Stitcher Boy - a small library to help building post-build taks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT
#endregion

namespace StitcherBoy
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Reflection;

    /// <summary>
    /// Helper for Blobber class
    /// </summary>
    public static class BlobberHelper
    {
        private static Type _loaderType;

        private static Type Loader
        {
            get
            {
                if (_loaderType == null)
                    _loaderType = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("\u2302")).FirstOrDefault(t => t != null);
                return _loaderType;
            }
        }

        /// <summary>
        /// Sets up Blobber resolution, because in MSBuild tasks, the static loader is not explicitly invoked.
        /// </summary>
        public static void Setup()
        {
            // this is Blobber's official method to execute setup on task assemblies
            Loader?.GetMethod("Setup").Invoke(null, new object[0]);
        }

    }
}
