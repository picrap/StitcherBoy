// Stitcher Boy - a small library to help building post-build taks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT
namespace StitcherBoy.Logging
{
    using System;

    /// <summary>
    /// Very basic console logging
    /// </summary>
    public class ConsoleLogging : ILogging
    {
        /// <summary>
        /// Writes the specified text.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="parameters">The parameters.</param>
        public void Write(string format, params object[] parameters)
        {
            Console.WriteLine(format, parameters);
        }

        /// <summary>
        /// Writes the warning.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="parameters">The parameters.</param>
        public void WriteWarning(string format, params object[] parameters)
        {
            Write("! " + format, parameters);
        }

        /// <summary>
        /// Writes the error.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="parameters">The parameters.</param>
        public void WriteError(string format, params object[] parameters)
        {
            Write("* " + format, parameters);
        }

        /// <summary>
        /// Writes, debug level (does not show in release).
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="parameters">The parameters.</param>
        public void WriteDebug(string format, params object[] parameters)
        {
#if DEBUG
            Write(". " + format, parameters);
#endif
        }
    }
}