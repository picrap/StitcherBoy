
namespace StitcherBoy.Logging
{
    /// <summary>
    /// Logging abstraction, because for debug purposes,
    /// this task runs either in VS tasks or in process mode
    /// </summary>
    public interface ILogging
    {
        /// <summary>
        /// Writes the specified text.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="parameters">The parameters.</param>
        void Write(string format, params object[] parameters);
        /// <summary>
        /// Writes the warning.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="parameters">The parameters.</param>
        void WriteWarning(string format, params object[] parameters);
        /// <summary>
        /// Writes the error.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="parameters">The parameters.</param>
        void WriteError(string format, params object[] parameters);
        /// <summary>
        /// Writes, debug level (does not show in release).
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="parameters">The parameters.</param>
        void WriteDebug(string format, params object[] parameters);
    }
}