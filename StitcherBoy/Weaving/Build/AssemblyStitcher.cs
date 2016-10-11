#region Arx One
// Stitcher Boy - a small library to help building post-build taks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT
#endregion

namespace StitcherBoy.Weaving.Build
{
    using System;
    using System.Collections.Specialized;
    using Logging;

    /// <summary>
    /// Assembly stitcher
    /// </summary>
    /// <seealso cref="StitcherBoy.Weaving.IStitcher" />
    public class AssemblyStitcher: IStitcher
    {
        public ILogging Logging { get; set; }

        /// <summary>
        /// Processes the assembly based on given parameters.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="buildID">The build identifier.</param>
        /// <param name="buildTime">The build time.</param>
        /// <param name="entryAssemblyPath">The entry assembly path.</param>
        /// <returns></returns>
        public bool Process(StringDictionary parameters, Guid buildID, DateTime buildTime, string entryAssemblyPath)
        {
        }
    }
}
