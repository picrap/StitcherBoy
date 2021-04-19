#region Arx One
// Stitcher Boy - a small library to help building post-build tasks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT
#endregion

namespace StitcherBoy.Weaving
{
    using System.Collections.Generic;
    using Logging;

    /// <summary>
    /// Stitcher interface
    /// </summary>
    public interface IStitcher
    {
        /// <summary>
        /// Gets or sets the logging.
        /// </summary>
        /// <value>
        /// The logging.
        /// </value>
        ILogging Logging { get; set; }

        /// <summary>
        /// Processes the assembly based on given parameters.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="entryAssemblyPath">The entry assembly path.</param>
        /// <returns></returns>
        bool Process(IDictionary<string, string> parameters, string entryAssemblyPath);
    }
}
